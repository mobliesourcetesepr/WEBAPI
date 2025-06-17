using Microsoft.EntityFrameworkCore;
using MultiTenantAPI.Data;
using MultiTenantAPI.Models;

public class SessionMiddleware
{
private readonly RequestDelegate _next;
    private readonly UserDbContext _context;
    private readonly MyLogDbContext _logContext;

    public SessionMiddleware(RequestDelegate next, UserDbContext context, MyLogDbContext logContext)
    {
        _next = next;
        _context = context;
        _logContext = logContext;
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
}
