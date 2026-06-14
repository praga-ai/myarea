using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using MobileApp.Api.Services;

namespace MobileApp.Api.Functions;

public class GetAreasFunction
{
    private readonly IConfiguration _configuration;

    public GetAreasFunction(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [Function("GetAreas")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "areas/{partId}")] HttpRequest req,
        int partId)
    {
        try
        {
            var connectionString = _configuration["SqlConnectionString"] ??
                throw new InvalidOperationException("SqlConnectionString not configured");

            var service = new SurveyDataService(connectionString);
            var areas = await service.GetAreasByPartIdAsync(partId);

            return new OkObjectResult(areas);
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }
}
