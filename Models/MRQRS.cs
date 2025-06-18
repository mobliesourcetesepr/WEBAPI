using System.Text.Json.Serialization;

namespace MultiTenantAPI.Models
{




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
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class OAuthRequest
{
    [JsonIgnore]
    public string GrantType { get; set; } = "password";
    public string Username { get; set; }
    public string Password { get; set; }
}

}