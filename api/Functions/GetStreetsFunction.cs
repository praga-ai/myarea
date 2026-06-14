using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using MobileApp.Api.Services;

namespace MobileApp.Api.Functions;

public class GetStreetsFunction
{
    private readonly IConfiguration _configuration;

    public GetStreetsFunction(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [Function("GetStreets")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "streets/{areaId}")] HttpRequest req,
        int areaId)
    {
        try
        {
            var connectionString = _configuration["SqlConnectionString"] ??
                throw new InvalidOperationException("SqlConnectionString not configured");

            var service = new SurveyDataService(connectionString);
            var streets = await service.GetStreetsByAreaIdAsync(areaId);

            return new OkObjectResult(streets);
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }
}
