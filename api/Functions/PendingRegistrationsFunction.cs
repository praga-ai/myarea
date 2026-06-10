using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using MobileApp.Api.Models;
using MobileApp.Api.Services;

namespace MobileApp.Api.Functions;

public class PendingRegistrationsFunction
{
    private readonly IConfiguration _configuration;

    public PendingRegistrationsFunction(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [Function("PendingRegistrations")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "auth/pending-registrations")] HttpRequest req)
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

            // Get pending registrations
            var pendingRegistrations = await authService.GetPendingRegistrationsAsync();

            return new OkObjectResult(new PendingRegistrationsResponse
            {
                Registrations = pendingRegistrations
            });
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }
}
