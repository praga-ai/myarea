using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using MobileApp.Api.Models;
using MobileApp.Api.Services;

namespace MobileApp.Api.Functions;

public class VerifyTokenFunction
{
    private readonly IConfiguration _configuration;

    public VerifyTokenFunction(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [Function("VerifyToken")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/verify-token")] HttpRequest req)
    {
        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var verifyRequest = JsonSerializer.Deserialize<VerifyTokenRequest>(requestBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (verifyRequest == null || string.IsNullOrEmpty(verifyRequest.Token))
            {
                return new BadRequestObjectResult(new { error = "Token is required" });
            }

            var connectionString = _configuration["SqlConnectionString"] ??
                throw new InvalidOperationException("SqlConnectionString not configured");

            var authService = new AuthService(connectionString);
            var (valid, userId, email, role) = authService.VerifyToken(verifyRequest.Token);

            if (!valid)
            {
                return new UnauthorizedObjectResult(new { error = "Invalid or expired token" });
            }

            var user = await authService.GetUserByIdAsync(userId);

            return new OkObjectResult(new VerifyTokenResponse
            {
                Valid = true,
                User = user ?? new UserDto()
            });
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }
}
