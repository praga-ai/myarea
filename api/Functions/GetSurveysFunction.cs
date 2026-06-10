using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using MobileApp.Api.Services;

namespace MobileApp.Api.Functions;

public class GetSurveysFunction
{
    private readonly IConfiguration _configuration;

    public GetSurveysFunction(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [Function("GetSurveys")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "surveys")] HttpRequest req)
    {
        try
        {
            var connectionString = _configuration["SqlConnectionString"] ??
                throw new InvalidOperationException("SqlConnectionString not configured");

            var service = new SurveyDataService(connectionString);
            var surveys = await service.GetAllSurveysAsync();

            return new OkObjectResult(surveys);
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }
}
