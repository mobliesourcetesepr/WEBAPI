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
    public class AgentModel
    {
        //public string AGN_AGENT_ID { get; set; }
        public string AGN_AGENT_TYPE { get; set; }
        public string AGN_AGENCY_NAME { get; set; }
        //public string AGN_BRANCH_ID { get; set; }
        //public int AGN_TERMINAL_COUNT { get; set; }
        public string AGN_AGENT_TITLE { get; set; }
        public string AGN_AGENT_FIRSTNAME { get; set; }
        public string AGN_AGENT_LASTNAME { get; set; }
        public bool AGN_AGENT_ACTIVESTATUS { get; set; }
        public string AGN_AGENT_SALESMAN { get; set; }
        public string AGN_PHONE_NO { get; set; }
        public string AGN_MOBILE_NO { get; set; }
        public string AGN_FAX_NO { get; set; }
        public string AGN_EMAIL_ID { get; set; }
        public string AGN_ALTEREMAIL_ID { get; set; }
        public string AGN_ADDRESS_1 { get; set; }
        public string AGN_COUNTRY_ID { get; set; }
        public string AGN_STATE_ID { get; set; }
        public string AGN_CITY_ID { get; set; }
        public decimal AGN_CURRENT_BALANCE_AMT { get; set; }
        public decimal AGN_TOTAL_DEPOSIT_AMT { get; set; }
        public decimal AGN_CURRENT_CREDIT_BALANCE { get; set; }
        public string AGN_AGENT_REMARKS { get; set; }
    }

    public class AgentRequestModel
    {
        public string AGN_AGENCY_NAME { get; set; }
        public string AGN_AGENT_TITLE { get; set; }
        public string AGN_AGENT_FIRSTNAME { get; set; }
        public string AGN_AGENT_LASTNAME { get; set; }
        public string AGN_PHONE_NO { get; set; }
        public string AGN_MOBILE_NO { get; set; }
        public string AGN_EMAIL_ID { get; set; }
        public string AGN_ADDRESS_1 { get; set; }
        public string AGN_COUNTRY_ID { get; set; }
        public string AGN_CITY_ID { get; set; }
    }

    public class GroupDetailsModel
    {
        public bool GRP_STATUS { get; set; }
        public string GRP_GROUP_NAME { get; set; }
       // public string GRP_UPDATED_BY { get; set; }
        //public string GRP_BRANCH_ID { get; set; }
        //public string GRP_RCODE { get; set; }
        //public string GRP_GROUP_ID { get; set; }
       // public string GRP_CUR_CODE { get; set; }
        //public string GRP_REMARKS { get; set; }
        public string GRP_CURRENCY_CODE { get; set; }
    }


}