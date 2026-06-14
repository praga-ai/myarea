using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using MobileApp.Api.Models;
using MobileApp.Api.Services;

namespace MobileApp.Api.Functions;

public class RegisterFunction
{
    private readonly IConfiguration _configuration;

    public RegisterFunction(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [Function("Register")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/register")] HttpRequest req)
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
            var (success, message, user) = await authService.RegisterAsync(registerRequest);

            if (success)
            {
                return new OkObjectResult(new RegisterResponse
                {
                    Success = true,
                    Message = message,
                    User = user
                });
            }

            return new BadRequestObjectResult(new RegisterResponse
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
