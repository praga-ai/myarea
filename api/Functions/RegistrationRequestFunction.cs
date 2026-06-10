using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using MobileApp.Api.Models;
using MobileApp.Api.Services;

namespace MobileApp.Api.Functions;

public class RegistrationRequestFunction
{
    private readonly IConfiguration _configuration;

    public RegistrationRequestFunction(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [Function("RegistrationRequest")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/register-request")] HttpRequest req)
    {
        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var registerRequest = JsonSerializer.Deserialize<RegisterRequest>(requestBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (registerRequest == null)
            {
                return new BadRequestObjectResult(new { error = "Invalid request body" });
            }

            var connectionString = _configuration["SqlConnectionString"] ??
                throw new InvalidOperationException("SqlConnectionString not configured");

            var authService = new AuthService(connectionString);
            var (success, message, registrationId) = await authService.RegisterRequestAsync(registerRequest);

            if (success)
            {
                // Send verification email
                var emailService = new EmailService(_configuration);
                var verificationCode = authService.GenerateVerificationCode();

                // In production, save and send the code via email
                // For now, log it and let the user know to check their email
                await emailService.SendVerificationEmailAsync(registerRequest.Email, verificationCode);

                return new OkObjectResult(new
                {
                    success = true,
                    message = message,
                    registrationId = registrationId
                });
            }

            return new BadRequestObjectResult(new
            {
                success = false,
                message = message
            });
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }
}
