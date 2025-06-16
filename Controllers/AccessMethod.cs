using Microsoft.AspNetCore.Mvc;
using MultiTenantAPI.Data;
using MultiTenantAPI.Filters;
using MultiTenantAPI.Helpers;
using MultiTenantAPI.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace MultiTenantAPI.Controllers
{
    [Route("api/admin-secure")]
    [ApiController]
    public class SecureAdminController : ControllerBase
    {
        private readonly UserDbContext _context;

        public SecureAdminController(UserDbContext context)
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

        [HttpGet("dashboard")]
        [AllowRole("Admin")]
        public IActionResult Dashboard([FromHeader(Name = "X-Auth-Token")] string Token)
        {
            // Simulated dashboard data
            var dashboardData = new
            {
                TotalUsers = 105,
                ActiveSubAgents = 18,
                ReportsGeneratedToday = 7,
                ServerTime = DateTime.UtcNow
            };

            return Ok(dashboardData);
        }

        [HttpPost("report")]
        [AllowRole("Admin")]
        public IActionResult CreateReport([FromHeader(Name = "X-Auth-Token")] string Token,[FromBody] ReportModel report)
        {
            if (report == null || string.IsNullOrWhiteSpace(report.Title))
            {
                return BadRequest("Invalid report data.");
            }

            // Simulate report saving logic (in real case, save to DB)
            var savedReport = new
            {
                ReportId = Guid.NewGuid(),
                report.Title,
                report.Description,
                CreatedAt = DateTime.UtcNow
            };

            return Ok(new
            {
                Message = "Admin report created successfully.",
                Report = savedReport
            });
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = _context.AdminUser
                .FirstOrDefault(u => u.Username == request.Username && u.Password == request.Password);

            if (user == null)
                return Unauthorized("Invalid credentials");

            var payload = new AuthPayload
            {
                Username = user.Username,
                Role = user.Role,
            };
       
            var json = JsonSerializer.Serialize(payload);
            var token = AesEncryption.Encrypt(json);
         HttpContext.Session.SetString("Role", payload.Role);
            HttpContext.Session.SetString("Token", token);
        HttpContext.Session.SetString("Username", payload.Username);
            return Ok(new
            {
                Message = "Login successful",
                Token = token
            });
        }


 [HttpPut("update-admin/{username}")]
public IActionResult UpdateAdmin(string username, [FromBody] User updatedUser)
{
    try
    {
        var role = HttpContext.Session.GetString("Role");
        var sessionUsername = HttpContext.Session.GetString("Username");

        if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(sessionUsername))
            return Unauthorized("User not logged in.");

        // ✅ Only Admin or SubAgent/AppUser with CanUpdate rights can proceed
        bool hasUpdatePermission = false;

        if (role == "Admin")
        {
            hasUpdatePermission = true;
        }
        else if (role == "SubAgent")
        {
            var subAgent = _context.SubAgents.FirstOrDefault(sa => sa.Username == sessionUsername);
            hasUpdatePermission = subAgent?.CanUpdate ?? false;
        }
        else if (role == "AppUser")
        {
            var appUser = _context.AppUsers.FirstOrDefault(u => u.Username == sessionUsername);
            hasUpdatePermission = appUser?.CanUpdate ?? false;
        }

        if (!hasUpdatePermission)
            return Unauthorized("Update rights not granted. Contact Admin.");

        // ✅ Encrypt and Decrypt to simulate secure transmission
        string encryptedPayload = AesEncryption.Encrypt(JsonSerializer.Serialize(updatedUser));
        string decryptedJson = AesEncryption.Decrypt(encryptedPayload);
        var userUpdate = JsonSerializer.Deserialize<User>(decryptedJson);

        // ✅ Find existing admin
        var existingAdmin = _context.AdminUser.FirstOrDefault(a => a.Username == username);
        if (existingAdmin == null)
            return NotFound("Admin not found.");

        // ✅ Serialize old data for auditing
        var oldData = JsonSerializer.Serialize(existingAdmin);

        // ✅ Apply updates
        existingAdmin.Username = userUpdate.Username;
        existingAdmin.Password = userUpdate.Password;


        _context.SaveChanges();

        // ✅ Serialize new data for audit log
        var newData = JsonSerializer.Serialize(existingAdmin);

        // ✅ Create audit log
        var audit = new AdminAudit
        {
            AdminId = existingAdmin.Id,
            ChangedBy = sessionUsername,
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
    }
}
