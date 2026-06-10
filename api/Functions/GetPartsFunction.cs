using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using MobileApp.Api.Services;

namespace MobileApp.Api.Functions;

public class GetPartsFunction
{
    private readonly IConfiguration _configuration;

    public GetPartsFunction(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [Function("GetParts")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "parts/{wardId}")] HttpRequest req,
        int wardId)
    {
        try
        {
            var connectionString = _configuration["SqlConnectionString"] ??
                throw new InvalidOperationException("SqlConnectionString not configured");

            var service = new SurveyDataService(connectionString);
            var parts = await service.GetPartsByWardIdAsync(wardId);

            return new OkObjectResult(parts);
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }
}
