public static class WelcomeEmailTemplate
{
    public static string GetHtml(string fullName, string email, string password)
    {
        return $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; }}
                    .container {{
                        padding: 20px;
                        border: 1px solid #ddd;
                        border-radius: 10px;
                        background-color: #f9f9f9;
                    }}
                    .title {{
                        color: #4CAF50;
                        font-size: 24px;
                        font-weight: bold;
                    }}
                    .info {{
                        margin-top: 10px;
                        font-size: 16px;
                    }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='title'>Welcome to Our Platform, {fullName}!</div>
                    <div class='info'>
                        <p>Your account has been created successfully.</p>
                        <p><strong>Email:</strong> {email}</p>
                        <p><strong>Temporary Password:</strong> {password}</p>
                        <p>Please log in and change your password immediately.</p>
                        <p>Regards,<br/>Support Team</p>
                    </div>
                </div>
            </body>
            </html>
        ";
    }
}
