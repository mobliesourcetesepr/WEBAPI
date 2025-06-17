using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MultiTenantAPI.Data;
using MultiTenantAPI.Models;
[Route("oauth")]
[ApiController]
public class TokenController : ControllerBase
{
    private readonly TokenService _tokenService;
    private readonly UserDbContext _context;
    private readonly IConfiguration _config;

    public TokenController(TokenService tokenService, UserDbContext context, IConfiguration config)
    {
        _tokenService = tokenService;
        _context = context;
        _config = config;
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
    //  [HttpGet("protected")]
    //     [Authorize] // <- Requires valid JWT token in Authorization header
    //     public IActionResult ProtectedApi([FromHeader(Name = "X-Bearer-Token")] string Token)
    //     {
    //         var username = User.FindFirstValue(ClaimTypes.Name);
    //         var role = User.FindFirstValue(ClaimTypes.Role);

    //         if (string.IsNullOrEmpty(username))
    //             return Unauthorized("Invalid token");

    //         return Ok($"✅ Welcome {username} (Role: {role}) - You have accessed a protected API!");
    //     }

    [HttpGet("protected")]
    public IActionResult ProtectedApi([FromHeader(Name = "X-Bearer-Token")] string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Unauthorized("Token missing.");

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _config["Jwt:Issuer"],
            ValidAudience = _config["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]))
        };

        try
        {
            var handler = new JwtSecurityTokenHandler();
            handler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var username = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var role = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var exp = jwtToken.ValidTo;

            return Ok($"✅ Valid token.\n👤 User: {username}\n🔑 Role: {role}\n⏰ Expires At (UTC): {exp}");
        }
        catch (SecurityTokenException ex)
        {
            return Unauthorized($"Invalid token: {ex.Message}");
        }
        catch (Exception)
        {
            return Unauthorized("Invalid or expired token.");
        }
    }
}
