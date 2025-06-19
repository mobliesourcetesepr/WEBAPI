using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.Data.SqlClient;

[Route("api")]
[ApiController]
public class ClientMasterController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public ClientMasterController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("agentcreation")]
    public IActionResult InsertClient([FromBody] ClientMasterModel client)
    {
        using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("SqlServerConnection")))
        using (SqlCommand cmd = new SqlCommand("InsertClientMaster", conn))
        {
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@CLT_CLIENT_NAME", client.CLT_CLIENT_NAME ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@CLT_ADDRESS2", client.CLT_ADDRESS2 ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@CLT_STATE_ID", client.CLT_STATE_ID ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@CLT_CLIENT_TITLE", client.CLT_CLIENT_TITLE ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@CLT_COUNTRY_ID", client.CLT_COUNTRY_ID ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@CLT_CITY_ID", client.CLT_CITY_ID ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@CLT_ZIP_CODE", client.CLT_ZIP_CODE ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@CLT_CONTACT", client.CLT_CONTACT ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@CLT_EMAIL_ID", client.CLT_EMAIL_ID ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@CLT_CLIENT_LASTNAME", client.CLT_CLIENT_LASTNAME ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@CLT_CLIENT_FIRSTNAME", client.CLT_CLIENT_FIRSTNAME ?? (object)DBNull.Value);

            conn.Open();
            int result = cmd.ExecuteNonQuery();
            conn.Close();

            return Ok(new { Message = "Inserted Successfully", RowsAffected = result });
        }
    }
}
