using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Http;
using MultiTenantAPI.Helpers;
using System.Text.Json;
namespace MultiTenantAPI.Controllers
{
    [ApiController]
    [Route("api/secure")]
    public class AuthController : ControllerBase
    {
        // private readonly SessionService _sessionService;

        // public AuthController(SessionService sessionService)
        // {
        //     _sessionService = sessionService;
        // }

     [HttpPost("login")]
        public IActionResult Login([FromHeader(Name = "X-Tenant-ID")] string tenantId, [FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(tenantId))
                return BadRequest("Tenant ID header is required.");

            // Simple static validation (replace with your own logic)
            if (request.Username == "user" && request.Password == "password")
            {
                var sessionInfo = $"{request.Username}|{tenantId}|{DateTime.UtcNow.AddHours(1):O}";
                var token = AesEncryption.Encrypt(sessionInfo);

                HttpContext.Session.SetString("Token", token);

                return Ok(new { Token = token });
            }

            return Unauthorized("Invalid credentials.");
        }


        [HttpGet("validate")]
        public IActionResult Validate()
        {
            // 🟢 Retrieve from session
            var token = HttpContext.Session.GetString("Token");
            // if (string.IsNullOrEmpty(token))
            //     return BadRequest("No token in session. Please login.");

            // if (_sessionService.ValidateSession(token, out var username, out var tenantId))
            // {
                return Ok(new { TenantId = token });
           // }

           // return Unauthorized("Invalid or expired session.");
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
