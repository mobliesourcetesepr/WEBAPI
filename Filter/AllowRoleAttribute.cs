using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class AllowRoleAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _allowedRoles;
        //private readonly IConfiguration _config;
    public AllowRoleAttribute( params string[]  role)
    {
        _allowedRoles = role;
        //_config = config;
    }


    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var token = context.HttpContext.Request.Headers["X-Bearer-Token"].FirstOrDefault();
        Console.WriteLine("Token: " + token);

        if (string.IsNullOrEmpty(token))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("4d6f6f727468795f5365637265745f4a574b"); // Must match appsettings.json Jwt:Key

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = "AgentCreationAuthServer",
                ValidAudience = "AgentCreationClients",
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var role = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            Console.WriteLine("Role: " + role);
            Console.WriteLine("Role: " + _allowedRoles);
            if (!_allowedRoles.Contains(role))
            {
                context.Result = new ContentResult
                {
                    StatusCode = 403,
                    Content = "Forbidden: You do not have the permission to access this resource."
                };
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine("JWT validation error: " + ex.Message);
            context.Result = new UnauthorizedResult();
        }
    }
}
