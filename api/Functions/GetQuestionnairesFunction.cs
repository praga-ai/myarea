using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using MobileApp.Api.Services;

namespace MobileApp.Api.Functions;

public class GetQuestionnairesFunction
{
    private readonly IConfiguration _configuration;

    public GetQuestionnairesFunction(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [Function("GetQuestionnaires")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "questionnaires")] HttpRequest req)
    {
        try
        {
            var connectionString = _configuration["SqlConnectionString"] ??
                throw new InvalidOperationException("SqlConnectionString not configured");

            var service = new SurveyDataService(connectionString);
            var questionnaires = await service.GetQuestionnairesAsync();

            return new OkObjectResult(questionnaires);
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }
}
