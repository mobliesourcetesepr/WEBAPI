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

        [HttpPost("register-admin")]
        public IActionResult RegisterAdmin(AdminUser admin)
        {
            string XmlData = string.Empty;
            try
            {

                string encryptedPayload = AesEncryption.Encrypt(JsonSerializer.Serialize(admin));

                // ✅ Decrypt the payload back to original
                string ReqTime = DateTime.Now.ToString();
                string decryptedJson = AesEncryption.Decrypt(encryptedPayload);
                XmlData += "<Event><Data><RQ>" + decryptedJson + "</RQ></Data></Event>";
                XmlData += "<ReqTime>" + ReqTime + "</ReqTime>";

                // ✅ Deserialize to User object
                var user = JsonSerializer.Deserialize<Admin>(decryptedJson);

                if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password))
                    return BadRequest("Username, Password, and Email are required.");
                XmlData += "<RESPONSE>";
                var exists = _context.Admins.Any(u => u.Username == user.Username);
                XmlData += "<Data>" + exists + "</Data>";
                if (exists)
                    return Conflict("User already exists.");
                string ResTime = DateTime.Now.ToString();
                XmlData += "<ResTime>" + ResTime + "</ResTime>";
                admin.AdminId = $"{_context.AdminUser.Count() + 1}A";
                _context.AdminUser.Add(admin);
                _context.SaveChanges();
                XmlData += "</RESPONSE>";
                XmlData += "</Event>";
                Logger.LogData(HttpContext.RequestServices, "E", "PostMethod", "CreateUser", XmlData);
                return Ok(new
                {
                    Message = "Admin created successfully.",

                });
            }
            catch (Exception ex)
            {
                XmlData += "<Exception>" + ex.Message.ToString() + "</Exception></RESPONSE>";
                // Log error
                Logger.LogData(HttpContext.RequestServices, "EX", "PostMethod", "CreateUser", XmlData);
                return StatusCode(500, "An error occurred while creating the user.");
            }


    }

        [HttpPost("register-subagent")]
        public IActionResult RegisterSubAgent(SubAgent agent)
        {
          
                    string XmlData = string.Empty;
            try
            {

                string encryptedPayload = AesEncryption.Encrypt(JsonSerializer.Serialize(agent));

                // ✅ Decrypt the payload back to original
                string ReqTime = DateTime.Now.ToString();
                string decryptedJson = AesEncryption.Decrypt(encryptedPayload);
                XmlData += "<Event><Data><RQ>" + decryptedJson + "</RQ></Data></Event>";
                XmlData += "<ReqTime>" + ReqTime + "</ReqTime>";

                // ✅ Deserialize to User object
                var user = JsonSerializer.Deserialize<Admin>(decryptedJson);

                if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password))
                    return BadRequest("Username, Password, and Email are required.");
                XmlData += "<RESPONSE>";
                var exists = _context.Admins.Any(u => u.Username == user.Username);
                XmlData += "<Data>" + exists + "</Data>";
                if (exists)
                    return Conflict("User already exists.");
                string ResTime = DateTime.Now.ToString();
                XmlData += "<ResTime>" + ResTime + "</ResTime>";
                agent.SubAgentId = $"{agent.AdminUserId}SA";
            _context.SubAgents.Add(agent);
            _context.SaveChanges();
                XmlData += "</RESPONSE>";
                XmlData += "</Event>";
                Logger.LogData(HttpContext.RequestServices, "E", "PostMethod", "Createsubagent", XmlData);
                return Ok(new
                {
                    Message = "SubAgent created successfully.",

                });
            }
            catch (Exception ex)
            {
                XmlData += "<Exception>" + ex.Message.ToString() + "</Exception></RESPONSE>";
                // Log error
                Logger.LogData(HttpContext.RequestServices, "EX", "PostMethod", "subagent", XmlData);
                return StatusCode(500, "An error occurred while creating the user.");
            }
            
        

    }

    [HttpPost("register-user")]
    public IActionResult RegisterUser(AppUser user)
    {

                    string XmlData = string.Empty;
            try
            {

                string encryptedPayload = AesEncryption.Encrypt(JsonSerializer.Serialize(user));

                // ✅ Decrypt the payload back to original
                string ReqTime = DateTime.Now.ToString();
                string decryptedJson = AesEncryption.Decrypt(encryptedPayload);
                XmlData += "<Event><Data><RQ>" + decryptedJson + "</RQ></Data></Event>";
                XmlData += "<ReqTime>" + ReqTime + "</ReqTime>";

                // ✅ Deserialize to User object
                var Appuser = JsonSerializer.Deserialize<Admin>(decryptedJson);

                if (string.IsNullOrEmpty(Appuser.Username) || string.IsNullOrEmpty(Appuser.Password))
                    return BadRequest("Username, Password, and Email are required.");
                XmlData += "<RESPONSE>";
                var exists = _context.Admins.Any(u => u.Username == user.Username);
                XmlData += "<Data>" + exists + "</Data>";
                if (exists)
                    return Conflict("User already exists.");
                string ResTime = DateTime.Now.ToString();
                XmlData += "<ResTime>" + ResTime + "</ResTime>";
                        user.UserId = $"{user.SubAgentId}U";
                _context.AppUsers.Add(user);
        _context.SaveChanges();
                XmlData += "</RESPONSE>";
                XmlData += "</Event>";
                Logger.LogData(HttpContext.RequestServices, "E", "PostMethod", "CreateAppUser", XmlData);
                return Ok(new
                {
                    Message = "AppUser created successfully.",

                });
            }
            catch (Exception ex)
            {
                XmlData += "<Exception>" + ex.Message.ToString() + "</Exception></RESPONSE>";
                // Log error
                Logger.LogData(HttpContext.RequestServices, "EX", "PostMethod", "CreateAppUser", XmlData);
                return StatusCode(500, "An error occurred while creating the user.");
            }
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
                HttpContext.Session.SetString("username", admin.Username);
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


[HttpPut("update-admin/{username}")]
public IActionResult UpdateAdmin(string username, [FromBody] Admin updatedAdmin)
{
    try
    {
        // ✅ Encrypt and Decrypt to simulate secure transmission
        string encryptedPayload = AesEncryption.Encrypt(JsonSerializer.Serialize(updatedAdmin));
        string decryptedJson = AesEncryption.Decrypt(encryptedPayload);
        var AdminUpdate = JsonSerializer.Deserialize<Admin>(decryptedJson);

        // ✅ Find existing admin
        var existingAdmin = _context.Admins.FirstOrDefault(a => a.Username == username);
        if (existingAdmin == null)
            return NotFound("Admin not found.");

        // ✅ Serialize old data for auditing
        var oldData = JsonSerializer.Serialize(existingAdmin);

        // ✅ Apply updates
        existingAdmin.Username = AdminUpdate.Username;
        existingAdmin.Password = AdminUpdate.Password;
        existingAdmin.Email = AdminUpdate.Email;
        existingAdmin.TenantId = AdminUpdate.TenantId;

        _context.SaveChanges();

        // ✅ Serialize new data for audit log
        var newData = JsonSerializer.Serialize(existingAdmin);

        // ✅ Create audit log
        var audit = new AdminAudit
        {
            AdminId = existingAdmin.Id,
            ChangedBy = HttpContext.Session.GetString("Username") ?? username,
            ChangedAt = DateTime.UtcNow,
            OldData = oldData,
            NewData = newData,
            ChangeType = "Update"
        };

        _context.AdminAudits.Add(audit);
        _context.SaveChanges();

        return Ok("Admin updated and audit logged.");
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"An error occurred: {ex.Message}");
    }
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
