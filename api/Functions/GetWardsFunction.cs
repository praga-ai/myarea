using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using MobileApp.Api.Services;

namespace MobileApp.Api.Functions;

public class GetWardsFunction
{
    private readonly IConfiguration _configuration;

    public GetWardsFunction(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [Function("GetWards")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "wards")] HttpRequest req)
    {
        try
        {
            var connectionString = _configuration["SqlConnectionString"] ??
                throw new InvalidOperationException("SqlConnectionString not configured");

            var service = new SurveyDataService(connectionString);
            var wards = await service.GetWardsAsync();

            return new OkObjectResult(wards);
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }
}
