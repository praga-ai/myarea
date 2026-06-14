using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using MobileApp.Api.Services;

namespace MobileApp.Api.Functions;

public class GetUsersFunction
{
    private readonly IConfiguration _configuration;

    public GetUsersFunction(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [Function("GetUsers")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "auth/users")] HttpRequest req)
    {
        try
        {
            // Extract and verify token
            var token = req.Headers["Authorization"].ToString().Replace("Bearer ", "");

            if (string.IsNullOrEmpty(token))
            {
                return new UnauthorizedObjectResult(new { error = "Authorization token required" });
            }

            var connectionString = _configuration["SqlConnectionString"] ??
                throw new InvalidOperationException("SqlConnectionString not configured");

            var authService = new AuthService(connectionString);
            var (valid, userId, email, role) = authService.VerifyToken(token);

            if (!valid)
            {
                return new UnauthorizedObjectResult(new { error = "Invalid or expired token" });
            }

            // Only Admin can access this endpoint
            if (role != "Admin")
            {
                return new ObjectResult(new { error = "Only administrators can access this endpoint" })
                {
                    StatusCode = 403
                };
            }

            var users = await authService.GetAllUsersAsync();

            return new OkObjectResult(new { users, total = users.Count });
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }
}
