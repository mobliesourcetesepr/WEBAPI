using System.Text.Json.Serialization;

namespace MultiTenantAPI.Models
{
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class Admin
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string TenantId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
    public class LogDetail
    {
        public int Id { get; set; }
        public string LogType { get; set; }
        public string PageName { get; set; }
        public string FunctionName { get; set; }
        public string LogData { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
    }

    public class AdminAudit
    {
        public int Id { get; set; }
        public int AdminId { get; set; }
        public string ChangedBy { get; set; }
        public DateTime ChangedAt { get; set; }
        public string OldData { get; set; }
        public string NewData { get; set; }
        public string ChangeType { get; set; } // e.g., "Update"
    }

    public class AdminUser
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string AdminId { get; set; } = string.Empty; // "1A"
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        [JsonIgnore]
        public string Role { get; set; } = "Admin"; // ✅ Default value



    }

    // Models/SubAgent.cs
    public class SubAgent
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string SubAgentId { get; set; } = string.Empty; // like "1SA"
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        // FK to AdminUser
        public int AdminUserId { get; set; }
        public bool CanLogin { get; set; } = false;
        public bool CanUpdate { get; set; } = false;

    }


    // Models/AppUser.cs
    public class AppUser
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty; // "1U"
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int SubAgentId { get; set; }
        public bool CanLogin { get; set; } = false;
        public bool CanUpdate { get; set; } = false;
    }

    public class PermissionModel
    {
        public bool CanLogin { get; set; }
        public bool CanUpdate { get; set; }
    }
    // Models/LoginRequest.cs
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
    public class AuthPayload
    {
        public string Username { get; set; }
        public string Role { get; set; }

    }
    public class ReportModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
    }

    public class LogEntry
    {
        public int Id { get; set; }
        public string Level { get; set; } // e.g., Information, Error
        public string Message { get; set; }
        public string Source { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
public class SessionStore
{
    public int Id { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
    public DateTime LastAccessedAt { get; set; } // NEW
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
}

public class OAuthRequest
{
    public string GrantType { get; set; } = "password";
    public string Username { get; set; }
    public string Password { get; set; }
}

}