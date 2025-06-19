using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.Data.SqlClient;
using AgentCreation.Data;
using AgentCreation.Services;

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


        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
              Console.WriteLine("User: " + request?.Username);
              Console.WriteLine("User: " + request?.Password);
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            return BadRequest("Username and Password are required");

            var user = _context.AdminUser
                .FirstOrDefault(u => u.Username == request.Username && u.Password == request.Password);
            Console.WriteLine("User: " + user?.Username);
            if (user == null)
                return Unauthorized("Invalid username or password");

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


    [HttpPost("agentcreation")]
    public IActionResult InsertClient([FromBody] ClientMasterModel client)
    {
        try
        {

            var CREATEDBY = HttpContext.Session.GetString("LoggedInUsername");
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
                cmd.Parameters.AddWithValue("@CLT_CREATED_BY",  CREATEDBY);

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
    [AllowRole("Admin", "Sales")]
    [HttpPut("update-client/{clientId}")]
    public IActionResult UpdateClient([FromHeader(Name = "X-Bearer-Token")] string token,string clientId, [FromBody] ClientUpdateModel client)
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
                cmd.Parameters.AddWithValue("@CLT_UPDATED_BY",  UPDATEDBY);

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
