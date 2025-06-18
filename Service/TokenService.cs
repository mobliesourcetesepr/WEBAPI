using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

public class TokenService
{
    private readonly IConfiguration _config;
    public DateTime TokenExpiry { get; private set; } // <-- store expiry here
    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    // public string GenerateToken(string username, string role)
    // {
    //     var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
    //     var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    //     TokenExpiry = DateTime.Now.Add(TimeSpan.FromMinutes(3)); // Save the expiry value

    //     var token = new JwtSecurityToken(
    //         issuer: _config["Jwt:Issuer"],
    //         audience: _config["Jwt:Audience"],
    //         claims: new[]
    //         {
    //             new Claim(ClaimTypes.Name, username),
    //             new Claim(ClaimTypes.Role, role)
    //         },
    //         expires:TokenExpiry,
    //         signingCredentials: creds
    //     );

    //     // <-- store expiry here
    //     return new JwtSecurityTokenHandler().WriteToken(token);
    // }



 public string GenerateToken(string username, string role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        TokenExpiry = DateTime.Now.AddMinutes(15); // Absolute expiry

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            },
            expires: TokenExpiry,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}
