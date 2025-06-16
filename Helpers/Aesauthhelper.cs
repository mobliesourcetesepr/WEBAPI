// AesAuthMiddleware.cs
using MultiTenantAPI.Models;
using System.Text.Json;

namespace MultiTenantAPI.Helpers
{
    public static class AuthHelper
    {
        public static void ExtractAuthInfo(HttpContext context)
        {
            var token = context.Request.Headers["X-Auth-Token"].FirstOrDefault();

            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var json = AesEncryption.Decrypt(token);
                    var payload = JsonSerializer.Deserialize<AuthPayload>(json);
                    Console.WriteLine("Extracted Payload: " + JsonSerializer.Serialize(payload));
                    if (payload != null)
                    {
                        context.Items["UserRole"] = payload.Role;
                        context.Items["Username"] = payload.Username;
                    }
                }
                catch
                {
                    // Optionally log or skip invalid token
                }
            }
        }
    }
}
