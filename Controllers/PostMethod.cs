using Microsoft.AspNetCore.Mvc;
using MultiTenantAPI.Models;
using Microsoft.AspNetCore.Http;
using MultiTenantAPI.Helpers;
using MultiTenantAPI.Data;
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

        private readonly UserDbContext _context;

        public AuthController(UserDbContext context)
        {
            _context = context;
        }

        // Simulated in-memory user store
        //private static Dictionary<string, string> _registeredUsers = new();


        // 🆕 CREATE USER
        [HttpPost("create-user")]
        public IActionResult CreateUser([FromHeader(Name = "X-Tenant-ID")] string tenantId, [FromBody] Admin admin)
        {
            if (string.IsNullOrEmpty(tenantId))
                return BadRequest("Tenant ID header is required.");
            // ✅ Encrypt the incoming request (simulate secure transmission)
            string encryptedPayload = AesEncryption.Encrypt(JsonSerializer.Serialize(admin));
        
            // ✅ Decrypt the payload back to original
            string decryptedJson = AesEncryption.Decrypt(encryptedPayload);
        
            // ✅ Deserialize to User object
            var user = JsonSerializer.Deserialize<Admin>(decryptedJson);

            if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password)|| string.IsNullOrEmpty(user.Email))
                return BadRequest("Username, Password, and Email are required.");
            admin.TenantId = tenantId;


            var exists = _context.Admins.Any(u => u.TenantId == tenantId && u.Username == user.Username);
            if (exists)
                return Conflict("User already exists.");
            _context.Admins.Add(admin);
            _context.SaveChanges();

            // string userKey = $"{tenantId}:{user.Username}";

            // if (_registeredUsers.ContainsKey(userKey))
            //     return Conflict("User already exists.");

            // _registeredUsers[userKey] = user.Password;
            //return Ok(new { Message = "Admin created successfully.", admin.Username, Tenant = tenantId });
            return Ok(new
            {
                Message = "Admin created successfully.",
                Username = admin.Username,
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

            var user = _context.Admins.FirstOrDefault(u =>
            u.Username == login.Username &&
            u.Password ==  login.Password);

        HttpContext.Session.SetString("Email", user.Email);
        HttpContext.Session.SetString("Username", user.Username);
        HttpContext.Session.SetString("TenantId", user.TenantId);
        

            if (login.Username == user.Username && login.Password == user.Password)
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
            var username = HttpContext.Session.GetString("Username");
            var tenantId = HttpContext.Session.GetString("TenantId");
            var email = HttpContext.Session.GetString("Email");
            // if (string.IsNullOrEmpty(token))
            //     return BadRequest("No token in session. Please login.");

            // if (_sessionService.ValidateSession(token, out var username, out var tenantId))
            // {
                return Ok(new { TenantId = tenantId, Username = username, Email = email, Token = token });
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
