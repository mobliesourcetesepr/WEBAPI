using System.Text;

public static class WelcomeEmailTemplate
{
    public static string GetHtml(string fullName, string email, string password)
    {
        var sb = new StringBuilder();

        sb.AppendLine("<html>");
        sb.AppendLine("<head>");
        sb.AppendLine("<style>");
        sb.AppendLine("body { font-family: Arial, sans-serif; background-color: #f2f2f2; }");
        sb.AppendLine(".container { max-width: 600px; margin: auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px; background-color: #ffffff; }");
        sb.AppendLine(".title { color: #4CAF50; font-size: 24px; font-weight: bold; margin-bottom: 10px; }");
        sb.AppendLine(".info { font-size: 16px; margin-top: 10px; }");
        sb.AppendLine(".footer { margin-top: 30px; font-size: 14px; color: #666; }");
        sb.AppendLine(".logo { text-align: center; margin-bottom: 20px; }");
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("<div class='container'>");

        // Logo
        sb.AppendLine("<div class='logo'>");
        sb.AppendLine("<img src='https://yourcompanydomain.com/logo.png' alt='Company Logo' width='150' />");
        sb.AppendLine("</div>");

        // Title and Welcome Message
        sb.AppendLine($"<div class='title'>Welcome to Our Platform, {fullName}!</div>");
        sb.AppendLine("<div class='info'>");
        sb.AppendLine("<p>Your account has been created successfully.</p>");
        sb.AppendLine($"<p><strong>Email:</strong> {email}</p>");
        sb.AppendLine($"<p><strong>Temporary Password:</strong> {password}</p>");
        sb.AppendLine("<p>Please log in and change your password immediately.</p>");
        sb.AppendLine("</div>");

        // Footer
        sb.AppendLine("<div class='footer'>");
        sb.AppendLine("<p>Regards,<br/>Support Team</p>");
        sb.AppendLine("<p><strong>Registered Office:</strong><br/>");
        sb.AppendLine("XYZ Pvt Ltd,<br/>");
        sb.AppendLine("123 Business Park, Tech City,<br/>");
        sb.AppendLine("Chennai - 600042, India</p>");
        sb.AppendLine("<p>Email: support@yourcompany.com | Phone: +91-9876543210</p>");
        sb.AppendLine("</div>");

        sb.AppendLine("</div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }
}
