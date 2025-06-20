using System.Net;
using System.Net.Mail;

public class EmailSender
{
    private readonly IConfiguration _config;

    public EmailSender(IConfiguration config)
    {
        _config = config;
    }

    public void SendEmail(string toEmail, string subject, string htmlBody)
    {
        var smtpClient = new SmtpClient(_config["Email:SmtpServer"])
        {
            Port = int.Parse(_config["Email:Port"]),
            Credentials = new NetworkCredential(_config["Email:Username"], _config["Email:Password"]),
            EnableSsl = true,
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_config["Email:From"]),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true,
        };

        mailMessage.To.Add(toEmail);
        smtpClient.Send(mailMessage);
    }
}
