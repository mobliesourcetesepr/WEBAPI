using Microsoft.AspNetCore.Mvc;
using MultiTenantAPI.Models;
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

        // Simulated in-memory user store
        private static Dictionary<string, string> _registeredUsers = new();


        // 🆕 CREATE USER
        [HttpPost("create-user")]
        public IActionResult CreateUser([FromHeader(Name = "X-Tenant-ID")] string tenantId, [FromBody] User request)
        {
            if (string.IsNullOrEmpty(tenantId))
                return BadRequest("Tenant ID header is required.");

            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                return BadRequest("Username and Password are required.");

            string userKey = $"{tenantId}:{request.Username}";

            if (_registeredUsers.ContainsKey(userKey))
                return Conflict("User already exists.");

            _registeredUsers[userKey] = request.Password;

            return Ok(new
            {
                Message = "User created successfully.",
                Username = request.Username,
                Tenant = tenantId
            });
        }



        [HttpPost("login")]
        public IActionResult Login([FromHeader(Name = "X-Tenant-ID")] string tenantId, [FromBody] User request)
        {
            if (string.IsNullOrEmpty(tenantId))
                return BadRequest("Tenant ID header is required.");
            // 1. Encrypt the original JSON (Username + Password)
            string encryptedPayload = AesEncryption.Encrypt(JsonSerializer.Serialize(request));
            // 2. Decrypt the payload back to verify and use it
            string decryptedJson = AesEncryption.Decrypt(encryptedPayload);
            // 3. Deserialize decrypted JSON to get credentials
            var login = JsonSerializer.Deserialize<User>(decryptedJson);
            // Simple static validation (replace with your own logic)



            string userKey = $"{tenantId}:{login.Username}";
            if (login != null && _registeredUsers.TryGetValue(userKey, out var correctPassword) && correctPassword == login.Password)
            {
                var sessionInfo = $"{login.Username}|{tenantId}|{DateTime.UtcNow.AddHours(1):O}";
                var token = AesEncryption.Encrypt(sessionInfo);

                HttpContext.Session.SetString("Token", token);

                return Ok(new { Token = token });
            }



            
            // if (login.Username == "user" && login.Password == "password")
            // {
            //     var sessionInfo = $"{login.Username}|{tenantId}|{DateTime.UtcNow.AddHours(1):O}";
            //     var token = AesEncryption.Encrypt(sessionInfo);

            //     HttpContext.Session.SetString("Token", token);

            //     return Ok(new { Token = token });
            // }

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

    // public class LoginRequest
    // {
    //     public string Username { get; set; }
    //     public string Password { get; set; }
    // }
}
