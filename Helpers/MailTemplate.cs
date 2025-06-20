using System.Text;

namespace AgentCreation.Utilities
{
     public static class EmailTemplateGenerator
    {
        public static string GetHtml(
            string fullName,
            string email,
            string password,
            EmailTemplateType templateType,
            WelcomeTemplateVariant? welcomeFormat = null)
        {
            string bodyContent = templateType switch
            {
                EmailTemplateType.Welcome => GetWelcomeBody(fullName, email, password, welcomeFormat ?? WelcomeTemplateVariant.Format1),
                EmailTemplateType.PasswordReset => GetPasswordResetBody(fullName, email, password),
                EmailTemplateType.AccountDeactivated => GetAccountDeactivatedBody(fullName),
                _ => GetWelcomeBody(fullName, email, password, WelcomeTemplateVariant.Format1),
            };

            return WrapWithHeaderAndFooter(bodyContent);
        }

        private static string WrapWithHeaderAndFooter(string bodyContent)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: Arial, sans-serif; background-color: #f2f2f2; }");
            sb.AppendLine(".container { max-width: 600px; margin: auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px; background-color: #ffffff; }");
            sb.AppendLine(".title { font-size: 24px; font-weight: bold; margin-bottom: 10px; color: #4CAF50; }");
            sb.AppendLine(".info { font-size: 16px; margin-top: 10px; }");
            sb.AppendLine(".footer { margin-top: 30px; font-size: 14px; color: #666; }");
            sb.AppendLine(".logo { text-align: center; margin-bottom: 20px; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head><body>");
            sb.AppendLine("<div class='container'>");

            sb.AppendLine("<div class='logo'><img src='https://yourcompanydomain.com/logo.png' alt='Company Logo' width='150' /></div>");
            sb.AppendLine(bodyContent);

            sb.AppendLine("<div class='footer'>");
            sb.AppendLine("<p>Regards,<br/>Support Team</p>");
            sb.AppendLine("<p><strong>Registered Office:</strong><br/>XYZ Pvt Ltd,<br/>123 Business Park, Tech City,<br/>Chennai - 600042, India</p>");
            sb.AppendLine("<p>Email: support@yourcompany.com | Phone: +91-9876543210</p>");
            sb.AppendLine("</div>");

            sb.AppendLine("</div></body></html>");
            return sb.ToString();
        }

        private static string GetWelcomeBody(string fullName, string email, string password, WelcomeTemplateVariant variant)
        {
            return variant switch
            {
                WelcomeTemplateVariant.Format2 => GetWelcomeFormat2(fullName, email, password),
                WelcomeTemplateVariant.Format3 => GetWelcomeFormat3(fullName, email, password),
                _ => GetWelcomeFormat1(fullName, email, password)
            };
        }

        private static string GetWelcomeFormat1(string fullName, string email, string password)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"<div class='title'>Welcome to Our Platform, {fullName}!</div>");
            sb.AppendLine("<div class='info'>");
            sb.AppendLine("<p>Your account has been created successfully.</p>");
            sb.AppendLine($"<p><strong>Email:</strong> {email}</p>");
            sb.AppendLine($"<p><strong>Temporary Password:</strong> {password}</p>");
            sb.AppendLine("<p>Please log in and change your password immediately.</p>");
            sb.AppendLine("</div>");
            return sb.ToString();
        }

        private static string GetWelcomeFormat2(string fullName, string email, string password)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<div style='background-color:#ffffff; padding:20px; border:1px solid #e0e0e0; border-radius:8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);'>");
            sb.AppendLine($"<h2 style='color:#2c3e50; text-align:center;'>🎉 Welcome, {fullName}!</h2>");
            sb.AppendLine("<p style='font-size:16px; color:#555;'>We're excited to have you onboard.</p>");
            sb.AppendLine("<table style='width:100%; margin-top:10px; font-size:16px;'>");
            sb.AppendLine($"<tr><td><strong>Email:</strong></td><td>{email}</td></tr>");
            sb.AppendLine($"<tr><td><strong>Password:</strong></td><td>{password}</td></tr>");
            sb.AppendLine("</table>");
            sb.AppendLine("<p style='margin-top:15px;'>Please log in and <strong>change your password</strong> immediately.</p>");
            sb.AppendLine("<div style='text-align:center; margin-top:20px;'>");
            sb.AppendLine("<a href='https://yourdomain.com/login' style='padding:10px 20px; background-color:#4CAF50; color:#fff; border-radius:4px; text-decoration:none;'>Login Now</a>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
            return sb.ToString();
        }

        private static string GetWelcomeFormat3(string fullName, string email, string password)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<div style='text-align:center;'>");
            sb.AppendLine($"<h2 style='color:#34495e;'>Hi {fullName},</h2>");
            sb.AppendLine("<p style='font-size:15px;'>Welcome to our service! Your login credentials are:</p>");
            sb.AppendLine($"<p><b>Email:</b> {email}</p>");
            sb.AppendLine($"<p><b>Password:</b> {password}</p>");
            sb.AppendLine("<p>Use the button below to login:</p>");
            sb.AppendLine("<a href='https://yourdomain.com/login' style='padding:12px 20px; background-color:#2980b9; color:#fff; text-decoration:none; border-radius:5px;'>Go to Dashboard</a>");
            sb.AppendLine("<p style='margin-top:20px;'>Please update your password after login.</p>");
            sb.AppendLine("</div>");
            return sb.ToString();
        }

        private static string GetPasswordResetBody(string fullName, string email, string password)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<div class='title'>Password Reset Requested</div>");
            sb.AppendLine("<div class='info'>");
            sb.AppendLine($"<p>Hi {fullName},</p>");
            sb.AppendLine("<p>You requested a password reset.</p>");
            sb.AppendLine($"<p><strong>Email:</strong> {email}</p>");
            sb.AppendLine($"<p><strong>Temporary Password:</strong> {password}</p>");
            sb.AppendLine("<p>Please change your password after logging in.</p>");
            sb.AppendLine("</div>");
            return sb.ToString();
        }

        private static string GetAccountDeactivatedBody(string fullName)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<div class='title'>Account Deactivated</div>");
            sb.AppendLine("<div class='info'>");
            sb.AppendLine($"<p>Dear {fullName},</p>");
            sb.AppendLine("<p>Your account has been deactivated. Please contact support if you believe this is a mistake.</p>");
            sb.AppendLine("</div>");
            return sb.ToString();
        }
    }
}
