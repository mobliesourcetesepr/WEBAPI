using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.Data.SqlClient;
using AgentCreation.Data;

[Route("api")]
[ApiController]
public class ClientMasterController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly UserDbContext _context;

    public ClientMasterController(IConfiguration configuration, UserDbContext context)
    {
        _configuration = configuration;
        _context = context;
    }

 [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            return BadRequest("Username and Password are required");

        // 🔍 Validate using DbContext
        var user = _context.AdminUser
            .FirstOrDefault(u => u.Username == request.Username && u.Password == request.Password);

        if (user == null)
            return Unauthorized("Invalid username or password");

        // ✅ Save username in session
        HttpContext.Session.SetString("LoggedInUsername", user.Username);

        return Ok(new
        {
            Message = "Login successful",
        });
    }



    [HttpPost("agentcreation")]
    public IActionResult InsertClient([FromBody] ClientMasterModel client)
    {
        try
        {

            client.CLT_CREATED_BY = HttpContext.Session.GetString("LoggedInUsername");
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
                cmd.Parameters.AddWithValue("@CLT_CREATED_BY",  client.CLT_CREATED_BY ?? (object)DBNull.Value);

                conn.Open();
                int result = cmd.ExecuteNonQuery();
                conn.Close();

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

    [HttpPut("update-client/{clientId}")]
    public IActionResult UpdateClient(string clientId, [FromBody] ClientUpdateModel client)
    {
        try
        {
             client.CLT_UPDATED_BY = HttpContext.Session.GetString("LoggedInUsername");
            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("SqlServerConnection")))
            using (SqlCommand cmd = new SqlCommand("UpdateClientMaster", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@CLT_CLIENT_ID", clientId);
                cmd.Parameters.AddWithValue("@CLT_MOBILE_NO", client.CLT_MOBILE_NO ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@CLT_CLIENT_FIRSTNAME", client.CLT_CLIENT_FIRSTNAME ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@CLT_CLIENT_LASTNAME", client.CLT_CLIENT_LASTNAME ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@CLT_UPDATED_BY",  client.CLT_UPDATED_BY ?? (object)DBNull.Value);

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
