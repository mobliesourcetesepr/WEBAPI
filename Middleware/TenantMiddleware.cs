using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace MultiTenantAPI.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _config;

        public TenantMiddleware(RequestDelegate next, IConfiguration config)
        {
            _next = next;
            _config = config;
        }

        public async Task Invoke(HttpContext context)
        {
            // var tenantId = context.Request.Headers["X-Tenant-ID"].ToString();
            // if (string.IsNullOrEmpty(tenantId))
            // {
            //     context.Response.StatusCode = 400;
            //     await context.Response.WriteAsync("Tenant ID is missing");
            //     return;
            // } 

            // var tenantSection = _config.GetSection($"Tenants:{tenantId}");
            // if (!tenantSection.Exists())
            // {
            //     context.Response.StatusCode = 400;
            //     await context.Response.WriteAsync("Invalid Tenant ID");
            //     return;
            // }

            // context.Items["TenantId"] = tenantId;
            // await _next(context);

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

