// Middleware/AuthTokenMiddleware.cs
using Microsoft.AspNetCore.Http;
using MultiTenantAPI.Helpers;
using MultiTenantAPI.Middleware;
public class AuthTokenMiddleware
{
    private readonly RequestDelegate _next;

    public AuthTokenMiddleware(RequestDelegate next)
    {
        _next = next;
    }

public async Task InvokeAsync(HttpContext context)
        {
            AuthHelper.ExtractAuthInfo(context);
            await _next(context);
        }
}
