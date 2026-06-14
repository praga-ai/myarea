using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using MobileApp.Api.Models;
using MobileApp.Api.Services;

namespace MobileApp.Api.Functions;

public class LoginFunction
{
    private readonly IConfiguration _configuration;

    public LoginFunction(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [Function("Login")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/login")] HttpRequest req)
    {
        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var loginRequest = JsonSerializer.Deserialize<LoginRequest>(requestBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (loginRequest == null)
            {
                return new BadRequestObjectResult(new { error = "Invalid request body" });
            }

            var connectionString = _configuration["SqlConnectionString"] ??
                throw new InvalidOperationException("SqlConnectionString not configured");

            var authService = new AuthService(connectionString);

            // Capture client IP + user agent for the audit log.
            // Azure surfaces the client IP via X-Forwarded-For (may include :port) or X-Azure-ClientIP.
            string? ipAddress = null;
            foreach (var h in new[] { "X-Forwarded-For", "X-Azure-ClientIP", "X-Client-IP" })
            {
                var v = req.Headers[h].ToString();
                if (!string.IsNullOrWhiteSpace(v)) { ipAddress = v.Split(',')[0].Trim(); break; }
            }
            ipAddress ??= req.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            // strip :port for IPv4 form "1.2.3.4:5678" (single colon = IPv4, not IPv6)
            if (!string.IsNullOrEmpty(ipAddress) && ipAddress.Count(c => c == ':') == 1)
                ipAddress = ipAddress.Split(':')[0];
            var userAgent = req.Headers["User-Agent"].ToString();

            var (success, message, token, user) = await authService.LoginAsync(loginRequest, ipAddress, userAgent);

            if (success)
            {
                return new OkObjectResult(new LoginResponse
                {
                    Success = true,
                    Message = message,
                    Token = token,
                    User = user
                });
            }

            return new UnauthorizedObjectResult(new LoginResponse
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
