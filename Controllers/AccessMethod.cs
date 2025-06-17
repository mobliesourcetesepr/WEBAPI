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
    string traceId = Guid.NewGuid().ToString();  // ✅ Unique Trace ID per request
    var logBuilder = new System.Text.StringBuilder();
    string finalMessage = string.Empty;
    string overallLevel = "Information";
    int step = 0;
    string methodName = nameof(RegisterAdmin);

    try
    {
        step++;
        logBuilder.AppendLine($"[TraceId: {traceId}] [{methodName}] [Information] [Step {step}] Received admin registration request.");

        step++;
        logBuilder.AppendLine($"[TraceId: {traceId}] [{methodName}] [Information] [Step {step}] Encrypting payload.");
        string encryptedPayload = AesEncryption.Encrypt(JsonSerializer.Serialize(admin));

        step++;
        logBuilder.AppendLine($"[TraceId: {traceId}] [{methodName}] [Information] [Step {step}] Decrypting payload.");
        string decryptedJson = AesEncryption.Decrypt(encryptedPayload);

        step++;
        logBuilder.AppendLine($"[TraceId: {traceId}] [{methodName}] [Information] [Step {step}] Deserializing payload.");
        var user = JsonSerializer.Deserialize<Admin>(decryptedJson);

        step++;
        if (string.IsNullOrEmpty(user?.Username) || string.IsNullOrEmpty(user.Password))
        {
            finalMessage = $"[Step {step}] [Code: E001] Username and Password are required. (E001: Missing credentials)";
            overallLevel = "Warning";
            logBuilder.AppendLine($"[TraceId: {traceId}] [{methodName}] [Warning] {finalMessage}");
            return BadRequest(finalMessage);
        }

        step++;
        logBuilder.AppendLine($"[TraceId: {traceId}] [{methodName}] [Information] [Step {step}] Checking if user already exists.");
        if (_context.AdminUser.Any(u => u.Username == user.Username))
        {
            finalMessage = $"[Step {step}] [Code: E002] User '{user.Username}' already exists. (E002: Duplicate user)";
            overallLevel = "Warning";
            logBuilder.AppendLine($"[TraceId: {traceId}] [{methodName}] [Warning] {finalMessage}");
            return Conflict(finalMessage);
        }

        step++;
        logBuilder.AppendLine($"[TraceId: {traceId}] [{methodName}] [Information] [Step {step}] Creating new admin ID.");
        admin.AdminId = $"{_context.AdminUser.Count() + 1}A";

        step++;
        logBuilder.AppendLine($"[TraceId: {traceId}] [{methodName}] [Information] [Step {step}] Adding new admin to database.");
        _context.AdminUser.Add(admin);
        _context.SaveChanges();

        step++;
        finalMessage = $"[Step {step}] Admin '{admin.Username}' registered successfully.";
        logBuilder.AppendLine($"[TraceId: {traceId}] [{methodName}] [Information] {finalMessage}");

        return Ok(new { TraceId = traceId, Message = finalMessage });
    }
    catch (Exception ex)
    {
        step++;
        finalMessage = $"[Step {step}] [Code: E999] Exception occurred: {ex.Message}" +
                       $"{(ex.InnerException != null ? $" | Inner: {ex.InnerException.Message}" : "")} (E999: Unhandled exception)";
        overallLevel = "Error";
        logBuilder.AppendLine($"[TraceId: {traceId}] [{methodName}] [Error] {finalMessage}");
        return StatusCode(500, "An error occurred while creating the user.");
    }
    finally
    {
        try
        {
            var logMessages = logBuilder.ToString();
            var log = new LogEntry
            {
              
                Level = overallLevel,
                Message = logMessages.Trim(),
                Source = source,
                Timestamp = DateTime.UtcNow
            };

            _logContext.Logs.Add(log);
            _logContext.SaveChanges();
        }
        catch
        {
            // Ignore logging failure
        }
    }
}



        // public IActionResult RegisterAdmin(AdminUser admin)
        // {
        //     string source = "/api/secure/register-admin";
        //     var logBuilder = new System.Text.StringBuilder();
        //     string finalMessage = string.Empty;
        //     string overallLevel = "Information";
        //     int step = 0;
        //       string methodName = nameof(RegisterAdmin);  // ✅ automatically get method name

        //     try
        //     {
        //         step++;
        //         logBuilder.AppendLine($"[{methodName}] [Information] [Step {step}] Received admin registration request.");

        //         step++;
        //         logBuilder.AppendLine($"[{methodName}] [Information] [Step {step}] Encrypting payload.");
        //         string encryptedPayload = AesEncryption.Encrypt(JsonSerializer.Serialize(admin));

        //         step++;
        //         logBuilder.AppendLine($"[{methodName}] [Information] [Step {step}] Decrypting payload.");
        //         string decryptedJson = AesEncryption.Decrypt(encryptedPayload);

        //         step++;
        //         logBuilder.AppendLine($"[{methodName}] [Information] [Step {step}] Deserializing payload.");
        //         var user = JsonSerializer.Deserialize<Admin>(decryptedJson);

        //         step++;
        //         if (string.IsNullOrEmpty(user?.Username) || string.IsNullOrEmpty(user.Password))
        //         {
        //             finalMessage = $"[Step {step}] [Code: E001] Username and Password are required. (E001: Missing credentials)";
        //             overallLevel = "Warning";
        //             logBuilder.AppendLine($"[{methodName}] [Warning] {finalMessage}");
        //             return BadRequest(finalMessage);
        //         }

        //         step++;
        //         logBuilder.AppendLine($"[{methodName}] [Information] [Step {step}] Checking if user already exists.");
        //         if (_context.AdminUser.Any(u => u.Username == user.Username))
        //         {
        //             step++;
        //             finalMessage = $"[Step {step}] [Code: E002] User '{user.Username}' already exists. (E002: Duplicate user)";
        //             overallLevel = "Warning";
        //             logBuilder.AppendLine($"[{methodName}][Warning] {finalMessage}");
        //             return Conflict(finalMessage);
        //         }

        //         step++;
        //         logBuilder.AppendLine($"[{methodName}][Information] [Step {step}] Creating new admin ID.");
        //         admin.AdminId = $"{_context.AdminUser.Count() + 1}A";

        //         step++;
        //         logBuilder.AppendLine($"[{methodName}] [Information] [Step {step}] Adding new admin to database.");
        //         _context.AdminUser.Add(admin);
        //         _context.SaveChanges();

        //         step++;
        //         finalMessage = $"[Step {step}] Admin '{admin.Username}' registered successfully.";
        //         logBuilder.AppendLine($"[{methodName}][Information] {finalMessage}");

        //         return Ok(new { Message = finalMessage });
        //     }
        //     catch (Exception ex)
        //     {
        //         step++;
        //         finalMessage = $"[Step {step}] [Code: E999] Exception occurred: {ex.Message} (E999: Unhandled exception)";
        //         overallLevel = "Error";
        //         logBuilder.AppendLine($"[{methodName}][Error] {finalMessage}");
        //         return StatusCode(500, "An error occurred while creating the user.");
        //     }
        //     finally
        //     {
        //         try
        //         {
        //             var logMessages = logBuilder.ToString();
        //             var log = new LogEntry
        //             {
        //                 Level = overallLevel,
        //                 Message = logMessages.Trim(),
        //                 Source = source,
        //                 Timestamp = DateTime.UtcNow
        //             };

        //             _logContext.Logs.Add(log);
        //             _logContext.SaveChanges();
        //         }
        //         catch
        //         {
        //             // Ignore logging failure
        //         }
        //     }
        // }






        // [HttpPost("register-admin")]
        // public IActionResult RegisterAdmin(AdminUser admin)
        // {
        //     string source = "/api/secure/register-admin";
        //     var logMessages = new List<(string Level, string Message)>();
        //     string finalMessage = string.Empty;
        //     int step = 0;  // dynamic step counter

        //     try
        //     {
        //         step++;
        //         logMessages.Add(("Information", $"[Step {step}] Received admin registration request."));

        //         step++;
        //         logMessages.Add(("Information", $"[Step {step}] Encrypting payload."));
        //         string encryptedPayload = AesEncryption.Encrypt(JsonSerializer.Serialize(admin));

        //         step++;
        //         logMessages.Add(("Information", $"[Step {step}] Decrypting payload."));
        //         string decryptedJson = AesEncryption.Decrypt(encryptedPayload);

        //         step++;
        //         logMessages.Add(("Information", $"[Step {step}] Deserializing payload."));
        //         var user = JsonSerializer.Deserialize<Admin>(decryptedJson);

        //         step++;
        //         if (string.IsNullOrEmpty(user?.Username) || string.IsNullOrEmpty(user.Password))
        //         {
        //             finalMessage = $"[Step {step}] [Code: E001] Username and Password are required. (E001: Missing credentials)";
        //             logMessages.Add(("Warning", finalMessage));
        //             return BadRequest(finalMessage);
        //         }

        //         step++;
        //         logMessages.Add(("Information", $"[Step {step}] Checking if user already exists."));
        //         if (_context.AdminUser.Any(u => u.Username == user.Username))
        //         {
        //             step++;
        //             finalMessage = $"[Step {step}] [Code: E002] User '{user.Username}' already exists. (E002: Duplicate user)";
        //             logMessages.Add(("Warning", finalMessage));
        //             return Conflict(finalMessage);
        //         }

        //         step++;
        //         logMessages.Add(("Information", $"[Step {step}] Creating new admin ID."));
        //         admin.AdminId = $"{_context.AdminUser.Count() + 1}A";

        //         step++;
        //         logMessages.Add(("Information", $"[Step {step}] Adding new admin to database."));
        //         _context.AdminUser.Add(admin);
        //         _context.SaveChanges();

        //         step++;
        //         finalMessage = $"[Step {step}] Admin '{admin.Username}' registered successfully.";
        //         logMessages.Add(("Information", finalMessage));

        //         return Ok(new { Message = finalMessage });
        //     }
        //     catch (Exception ex)
        //     {
        //         step++;
        //         finalMessage = $"[Step {step}] [Code: E999] Exception occurred: {ex.Message} (E999: Unhandled exception)";
        //         logMessages.Add(("Error", finalMessage));
        //         return StatusCode(500, "An error occurred while creating the user.");
        //     }
        //     finally
        //     {
        //         try
        //         {
        //             // Determine most severe log level
        //             string overallLevel = "Information"; // default
        //             if (logMessages.Any(m => m.Level == "Error"))
        //                 overallLevel = "Error";
        //             else if (logMessages.Any(m => m.Level == "Warning"))
        //                 overallLevel = "Warning";

        //             // Combine all messages with level tags
        //             string combinedMessage = string.Join(" | ", logMessages.Select(m => $"[{m.Level}] {m.Message}"));

        //             var log = new LogEntry
        //             {
        //                 Level = overallLevel,
        //                 Message = combinedMessage,
        //                 Source = source,
        //                 Timestamp = DateTime.UtcNow
        //             };

        //             _logContext.Logs.Add(log);
        //             _logContext.SaveChanges();
        //         }
        //         catch
        //         {
        //             // Ignore logging failure
        //         }
        //     }
        // }



        // [HttpPost("register-admin")]
        // public IActionResult RegisterAdmin(AdminUser admin)
        // {
        //     string source = "/api/secure/register-admin";
        //     var logMessages = new List<(string Level, string Message)>();
        //     string finalMessage = string.Empty;

        //     try
        //     {
        //         logMessages.Add(("Information", "Received admin registration request."));

        //         string encryptedPayload = AesEncryption.Encrypt(JsonSerializer.Serialize(admin));
        //         string decryptedJson = AesEncryption.Decrypt(encryptedPayload);
        //         logMessages.Add(("Information", "Payload encrypted and decrypted successfully."));

        //         var user = JsonSerializer.Deserialize<Admin>(decryptedJson);

        //         if (string.IsNullOrEmpty(user?.Username) || string.IsNullOrEmpty(user.Password))
        //         {
        //             finalMessage = "Username and Password are required.";
        //             logMessages.Add(("Warning", finalMessage));
        //             return BadRequest(finalMessage);
        //         }

        //         if (_context.Admins.Any(u => u.Username == user.Username))
        //         {
        //             finalMessage = $"User '{user.Username}' already exists.";
        //             logMessages.Add(("Warning", finalMessage));
        //             return Conflict(finalMessage);
        //         }

        //         admin.AdminId = $"{_context.AdminUser.Count() + 1}A";
        //         _context.AdminUser.Add(admin);
        //         _context.SaveChanges();

        //         finalMessage = $"Admin '{admin.Username}' registered successfully.";
        //         logMessages.Add(("Information", finalMessage));

        //         return Ok(new { Message = finalMessage });
        //     }
        //     catch (Exception ex)
        //     {
        //         finalMessage = $"Exception: {ex.Message}";
        //         logMessages.Add(("Error", finalMessage));
        //         return StatusCode(500, "An error occurred while creating the user.");
        //     }
        //     finally
        //     {
        //                 try
        //                 {
        //                     // Determine most severe log level
        //                     string overallLevel = "Information"; // default
        //                     if (logMessages.Any(m => m.Level == "Error"))
        //                         overallLevel = "Error";
        //                     else if (logMessages.Any(m => m.Level == "Warning"))
        //                         overallLevel = "Warning";

        //                     // Combine all messages with level tags
        //                     string combinedMessage = string.Join(" | ", logMessages.Select(m => $"[{m.Level}] {m.Message}"));

        //                     var log = new LogEntry
        //                     {
        //                         Level = overallLevel,
        //                         Message = combinedMessage,
        //                         Source = source,
        //                         Timestamp = DateTime.UtcNow
        //                     };

        //                     _logContext.Logs.Add(log);
        //                     _logContext.SaveChanges();
        //                 }
        //                 catch(Exception ex)
        //                 {
        //                      //return StatusCode(500, "An error occurred while creating the user.");
        //             // Do nothing — logging failure shouldn't break the request
        //                 }
        //     }
        // }



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
                CreatedAt = DateTime.Now
            };

            return Ok(new
            {
                Message = "Admin report created successfully.",
                Report = savedReport
            });
        }
        // [HttpPost("login")]
        // public async Task<IActionResult> Login([FromBody] LoginRequest request)
        // {
        //     var user = _context.AdminUser
        //         .FirstOrDefault(u => u.Username == request.Username && u.Password == request.Password);

        //     if (user == null)
        //         return Unauthorized("Invalid credentials");

        //     var payload = new AuthPayload
        //     {
        //         Username = user.Username,
        //         Role = user.Role,
        //     };

        //     var json = JsonSerializer.Serialize(payload);
        //     var token = AesEncryption.Encrypt(json);
        //  HttpContext.Session.SetString("Role", payload.Role);
        //     HttpContext.Session.SetString("Token", token);
        // HttpContext.Session.SetString("Username", payload.Username);
        //     return Ok(new
        //     {
        //         Message = "Login successful",
        //         Token = token
        //     });
        // }


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
    var token = AesEncryption.Encrypt(json); // secure token

    // 🔐 Store in HttpContext.Session
    HttpContext.Session.SetString("Role", payload.Role);
    HttpContext.Session.SetString("Token", token);
    HttpContext.Session.SetString("Username", payload.Username);

    // 💾 Store session info in database
    var session = new SessionStore
    {

        UserId = user.AdminId,
        Token = token,
        IssuedAt = DateTime.Now,
        ExpiresAt = DateTime.Now.AddMinutes(5),
        LastAccessedAt = DateTime.Now,
        IsActive = true
    };

    _context.SessionStores.Add(session);
    await _context.SaveChangesAsync();

    return Ok(new
    {
        Message = "Login successful",
        Token = token
    });
}

  [HttpGet("protected")]
    public IActionResult ProtectedApi([FromHeader(Name = "X-Session-Token")] string Sessiontoken)
    {
        var token = Request.Headers["X-Session-Token"].ToString();

        var session = _context.SessionStores.FirstOrDefault(s =>
            s.Token == token && s.IsActive && s.ExpiresAt > DateTime.Now);

        if (session == null)
            return Unauthorized("Session expired or invalid.");

        // 🔁 Refresh session
        session.LastAccessedAt = DateTime.Now;
        session.ExpiresAt = DateTime.UtcNow.AddMinutes(5); // refresh expiry
        _context.SaveChanges();

        return Ok($"Welcome back, user: {session.UserId}");
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
