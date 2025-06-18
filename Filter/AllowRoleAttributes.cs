using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MultiTenantAPI.Helpers;
using MultiTenantAPI.Models;

namespace MultiTenantAPI.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AllowRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _requiredRole;

        public AllowRoleAttribute(string requiredRole)
        {
            _requiredRole = requiredRole;
        }

  public void OnAuthorization(AuthorizationFilterContext context)
    {
        var token = context.HttpContext.Request.Headers["X-Auth-Token"].FirstOrDefault();
        Console.WriteLine("Token: " + token);
        if (string.IsNullOrEmpty(token))
        {
            context.Result = new UnauthorizedResult(); // Changed
            return;
        }

        try
        {
            var decrypted = AesEncryption.Decrypt(token);
            var payload = JsonSerializer.Deserialize<AuthPayload>(decrypted);
            Console.WriteLine("Token: " +payload);
            if (payload == null || payload.Role != _requiredRole)
            {
                context.Result = new ContentResult
                {
                    StatusCode = 403,
                    Content = "Forbidden: You do not have the permission to access this resource."
                };
            }
        }
        catch
        {
            context.Result = new UnauthorizedResult(); // Changed
        }
    }
    }
}