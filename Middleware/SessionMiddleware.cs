using Microsoft.EntityFrameworkCore;
using MultiTenantAPI.Data;
using MultiTenantAPI.Models;
using Microsoft.Extensions.Caching.Memory;

public class SessionMiddleware
{
private readonly RequestDelegate _next;
    private readonly UserDbContext _context;
    private readonly MyLogDbContext _logContext;
    private readonly IMemoryCache _cache;


    public SessionMiddleware(RequestDelegate next, UserDbContext context, MyLogDbContext logContext,IMemoryCache cache)
    {
        _next = next;
        _context = context;
        _logContext = logContext;
        _cache = cache;
    }
    public async Task Invoke(HttpContext context)
    {
        var token = context.Request.Headers["Token"].FirstOrDefault();
        var tenantId = context.Request.Headers["TenantId"].FirstOrDefault();

        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(tenantId))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Token and TenantId are required.");
            return;
        }

        var session = await _context.SessionStores
            .FirstOrDefaultAsync(s => s.Token == token && s.TenantId == tenantId);

        if (session == null || !session.IsActive)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Session not found or inactive.");
            return;
        }

        // Check if session is expired by inactivity
        var now = DateTime.Now;
        var inactivityDuration = now - session.LastAccessedAt;

        if (inactivityDuration.TotalMinutes > 10)
        {
            session.IsActive = false;
            await _context.SaveChangesAsync();

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Session expired due to inactivity.");
            return;
        }

        // Session is still valid — update LastAccessedAt
        session.LastAccessedAt = now;
        await _context.SaveChangesAsync();

        // You can pass user info to downstream code
        context.Items["UserId"] = session.UserId;
        context.Items["TenantId"] = session.TenantId;

        await _next(context);
    }
public async Task TokenInvoke(HttpContext context)
{
    var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");
    if (!string.IsNullOrEmpty(token))
    {
        var now = DateTime.Now;

        // 🔹 Check cache first
        if (_cache.TryGetValue(token, out DateTime lastUsed))
        {
            if ((now - lastUsed) <= TimeSpan.FromMinutes(3))
            {
                // ✅ Extend cache expiration
                _cache.Set(token, now, TimeSpan.FromMinutes(3));

                // ✅ Update DB session
                var session = _context.SessionStores.FirstOrDefault(s => s.Token == token && s.IsActive);
                if (session != null)
                {
                    session.LastAccessedAt = now;
                    session.ExpiresAt = now.AddMinutes(3);
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                // ❌ Expired in cache — deactivate session
                await ExpireSessionAndReject(token, context);
                return;
            }
        }
        else
        {
            // 🔹 Cache miss: check database
            var session = _context.SessionStores.FirstOrDefault(s => s.Token == token && s.IsActive);
            if (session != null)
            {
                if ((now - session.LastAccessedAt) <= TimeSpan.FromMinutes(3))
                {
                    // ✅ Refresh session and re-cache
                    session.LastAccessedAt = now;
                    session.ExpiresAt = now.AddMinutes(3);
                    await _context.SaveChangesAsync();

                    _cache.Set(token, now, TimeSpan.FromMinutes(3));
                }
                else
                {
                    await ExpireSessionAndReject(token, context);
                    return;
                }
            }
        }
    }

    await _next(context);
}

private async Task ExpireSessionAndReject(string token, HttpContext context)
{
    var session = _context.SessionStores.FirstOrDefault(s => s.Token == token && s.IsActive);
    if (session != null)
    {
        session.IsActive = false;
        await _context.SaveChangesAsync();
    }

    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
    await context.Response.WriteAsync("Token expired due to inactivity.");
}


}
