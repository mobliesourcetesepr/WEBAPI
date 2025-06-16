using Microsoft.AspNetCore.Mvc;
using MultiTenantAPI.Data;
using MultiTenantAPI.Filters;
using MultiTenantAPI.Helpers;
using MultiTenantAPI.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using MultiTenantApi.Attributes;
namespace MultiTenantAPI.Controllers
{
    [Route("api/admin-secure")]
    [ApiController]
    public class SecureAdminController : ControllerBase
    {
        private readonly UserDbContext _context;

        private readonly MyLogDbContext _logContext;

    public SecureAdminController(UserDbContext context, MyLogDbContext logContext)
{
    _context = context;
    _logContext = logContext;
}


[HttpPost("register-admin")]
public IActionResult RegisterAdmin(AdminUser admin)
{
    string source = "/api/secure/register-admin";
    var logMessages = new List<(string Level, string Message)>();
    string finalMessage = string.Empty;

    try
    {
        logMessages.Add(("Information", "Received admin registration request."));

        string encryptedPayload = AesEncryption.Encrypt(JsonSerializer.Serialize(admin));
        string decryptedJson = AesEncryption.Decrypt(encryptedPayload);
        logMessages.Add(("Information", "Payload encrypted and decrypted successfully."));

        var user = JsonSerializer.Deserialize<Admin>(decryptedJson);

        if (string.IsNullOrEmpty(user?.Username) || string.IsNullOrEmpty(user.Password))
        {
            finalMessage = "Username and Password are required.";
            logMessages.Add(("Warning", finalMessage));
            return BadRequest(finalMessage);
        }

        if (_context.Admins.Any(u => u.Username == user.Username))
        {
            finalMessage = $"User '{user.Username}' already exists.";
            logMessages.Add(("Warning", finalMessage));
            return Conflict(finalMessage);
        }

        admin.AdminId = $"{_context.AdminUser.Count() + 1}A";
        _context.AdminUser.Add(admin);
        _context.SaveChanges();

        finalMessage = $"Admin '{admin.Username}' registered successfully.";
        logMessages.Add(("Information", finalMessage));

        return Ok(new { Message = finalMessage });
    }
    catch (Exception ex)
    {
        finalMessage = $"Exception: {ex.Message}";
        logMessages.Add(("Error", finalMessage));
        return StatusCode(500, "An error occurred while creating the user.");
    }
    finally
    {
                try
                {
                    // Determine most severe log level
                    string overallLevel = "Information"; // default
                    if (logMessages.Any(m => m.Level == "Error"))
                        overallLevel = "Error";
                    else if (logMessages.Any(m => m.Level == "Warning"))
                        overallLevel = "Warning";

                    // Combine all messages with level tags
                    string combinedMessage = string.Join(" | ", logMessages.Select(m => $"[{m.Level}] {m.Message}"));

                    var log = new LogEntry
                    {
                        Level = overallLevel,
                        Message = combinedMessage,
                        Source = source,
                        Timestamp = DateTime.UtcNow
                    };

                    _logContext.Logs.Add(log);
                    _logContext.SaveChanges();
                }
                catch(Exception ex)
                {
                     //return StatusCode(500, "An error occurred while creating the user.");
            // Do nothing — logging failure shouldn't break the request
                }
    }
}



        // [HttpPost("register-admin")]
        // public IActionResult RegisterAdmin(AdminUser admin)
        // {
        //     try
        //     {
        //         Log("Info", "Received admin registration request.");

        //         string encryptedPayload = AesEncryption.Encrypt(JsonSerializer.Serialize(admin));
        //         string decryptedJson = AesEncryption.Decrypt(encryptedPayload);

        //         var user = JsonSerializer.Deserialize<Admin>(decryptedJson);

        //         if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password))
        //         {
        //             Log("Warning", "Missing required fields.");
        //             return BadRequest("Username, Password, and Email are required.");
        //         }

        //         var exists = _context.Admins.Any(u => u.Username == user.Username);

        //         if (exists)
        //         {
        //             Log("Warning", $"User '{user.Username}' already exists.");
        //             return Conflict("User already exists.");
        //         }

        //         admin.AdminId = $"{_context.AdminUser.Count() + 1}A";
        //         _context.AdminUser.Add(admin);
        //         _context.SaveChanges();

        //         Log("Info", $"Admin '{admin.Username}' registered successfully.");

        //         return Ok(new { Message = "Admin created successfully." });
        //     }
        //     catch (Exception ex)
        //     {
        //         Log("Error", $"Exception occurred: {ex.Message}");
        //         return StatusCode(500, "An error occurred while creating the user.");
        //     }
        // }

        // // Logging helper
        // private void Log(string level, string message)
        // {
        //     _logContext.Logs.Add(new LogEntry
        //     {
        //         Level = level,
        //         Message = message,
        //         Source = "RegisterAdmin"
        //     });
        //     _logContext.SaveChanges();
        // }



        //[SwaggerIgnore]
        //  [HttpPost("register-admin")]
        //     public IActionResult RegisterAdmin(AdminUser admin)
        //     {
        //         string XmlData = string.Empty;
        //         try
        //         {

        //             string encryptedPayload = AesEncryption.Encrypt(JsonSerializer.Serialize(admin));

        //             // ✅ Decrypt the payload back to original
        //             string decryptedJson = AesEncryption.Decrypt(encryptedPayload);

        //             // ✅ Deserialize to User object
        //             var user = JsonSerializer.Deserialize<Admin>(decryptedJson);

        //             if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password))
        //                 return BadRequest("Username, Password, and Email are required.");

        //             var exists = _context.Admins.Any(u => u.Username == user.Username);

        //             if (exists)
        //                 return Conflict("User already exists.");


        //             admin.AdminId = $"{_context.AdminUser.Count() + 1}A";
        //             _context.AdminUser.Add(admin);
        //             _context.SaveChanges();


        //             return Ok(new
        //             {
        //                 Message = "Admin created successfully.",

        //             });
        //         }
        //         catch (Exception ex)
        //         {

        //             return StatusCode(500, "An error occurred while creating the user.");
        //         }


        // }

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
