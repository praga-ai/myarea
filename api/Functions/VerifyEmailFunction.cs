using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using MobileApp.Api.Models;
using MobileApp.Api.Services;

namespace MobileApp.Api.Functions;

public class VerifyEmailFunction
{
    private readonly IConfiguration _configuration;

    public VerifyEmailFunction(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [Function("VerifyEmail")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/verify-email")] HttpRequest req)
    {
        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var verifyRequest = JsonSerializer.Deserialize<VerifyEmailRequest>(requestBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (verifyRequest == null)
            {
                return new BadRequestObjectResult(new { error = "Invalid request body" });
            }

            var connectionString = _configuration["SqlConnectionString"] ??
                throw new InvalidOperationException("SqlConnectionString not configured");

            var authService = new AuthService(connectionString);
            var (success, message) = await authService.VerifyEmailAsync(verifyRequest.Email, verifyRequest.VerificationCode);

            if (success)
            {
                return new OkObjectResult(new VerifyEmailResponse
                {
                    Success = true,
                    Message = message
                });
            }

            return new BadRequestObjectResult(new VerifyEmailResponse
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
