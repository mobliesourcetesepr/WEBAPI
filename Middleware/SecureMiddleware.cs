using Microsoft.AspNetCore.Http;
using MultiTenantAPI.Helpers;
using System;
using System.Threading.Tasks;

namespace MultiTenantAPI.Middleware
{
    public class SessionAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var path = context.Request.Path.ToString().ToLower();
            Console.WriteLine("Requested Path: " + path);
            // Allow anonymous access to login, logout, swagger endpoints
if (context.Request.Path.StartsWithSegments("/api/secure/login") ||
    context.Request.Path.StartsWithSegments("/api/secure/validate") ||
    context.Request.Path.StartsWithSegments("/swagger"))
{
    await _next(context);
    return;
}

            var token = context.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized: No token found.");
                return;
            }

            try
            {
                var decrypted = AesEncryption.Decrypt(token);
                var parts = decrypted.Split('|');

                if (parts.Length != 3 || !DateTime.TryParse(parts[2], out var expiry))
                {
                    context.Session.Remove("Token");
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Unauthorized: Invalid token format.");
                    return;
                }

                if (expiry < DateTime.UtcNow)
                {
                    context.Session.Remove("Token");
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Unauthorized: Token expired.");
                    return;
                }

                context.Items["Username"] = parts[0];
                context.Items["TenantId"] = parts[1];

                await _next(context);
            }
            catch
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized: Token decryption failed.");
            }
        }
    }
}
