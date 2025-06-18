using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MultiTenantAPI.Data;
using MultiTenantAPI.Models;
using Microsoft.Extensions.Caching.Memory;
using MultiTenantAPI.Services;
using MultiTenantAPI.Filters;
using MultiTenantApi.Attributes;
[Route("oauth")]
[ApiController]
public class TokenController : ControllerBase
{
    private readonly TokenService _tokenService;
    private readonly UserDbContext _context;

    public TokenController(TokenService tokenService, UserDbContext context  )
    {
        _tokenService = tokenService;
        _context = context;

    }


   [HttpPost("token")]
    public IActionResult GetToken([FromBody] OAuthRequest request)
    {
        // Simulate user validation (replace with real DB check)
        var user = _context.AdminUser
        .FirstOrDefault(u => u.Username == request.Username && u.Password == request.Password);

        if (user == null)
        {
            return Unauthorized("Invalid credentials.");
        }

        var token = _tokenService.GenerateToken(user.Username, user.Role);
        return Ok(new { access_token = token, token_type = "bearer", expires_at = _tokenService.TokenExpiry.ToString("o") });
    }

//       [HttpGet("dashboard")]
// [AllowRole("Admin")]
// public IActionResult Dashboard([FromHeader(Name = "X-Bearer-Token")] string token)
// {
//     if (string.IsNullOrEmpty(token))
//         return BadRequest("Token is required.");

//     var tokenHandler = new JwtSecurityTokenHandler();
//     var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);

//     try
//     {
//         // ✅ Strict JWT token validation
//         tokenHandler.ValidateToken(token, new TokenValidationParameters
//         {
//             ValidateIssuer = true,
//             ValidateAudience = true,
//             ValidateLifetime = true,
//             RequireExpirationTime = true,
//             ValidateIssuerSigningKey = true,
//             ClockSkew = TimeSpan.Zero,
//             ValidIssuer = _config["Jwt:Issuer"],
//             ValidAudience = _config["Jwt:Audience"],
//             IssuerSigningKey = new SymmetricSecurityKey(key)
//         }, out SecurityToken validatedToken);

//         // Optional: Extract claims if needed
//         var jwtToken = (JwtSecurityToken)validatedToken;
//         var username = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
//         var role = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

//         // Simulated dashboard response
//         var dashboardData = new
//         {
//             Message = $"Welcome, {username}",
//             Role = role,
//             TotalUsers = 105,
//             ActiveSubAgents = 18,
//             ReportsGeneratedToday = 7,
//             ServerTime = DateTime.UtcNow
//         };

//         return Ok(dashboardData);
//     }
//     catch (SecurityTokenException ex)
//     {
//         return Unauthorized($"Invalid or expired token: {ex.Message}");
//     }
//     catch (Exception)
//     {
//         return Unauthorized("Token validation failed.");
//     }
// }


   

// [HttpPost("login")]
// public async Task<IActionResult> Login([FromHeader(Name = "X-Bearer-Token")] string token)
// {
//     if (string.IsNullOrEmpty(token))
//         return BadRequest("Token is required.");

//         var tokenHandler = new JwtSecurityTokenHandler();
//     var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);

//     try
//     {
//         // ✅ Validate token strictly (no clock skew)
//         tokenHandler.ValidateToken(token, new TokenValidationParameters
//         {
//             ValidateIssuer = true,
//             ValidateAudience = true,
//             ValidateLifetime = true,
//             RequireExpirationTime = true,
//             ValidateIssuerSigningKey = true,
//             ClockSkew = TimeSpan.Zero,
//             ValidIssuer = _config["Jwt:Issuer"],
//             ValidAudience = _config["Jwt:Audience"],
//             IssuerSigningKey = new SymmetricSecurityKey(key)
//         }, out SecurityToken validatedToken);

//         var jwtToken = (JwtSecurityToken)validatedToken;
//         var username = jwtToken.Claims.First(c => c.Type == ClaimTypes.Name).Value;
//         var role = jwtToken.Claims.First(c => c.Type == ClaimTypes.Role).Value;

//         // ✅ Lookup user
//         var user = _context.AdminUser.FirstOrDefault(u => u.Username == username && u.Role == role);
//         if (user == null)
//             return Unauthorized("User not found.");

//         // ✅ Store sliding-expiring session in DB
//         var session = new SessionStore
//         {
//             UserId = user.AdminId,
//             Token = token,
//             Username = username,
//             IssuedAt = DateTime.Now,
//             LastAccessedAt = DateTime.Now,
//             ExpiresAt = DateTime.Now.AddMinutes(3), // sliding expiry window
//             IsActive = true
//         };

//         _context.SessionStores.Add(session);
//         await _context.SaveChangesAsync();

//         // ✅ Also cache token to enable middleware sliding extension
//         _cache.Set(token, DateTime.Now, TimeSpan.FromMinutes(3));

//         return Ok(new
//         {
//             Message = "Session started successfully.",
//             Username = username,
//             Role = role,
//             ExpiresAt = session.ExpiresAt.ToString("o")
//         });
//     }
//     catch (SecurityTokenException)
//     {
//         return Unauthorized("Invalid or expired token.");
//     }
// }

// [SwaggerIgnore]
// [HttpGet("protected")]
// public async Task<IActionResult> ProtectedApi([FromHeader(Name = "X-Bearer-Token")] string token)
// {
//     if (string.IsNullOrWhiteSpace(token))
//         return Unauthorized("Token missing.");

//     var validationParameters = new TokenValidationParameters
//     {
//         ValidateIssuer = true,
//         ValidateAudience = true,
//         ValidateLifetime = true,
//         RequireExpirationTime = true,
//         ValidateIssuerSigningKey = true,
//         ClockSkew = TimeSpan.Zero,
//         ValidIssuer = _config["Jwt:Issuer"],
//         ValidAudience = _config["Jwt:Audience"],
//         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]))
//     };

//     try
//     {
//         var handler = new JwtSecurityTokenHandler();
//         handler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

//         var jwtToken = (JwtSecurityToken)validatedToken;
//         var username = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
//         var role = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

//         // ✅ Check session token in DB
//         var session =  _context.SessionStores
//             .FirstOrDefault(s => s.Token == token && s.IsActive);

//         if (session == null)
//             return Unauthorized("Session not found or already expired.");

//         if (session.ExpiresAt < DateTime.Now)
//         {
//             // ❌ Mark session inactive if expired
//             session.IsActive = false;
//             await _context.SaveChangesAsync();
//             return Unauthorized("Session expired due to inactivity.");
//         }

//         return Ok(new
//         {
//             Message = "✅ Token and session are valid.",
//             Username = username,
//             Role = role,
//             ExpiresAt = session.ExpiresAt.ToString("o")
//         });
//     }
//     catch (SecurityTokenException ex)
//     {
//         return Unauthorized($"Invalid token: {ex.Message}");
//     }
//     catch (Exception)
//     {
//         return Unauthorized("Invalid or expired token.");
//     }
// }

}
