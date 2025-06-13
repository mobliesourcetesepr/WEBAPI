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

}