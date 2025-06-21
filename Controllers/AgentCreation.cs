using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.Data.SqlClient;
using AgentCreation.Data;
using AgentCreation.Services;
using System.Text.Json;
using AgentCreation.Models;
using AgentCreation.Helpers;
using System.Security.Cryptography;
using Swashbuckle.AspNetCore.Annotations;
using AgentCreation.Utilities;

[Route("api")]
[ApiController]
public class ClientMasterController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly UserDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly JwtTokenService _tokenService;
    public ClientMasterController(IConfiguration configuration, UserDbContext context, IHttpContextAccessor httpContextAccessor, JwtTokenService tokenService)
    {
        _configuration = configuration;
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _tokenService = tokenService;
    }


    [HttpPost("Adminlogin")]
    public IActionResult Login([FromBody] LoginRequest request)
    {

        try
        {

            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                return BadRequest("Username and Password are required");

            var user = _context.AdminUser
                .FirstOrDefault(u => u.Username == request.Username && u.Password == request.Password);
            //Console.WriteLine("User: " + user?.Username);
            if (user == null)
                return Unauthorized("Invalid username or password");
            //string HaxhedPassword = AesEncryption.ComputeSha256Hash(request.Password);

            var hashed = AesEncryption.SHAPROCESS(request.Password);


            // ✅ Save to session
            HttpContext.Session.SetString("LoggedInUsername", user.Username);

            // ✅ Generate JWT token
            var token = _tokenService.GenerateToken(user.Username, user.Role);

            return Ok(new
            {
                Message = "Login successful",
                Token = token,
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Message = "Unexpected error occurred",
                Error = ex.Message
            });
        }
    }
    [SwaggerIgnore]
    [HttpPost("agentlogin")]
    public IActionResult AgentLoginInsert(string ID, string Title, string Username, string Firstname, string lastname, string Email, string MobileNo, string password)
    {
        try
        {

            // 🔐 Hash the password
            var hashedPassword = AesEncryption.SHAPROCESS(password);

            // ✅ Insert into login log table
            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("SqlServerConnection")))
            using (SqlCommand cmd = new SqlCommand("InsertTerminalLogin", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@MODE", "I");
                cmd.Parameters.AddWithValue("@LGN_TERMINAL_LOGIN_NAME", Email ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@LGN_TERMINAL_LOGIN_PWD", hashedPassword ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@LGN_AGENT_ID", ID ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@LGN_TITLE", Title ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@LGN_MOBILE_NUMBER", MobileNo ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@LGN_EMAILID", Email ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@LGN_FIRSTNAME", Firstname ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@LGN_LASTNAME", lastname ?? (object)DBNull.Value);
                conn.Open();
                int result = cmd.ExecuteNonQuery();
                conn.Close();

                return Ok(new
                {
                    Message = "Login data inserted successfully",
                    RowsAffected = result
                });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Message = "Unexpected error occurred",
                Error = ex.Message
            });
        }
    }
    [HttpPost("changepassword")]
    public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("SqlServerConnection")))
            {
                conn.Open();

                string oldPasswordHash = AesEncryption.SHAPROCESS(request.OldPassword);
                string newPasswordHash = AesEncryption.SHAPROCESS(request.NewPassword);

                using (SqlCommand cmd = new SqlCommand("InsertTerminalLogin", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@MODE", "CPWD");
                    cmd.Parameters.AddWithValue("@LGN_TERMINAL_LOGIN_NAME", request.Username);
                    cmd.Parameters.AddWithValue("@LGN_TERMINAL_LOGIN_PWD", oldPasswordHash);
                    cmd.Parameters.AddWithValue("@NEW_PASSWORD", newPasswordHash);

                    var result = cmd.ExecuteScalar()?.ToString();

                    return result switch
                    {
                        "SUCCESS" => Ok("Password changed successfully."),
                        "INVALID_OLD_PASSWORD" => Unauthorized("Old password is incorrect."),
                        "PASSWORD_ALREADY_USED" => BadRequest("New password cannot be the same as old password."),
                        "USER_NOT_FOUND" => NotFound("User not found."),
                        _ => BadRequest("Password change failed.")
                    };
                }
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }


    //     [HttpPost("changepassword")]
    // public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
    // {
    //     using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("SqlServerConnection")))
    //     {
    //         conn.Open();

    //         // 1. Fetch stored password hash
    //         using (SqlCommand cmd = new SqlCommand("SELECT LGN_TERMINAL_LOGIN_PWD FROM T_M_LOGIN WHERE LGN_TERMINAL_LOGIN_NAME = @Username", conn))
    //         {
    //             cmd.Parameters.AddWithValue("@Username", request.Username);
    //             var storedHash = cmd.ExecuteScalar()?.ToString();

    //             if (storedHash == null)
    //                 return NotFound("User not found");

    //             // 2. Compare hash of provided old password
    //             string oldPasswordHash = AesEncryption.SHAPROCESS(request.OldPassword);
    //             if (storedHash != oldPasswordHash)
    //                 return Unauthorized("Old password is incorrect");
    //         }

    //         // 3. Hash new password and update it
    //         string newPasswordHash = AesEncryption.SHAPROCESS(request.NewPassword);
    //         using (SqlCommand updateCmd = new SqlCommand("UPDATE T_M_LOGIN SET LGN_TERMINAL_LOGIN_PWD = @NewHash WHERE LGN_TERMINAL_LOGIN_NAME = @Username", conn))
    //         {
    //             updateCmd.Parameters.AddWithValue("@NewHash", newPasswordHash);
    //             updateCmd.Parameters.AddWithValue("@Username", request.Username);
    //             updateCmd.ExecuteNonQuery();
    //         }

    //         return Ok("Password updated successfully");
    //     }
    // }



    [HttpPost("agentcreation")]
    public IActionResult InsertClient([FromBody] ClientMasterModel client)
    {
        try
        {

            var CREATEDBY = HttpContext.Session.GetString("LoggedInUsername");
            var password = AesEncryption.Generate();

            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("SqlServerConnection")))
            using (SqlCommand cmd = new SqlCommand("InsertClientMaster", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CLT_CLIENT_NAME", client.CLT_CLIENT_NAME ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@CLT_ADDRESS", client.CLT_ADDRESS1 ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@CLT_STATE_ID", client.CLT_STATE_ID ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@CLT_CLIENT_TITLE", client.CLT_CLIENT_TITLE ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@CLT_COUNTRY_ID", client.CLT_COUNTRY_ID ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@CLT_CITY_ID", client.CLT_CITY_ID ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@CLT_ZIP_CODE", client.CLT_ZIP_CODE ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@CLT_MOBILE_NO", client.CLT_MOBILE_NO ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@CLT_EMAIL_ID", client.CLT_EMAIL_ID ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@CLT_CLIENT_LASTNAME", client.CLT_CLIENT_LASTNAME ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@CLT_CLIENT_FIRSTNAME", client.CLT_CLIENT_FIRSTNAME ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@CLT_CREATED_BY", CREATEDBY);
                var clientIdParam = new SqlParameter("@CLT_CLIENT_ID", SqlDbType.VarChar, 50)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(clientIdParam);
                conn.Open();
                int result = cmd.ExecuteNonQuery();
                string generatedClientId = clientIdParam.Value?.ToString();
                conn.Close();
                AgentLoginInsert(generatedClientId, client.CLT_CLIENT_TITLE, client.CLT_CLIENT_NAME, client.CLT_CLIENT_FIRSTNAME, client.CLT_CLIENT_LASTNAME, client.CLT_EMAIL_ID, client.CLT_MOBILE_NO, password);
                string htmlContent = EmailTemplateGenerator.GetHtml($"{client.CLT_CLIENT_FIRSTNAME} {client.CLT_CLIENT_LASTNAME}", client.CLT_EMAIL_ID, password, EmailTemplateType.Welcome, WelcomeTemplateVariant.Format3);
                var emailSender = new EmailSender(_configuration);
                emailSender.SendEmail(client.CLT_EMAIL_ID, "Welcome to Our Platform", htmlContent);



                return Ok(new
                {
                    Message = "Inserted Successfully",
                    RowsAffected = result
                });
            }
        }
        catch (SqlException sqlEx)
        {
            return StatusCode(500, new
            {
                Message = "SQL error while inserting client",
                Error = sqlEx.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Message = "Unexpected error occurred",
                Error = ex.Message
            });
        }
    }

    [HttpPost("branchdetails")]
    public IActionResult InsertBranch([FromBody] BranchModel model)
    {
        var createdBy = HttpContext.Session.GetString("LoggedInUsername");

        if (string.IsNullOrEmpty(createdBy))
            return Unauthorized("User not logged in.");

        try
        {
            Console.WriteLine(model.BranchName + "," + model.AgentId + "," + model.Address + "," + model.StateCode + "," + model.CityCode + "," + model.CountryCode);
            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("SqlServerConnection")))
            using (SqlCommand cmd = new SqlCommand("InsertBranchDetails", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@BRH_BRANCH_NAME", model.BranchName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@BRH_ADDRESS", model.Address ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@BRH_BOA_AGENTID", model.AgentId ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@BRH_STATE_CODE", model.StateCode ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@BRH_CITY_CODE", model.CityCode ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@BRH_COUNTRY_CODE", model.CountryCode ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@BRH_CREATED_BY", createdBy ?? (object)DBNull.Value);
                cmd.Parameters.Add("@BRH_BRANCH_ID", SqlDbType.NVarChar, 50).Direction = ParameterDirection.Output;

                conn.Open();
                int rows = cmd.ExecuteNonQuery();

                if (rows > 0)
                    return Ok(new { message = "Branch inserted successfully." });
                else
                    return StatusCode(500, "Failed to insert branch.");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error: " + ex.Message);
        }
    }



    [HttpPost("agentinsert")]
    public IActionResult InsertAgent([FromBody] AgentModel model)
    {
        var createdBy = HttpContext.Session.GetString("LoggedInUsername");

        if (string.IsNullOrEmpty(createdBy))
            return Unauthorized("User not logged in.");

        try
        {
            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("SqlServerConnection")))
            using (SqlCommand cmd = new SqlCommand("InsertAgentDetails", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@AGN_AGENT_TYPE", model.AGN_AGENT_TYPE ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@AGN_AGENCY_NAME", model.AGN_AGENCY_NAME ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@AGN_AGENT_TITLE", model.AGN_AGENT_TITLE ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@AGN_AGENT_FIRSTNAME", model.AGN_AGENT_FIRSTNAME ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@AGN_AGENT_LASTNAME", model.AGN_AGENT_LASTNAME ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@AGN_AGENT_SALESMAN", model.AGN_AGENT_SALESMAN ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@AGN_PHONE_NO", model.AGN_PHONE_NO ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@AGN_MOBILE_NO", model.AGN_MOBILE_NO ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@AGN_FAX_NO", model.AGN_FAX_NO ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@AGN_EMAIL_ID", model.AGN_EMAIL_ID ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@AGN_ALTEREMAIL_ID", model.AGN_ALTEREMAIL_ID ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@AGN_ADDRESS_1", model.AGN_ADDRESS_1 ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@AGN_COUNTRY_ID", model.AGN_COUNTRY_ID ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@AGN_STATE_ID", model.AGN_STATE_ID ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@AGN_CITY_ID", model.AGN_CITY_ID ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@AGN_CURRENT_BALANCE_AMT", model.AGN_CURRENT_BALANCE_AMT);
                cmd.Parameters.AddWithValue("@AGN_TOTAL_DEPOSIT_AMT", model.AGN_TOTAL_DEPOSIT_AMT);
                cmd.Parameters.AddWithValue("@AGN_CURRENT_CREDIT_BALANCE", model.AGN_CURRENT_CREDIT_BALANCE);
                cmd.Parameters.AddWithValue("@AGN_AGENT_REMARKS", model.AGN_AGENT_REMARKS ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@AGN_AGENT_ACTIVESTATUS", model.AGN_AGENT_ACTIVESTATUS);
                cmd.Parameters.AddWithValue("@AGN_CREATED_BY", createdBy);
                SqlParameter outputIdParam = new SqlParameter("@GeneratedAgentId", SqlDbType.VarChar, 50)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(outputIdParam);
                conn.Open();
                int rows = cmd.ExecuteNonQuery();

                if (rows > 0)
                    return Ok(new { message = "Agent inserted successfully." });
                else
                    return StatusCode(500, "Failed to insert agent.");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error: " + ex.Message);
        }
    }

    [HttpPost("insertagentrequest")]
    public IActionResult InsertAgentRequest([FromBody] AgentRequestModel model)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("SqlServerConnection")))
            using (SqlCommand cmd = new SqlCommand("InsertAgentRequest", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@AGN_AGENCY_NAME", model.AGN_AGENCY_NAME ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@AGN_AGENT_TITLE", model.AGN_AGENT_TITLE ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@AGN_AGENT_FIRSTNAME", model.AGN_AGENT_FIRSTNAME ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@AGN_AGENT_LASTNAME", model.AGN_AGENT_LASTNAME ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@AGN_PHONE_NO", model.AGN_PHONE_NO ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@AGN_MOBILE_NO", model.AGN_MOBILE_NO ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@AGN_EMAIL_ID", model.AGN_EMAIL_ID ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@AGN_ADDRESS_1", model.AGN_ADDRESS_1 ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@AGN_COUNTRY_ID", model.AGN_COUNTRY_ID);
                cmd.Parameters.AddWithValue("@AGN_CITY_ID", model.AGN_CITY_ID);
                conn.Open();
                cmd.ExecuteNonQuery();
                return Ok("Agent request inserted successfully.");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error inserting agent request: " + ex.Message);
        }
    }


    [HttpPost("insertgroup")]
    public IActionResult InsertGroup([FromBody] GroupDetailsModel model)
    {

        try
        {
            var createdBy = HttpContext.Session.GetString("LoggedInUsername");

            if (string.IsNullOrEmpty(createdBy))
                return Unauthorized("User not logged in.");
            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("SqlServerConnection")))
            using (SqlCommand cmd = new SqlCommand("InsertGroupDetails", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@GRP_STATUS", model.GRP_STATUS);
                cmd.Parameters.AddWithValue("@GRP_CREATED_BY", createdBy ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@GRP_GROUP_NAME", model.GRP_GROUP_NAME ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@GRP_CURRENCY_CODE", model.GRP_CURRENCY_CODE ?? (object)DBNull.Value);

                // OUTPUT parameter
                SqlParameter outputIdParam = new SqlParameter("@GeneratedGroupId", SqlDbType.VarChar, 50)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(outputIdParam);

                conn.Open();
                cmd.ExecuteNonQuery();

                string generatedId = outputIdParam.Value.ToString();
                return Ok(new { Message = "Group inserted successfully.", GroupId = generatedId });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error inserting group: " + ex.Message);
        }
    }


    [AllowRole("Admin", "Sales")]
    [HttpPut("update-client/{clientId}")]
    public IActionResult UpdateClient([FromHeader(Name = "X-Bearer-Token")] string token, string clientId, [FromBody] ClientUpdateModel client)
    {
        try
        {
            var UPDATEDBY = HttpContext.Session.GetString("LoggedInUsername");
            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("SqlServerConnection")))
            using (SqlCommand cmd = new SqlCommand("UpdateClientMaster", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@CLT_CLIENT_ID", clientId);
                cmd.Parameters.AddWithValue("@CLT_MOBILE_NO", client.CLT_MOBILE_NO ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@CLT_CLIENT_FIRSTNAME", client.CLT_CLIENT_FIRSTNAME ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@CLT_CLIENT_LASTNAME", client.CLT_CLIENT_LASTNAME ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@CLT_UPDATED_BY", UPDATEDBY);

                conn.Open();
                int result = cmd.ExecuteNonQuery();
                conn.Close();

                return Ok(new { Message = "Client updated", RowsAffected = result });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Update failed", Error = ex.Message });
        }
    }
}
