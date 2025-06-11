using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace MultiTenantAPI.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue("X-Tenant-ID", out var tenantId))
            {
                context.Items["TenantId"] = tenantId.ToString();
            }
            else
            {
                context.Items["TenantId"] = "default"; // fallback
            }

            await _next(context);
        }
    }
}

