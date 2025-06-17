using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiTenantAPI.Data;
using MultiTenantAPI.Models;
[Route("oauth")]
[ApiController]
public class TokenController : ControllerBase
{
    private readonly TokenService _tokenService;
    private readonly UserDbContext _context;

    public TokenController(TokenService tokenService, UserDbContext context)
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
}
