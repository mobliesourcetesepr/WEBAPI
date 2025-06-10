using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using WEBAPI.Models;
using WEBAPI.DataMaintain;
using WEBAPI.Helpers;
using System.Text.Json;
using System.Net;
using System.Net.Mail;
[Route("api/secure")]
[ApiController]
public class SecureController : ControllerBase
{
    private readonly AppDbContext _context;

    public SecureController(AppDbContext context)
    {
        _context = context;
    }

    // POST: api/secure/usercreation
    [HttpPost("usercreation")]
   public IActionResult Logincreation([FromBody] User user)
    {
    if (user == null || string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password))
        return BadRequest("Username and password are required.");

    try
    {
        // Attempt to insert user into the database
        _context.Users.Add(user);
        int result = _context.SaveChanges();

        if (result > 0)
        {
            return Ok(new { Message = "Data inserted successfully" });
        }
        else
        {
            return StatusCode(500, new { Message = "Unable to insert data" });
        }
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { Message = "Error inserting data", Error = ex.Message });
    }
}



[HttpPost("login")]
public IActionResult Login([FromBody] RequestModel request)
{
    try
    {
        // Step 1: Decrypt input using the property from request
        string decryptedJson = AesEncryption.Decrypt(request.EncryptedPayload);

        // Step 2: Deserialize decrypted JSON to User object
        var login = JsonSerializer.Deserialize<User>(decryptedJson);

        if (login == null)
            return BadRequest("Invalid login data");

        // Step 3: Validate user credentials
        var user = _context.Users.FirstOrDefault(u =>
            u.Username == login.Username &&
            u.Password == login.Password);

            string token = AesEncryption.GenerateToken(user.Username);

        // Step 4: Return success or failure message
        if (user != null)
            return Ok(new { message = "Login successful", user = user.Username ,authToken = token});
        else
            return Unauthorized("Invalid username or password");
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"Error occurred: {ex.Message}");
    }
}

    [HttpPost("securedata")]
    public IActionResult GetSecureData([FromHeader(Name = "Authorization")] string token)
    {

        if (!AesEncryption.IsTokenValid(token, out var session))
        {       
            return Unauthorized(new { message = "Invalid or expired token" });
        }

        // Token is valid; allow access to protected data

        
    // ✅ Simulate fetching user-specific data
    string username = session.Username.ToLower();

    // ✅ Real use case: calculate discount
    decimal basePrice = 1000m;
    decimal discountPercent = 0;

    if (username.Contains("vip"))
        discountPercent = 45;
    else if (username.Contains("new"))
        discountPercent = 35;
    else
        discountPercent = 25;

    decimal discountAmount = basePrice * (discountPercent / 100);
    decimal finalPrice = basePrice - discountAmount;



    return Ok(new
    {
        message = "Access granted",
        user = session.Username,
        basePrice = basePrice,
        discount = $"{discountPercent}%",
        amountSaved = discountAmount,
        finalPrice = finalPrice
    });
    }


    [HttpPost("send")]
    public async Task<IActionResult> SendOtp([FromBody] OtpRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Contact))
            return BadRequest("Contact (Email or Mobile) is required.");

        string otp = GenerateOtp();

        // Send OTP
        if (request.Type.ToLower() == "email")
        {
            bool sent = await SendOtpEmail(request.Contact, otp);
            if (!sent)
                return StatusCode(500, "Failed to send OTP via email");
        }
        else if (request.Type.ToLower() == "mobile")
        {
            bool sent = SendOtpSms(request.Contact, otp); // Replace with real SMS sending
            if (!sent)
                return StatusCode(500, "Failed to send OTP via SMS");
        }
        else
        {
            return BadRequest("Invalid type. Use 'email' or 'mobile'.");
        }

        return Ok(new { message = "OTP sent successfully", otp }); // Don't return OTP in real app
    }

    private string GenerateOtp()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    private async Task<bool> SendOtpEmail(string email, string otp)
    {
        try
        {
            var mail = new MailMessage();
            mail.To.Add(email);
            mail.From = new MailAddress("support@shreyastechsolutions.in");
            mail.Subject = "Your OTP Code";
            mail.Body = $"Your OTP is: {otp}";
            mail.IsBodyHtml = false;

            using (var smtp = new SmtpClient("smtp.zoho.in"))
            {
                smtp.Port = 587;
                smtp.Credentials = new NetworkCredential("support@shreyastechsolutions.in", "St$$upport12");
                smtp.EnableSsl = true;
                await smtp.SendMailAsync(mail);
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private bool SendOtpSms(string mobile, string otp)
    {
        try
        {
            // Use SMS provider like Twilio, Msg91, etc.
            // Placeholder:
            Console.WriteLine($"SMS sent to {mobile}: Your OTP is {otp}");
            return true;
        }
        catch
        {
            return false;
        }
    }

}




