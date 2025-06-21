using System.Text.Json.Serialization;

namespace AgentCreation.Models
{
    public class ClientMasterModel
    {

        public string CLT_CLIENT_NAME { get; set; }
        public string CLT_ADDRESS1 { get; set; }
        public string CLT_STATE_ID { get; set; }
        public string CLT_CLIENT_TITLE { get; set; }
        public string CLT_COUNTRY_ID { get; set; }
        public string CLT_CITY_ID { get; set; }
        public string CLT_ZIP_CODE { get; set; }
        public string CLT_MOBILE_NO { get; set; }
        public string CLT_EMAIL_ID { get; set; }
        public string CLT_CLIENT_LASTNAME { get; set; }
        public string CLT_CLIENT_FIRSTNAME { get; set; }
        //[JsonIgnore]
        //public string CLT_CREATED_BY { get; set; }

    }
    public class ClientUpdateModel
    {
        //[JsonIgnore]
        // public string CLT_UPDATED_BY { get; set; }
        public string CLT_MOBILE_NO { get; set; }
        public string CLT_CLIENT_FIRSTNAME { get; set; }

        public string CLT_CLIENT_LASTNAME { get; set; }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class AdminUser
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string AdminId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        [JsonIgnore]
        public string Role { get; set; }

    }
    public class AuthPayload
    {
        public string Username { get; set; }
        public string Role { get; set; }

    }
    public class ChangePasswordRequest
    {
        public string Username { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
    public class BranchModel
    {
        public string BranchName { get; set; }
        public string AgentId { get; set; }
        public string Address { get; set; }
        public string CountryCode { get; set; }
        public string StateCode { get; set; }
        public string CityCode { get; set; }
    }
}