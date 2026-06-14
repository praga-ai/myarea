using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace MobileApp.Api.Services;

public class EmailService
{
    private readonly string _sendGridApiKey;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly HttpClient _httpClient;

    public EmailService(IConfiguration configuration)
    {
        _sendGridApiKey = configuration["SENDGRID_API_KEY"] ?? configuration["EmailServiceKey"] ?? string.Empty;
        _fromEmail = configuration["EMAIL_FROM_ADDRESS"] ?? "noreply@myarea.com";
        _fromName = configuration["EMAIL_FROM_NAME"] ?? "MyArea Admin";
        _httpClient = new HttpClient();
    }

    public async Task<bool> SendVerificationEmailAsync(string toEmail, string verificationCode)
    {
        try
        {
            var subject = "Email Verification - MyArea";
            var htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #8B1538 0%, #6B0E2A 100%); color: white; padding: 20px; border-radius: 8px; text-align: center; }}
        .content {{ padding: 20px; border: 1px solid #eee; border-radius: 8px; margin-top: 20px; }}
        .code-box {{ background: #f5f5f5; border-left: 4px solid #FF9500; padding: 20px; margin: 20px 0; text-align: center; }}
        .code {{ font-size: 32px; font-weight: bold; color: #8B1538; letter-spacing: 8px; }}
        .footer {{ color: #999; font-size: 12px; margin-top: 20px; text-align: center; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>📍 MyArea</h1>
            <p>Email Verification</p>
        </div>

        <div class='content'>
            <p>Hello,</p>

            <p>Thank you for registering with MyArea. To complete your registration, please verify your email address using the code below:</p>

            <div class='code-box'>
                <p>Your verification code is:</p>
                <div class='code'>{verificationCode}</div>
            </div>

            <p><strong>Important:</strong> This code will expire in 24 hours.</p>

            <p>If you did not request this verification code, please ignore this email.</p>

            <p>Best regards,<br>MyArea Admin Team</p>
        </div>

        <div class='footer'>
            <p>&copy; 2026 MyArea. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

            var plainTextContent = $@"Email Verification - MyArea

Hello,

Thank you for registering with MyArea. To complete your registration, please verify your email address using the code below:

Your verification code is: {verificationCode}

Important: This code will expire in 24 hours.

If you did not request this verification code, please ignore this email.

Best regards,
MyArea Admin Team";

            return await SendEmailAsync(toEmail, subject, htmlContent, plainTextContent);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending verification email: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SendConfirmationEmailAsync(string toEmail, string fullName, string role)
    {
        try
        {
            var subject = "Account Approved - MyArea";
            var htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #8B1538 0%, #6B0E2A 100%); color: white; padding: 20px; border-radius: 8px; text-align: center; }}
        .content {{ padding: 20px; border: 1px solid #eee; border-radius: 8px; margin-top: 20px; }}
        .success {{ background: #f0fdf4; border-left: 4px solid #4caf50; padding: 15px; margin: 20px 0; border-radius: 4px; color: #2e7d32; }}
        .details {{ background: #f5f5f5; padding: 15px; border-radius: 4px; margin: 20px 0; }}
        .detail-row {{ margin: 10px 0; }}
        .label {{ font-weight: bold; color: #8B1538; }}
        .footer {{ color: #999; font-size: 12px; margin-top: 20px; text-align: center; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>📍 MyArea</h1>
            <p>Account Approval Confirmation</p>
        </div>

        <div class='content'>
            <p>Dear {fullName},</p>

            <div class='success'>
                <strong>✓ Your registration has been approved!</strong>
                <p>Your MyArea account is now active and ready to use.</p>
            </div>

            <h3>Account Details</h3>
            <div class='details'>
                <div class='detail-row'>
                    <span class='label'>Email:</span> {toEmail}
                </div>
                <div class='detail-row'>
                    <span class='label'>Role:</span> {role}
                </div>
                <div class='detail-row'>
                    <span class='label'>Status:</span> Active
                </div>
                <div class='detail-row'>
                    <span class='label'>Created:</span> {DateTime.UtcNow:MMMM dd, yyyy}
                </div>
            </div>

            <p>You can now login to MyArea with your registered email and password.</p>

            <p><a href='https://app-survey-admin-cs-dev.azurewebsites.net/login' style='background: linear-gradient(135deg, #8B1538 0%, #6B0E2A 100%); color: white; padding: 10px 20px; text-decoration: none; border-radius: 4px; display: inline-block;'>Login to MyArea</a></p>

            <p>Best regards,<br>MyArea Admin Team</p>
        </div>

        <div class='footer'>
            <p>&copy; 2026 MyArea. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

            var plainTextContent = $@"Account Approval Confirmation - MyArea

Dear {fullName},

Your registration has been approved!

Account Details:
Email: {toEmail}
Role: {role}
Status: Active
Created: {DateTime.UtcNow:MMMM dd, yyyy}

You can now login to MyArea with your registered email and password.

Best regards,
MyArea Admin Team";

            return await SendEmailAsync(toEmail, subject, htmlContent, plainTextContent);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending confirmation email: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SendRejectionEmailAsync(string toEmail)
    {
        try
        {
            var subject = "Registration Status - MyArea";
            var htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #8B1538 0%, #6B0E2A 100%); color: white; padding: 20px; border-radius: 8px; text-align: center; }}
        .content {{ padding: 20px; border: 1px solid #eee; border-radius: 8px; margin-top: 20px; }}
        .notice {{ background: #fff5f5; border-left: 4px solid #8B1538; padding: 15px; margin: 20px 0; border-radius: 4px; color: #8B1538; }}
        .footer {{ color: #999; font-size: 12px; margin-top: 20px; text-align: center; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>📍 MyArea</h1>
            <p>Registration Status Update</p>
        </div>

        <div class='content'>
            <p>Dear User,</p>

            <div class='notice'>
                <p>Unfortunately, your registration request has been rejected by the administrator.</p>
            </div>

            <p>If you have any questions or believe this decision was made in error, please contact the MyArea support team.</p>

            <p>Best regards,<br>MyArea Admin Team</p>
        </div>

        <div class='footer'>
            <p>&copy; 2026 MyArea. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

            var plainTextContent = $@"Registration Status Update - MyArea

Dear User,

Unfortunately, your registration request has been rejected by the administrator.

If you have any questions or believe this decision was made in error, please contact the MyArea support team.

Best regards,
MyArea Admin Team";

            return await SendEmailAsync(toEmail, subject, htmlContent, plainTextContent);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending rejection email: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlContent, string plainTextContent)
    {
        try
        {
            if (string.IsNullOrEmpty(_sendGridApiKey))
            {
                Console.WriteLine($"SendGrid API key not configured. Email would be sent to: {toEmail}");
                Console.WriteLine($"Subject: {subject}");
                return true; // Return true to allow workflow to continue
            }

            var request = new
            {
                personalizations = new[]
                {
                    new
                    {
                        to = new[] { new { email = toEmail } },
                        subject = subject
                    }
                },
                from = new { email = _fromEmail, name = _fromName },
                content = new[]
                {
                    new { type = "text/plain", value = plainTextContent },
                    new { type = "text/html", value = htmlContent }
                }
            };

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.sendgrid.com/v3/mail/send");
            httpRequest.Headers.Add("Authorization", $"Bearer {_sendGridApiKey}");
            httpRequest.Content = JsonContent.Create(request);

            var response = await _httpClient.SendAsync(httpRequest);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending email via SendGrid: {ex.Message}");
            return false;
        }
    }
}
