using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using MobileApp.Api.Models;
using MobileApp.Api.Services;

namespace MobileApp.Api.Functions;

public class ApproveRegistrationFunction
{
    private readonly IConfiguration _configuration;

    public ApproveRegistrationFunction(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [Function("ApproveRegistration")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/approve-registration")] HttpRequest req)
    {
        try
        {
            // Verify JWT token and admin role
            var authHeader = req.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader))
            {
                return new UnauthorizedObjectResult(new { error = "Authorization header missing" });
            }

            var token = authHeader.Replace("Bearer ", string.Empty);
            var connectionString = _configuration["SqlConnectionString"] ??
                throw new InvalidOperationException("SqlConnectionString not configured");

            var authService = new AuthService(connectionString);
            var (valid, userId, email, role) = authService.VerifyToken(token);

            if (!valid || role != "Admin")
            {
                return new UnauthorizedObjectResult(new { error = "Unauthorized. Admin role required." });
            }

            // Parse request
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var approveRequest = JsonSerializer.Deserialize<ApproveRegistrationRequest>(requestBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (approveRequest == null || string.IsNullOrEmpty(approveRequest.Email))
            {
                return new BadRequestObjectResult(new { error = "Email is required" });
            }

            // Approve registration
            var (success, message, user) = await authService.ApproveRegistrationAsync(approveRequest.Email);

            if (success && user != null)
            {
                // Send confirmation email
                var emailService = new EmailService(_configuration);
                await emailService.SendConfirmationEmailAsync(approveRequest.Email, user.FullName, user.RoleName);

                return new OkObjectResult(new ApproveRegistrationResponse
                {
                    Success = true,
                    Message = message,
                    User = user
                });
            }

            return new BadRequestObjectResult(new ApproveRegistrationResponse
            {
                Success = false,
                Message = message
            });
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }
}
