using Microsoft.AspNetCore.Mvc;
using MultiTenantAPI.Models;
using Microsoft.AspNetCore.Http;
using MultiTenantAPI.Helpers;
using MultiTenantAPI.Data;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
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


        // 🆕 CREATE USER
        [HttpPost("create-user")]
        public IActionResult CreateUser([FromHeader(Name = "X-Tenant-ID")] string tenantId, [FromBody] Admin admin)
        {
            string XmlData = string.Empty;
            try
            {

                if (string.IsNullOrEmpty(tenantId))
                    return BadRequest("Tenant ID header is required.");

                // ✅ Encrypt the incoming request (simulate secure transmission)
                string encryptedPayload = AesEncryption.Encrypt(JsonSerializer.Serialize(admin));

                // ✅ Decrypt the payload back to original
                string ReqTime = DateTime.Now.ToString();
                string decryptedJson = AesEncryption.Decrypt(encryptedPayload);
                XmlData += "<Event><Data><RQ>" + decryptedJson + "</RQ></Data></Event>";
                XmlData += "<ReqTime>" + ReqTime + "</ReqTime>";

                // ✅ Deserialize to User object
                var user = JsonSerializer.Deserialize<Admin>(decryptedJson);

                if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password) || string.IsNullOrEmpty(user.Email))
                    return BadRequest("Username, Password, and Email are required.");
                admin.TenantId = tenantId;

                XmlData += "<RESPONSE>";
                var exists = _context.Admins.Any(u => u.TenantId == tenantId && u.Username == user.Username && u.Email == user.Email);
                XmlData+="<Data>"+exists+"</Data>";
                if (exists)
                    return Conflict("User already exists.");
                string ResTime = DateTime.Now.ToString();
                XmlData += "<ResTime>" + ResTime + "</ResTime>";
                _context.Admins.Add(admin);
                _context.SaveChanges();
                XmlData += "</RESPONSE>";
                XmlData += "</Event>";
                Console.WriteLine(XmlData);
               Logger.LogData(HttpContext.RequestServices,"E", "PostMethod", "CreateUser", XmlData);
                return Ok(new
                {
                    Message = "Admin created successfully.",
                    Username = admin.Username,
                    Tenant = tenantId
                });
            }
            catch (Exception ex)
            {
                XmlData += "<Exception>" + ex.Message.ToString() + "</Exception></RESPONSE>";
                // Log error
            Logger.LogData(HttpContext.RequestServices,"EX","PostMethod", "CreateUser", XmlData);
                return StatusCode(500, "An error occurred while creating the user.");
            }
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
                Console.WriteLine("Requested Path: " + login.Username, login.Password);
                // Simple static validation (replace with your own logic)

            var user = _context.Admins.FirstOrDefault(u =>
            u.Username == login.Username &&
            u.Password == login.Password);
            if (user == null)
            {
                return Unauthorized("Invalid credentials.");
            }
            HttpContext.Session.SetString("Email", user.Email);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("TenantId", user.TenantId);

            var sessionInfo = $"{login.Username}|{tenantId}|{DateTime.UtcNow.AddHours(1):O}";
            var token = AesEncryption.Encrypt(sessionInfo);
            HttpContext.Session.SetString("Token", token);

            return Ok(new { Token = token });
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

}
