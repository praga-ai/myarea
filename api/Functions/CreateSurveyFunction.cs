using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using MobileApp.Api.Models;
using MobileApp.Api.Services;

namespace MobileApp.Api.Functions;

public class CreateSurveyFunction
{
    private readonly IConfiguration _configuration;

    public CreateSurveyFunction(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [Function("CreateSurvey")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "survey")] HttpRequest req)
    {
        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var createRequest = JsonSerializer.Deserialize<CreateSurveyRequest>(requestBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (createRequest == null)
            {
                return new BadRequestObjectResult(new { error = "Invalid request body" });
            }

            if (createRequest.WardId <= 0 || createRequest.PartId <= 0 ||
                createRequest.AreaId <= 0 || createRequest.StreetId <= 0)
            {
                return new BadRequestObjectResult(new { error = "Invalid location selection" });
            }

            var connectionString = _configuration["SqlConnectionString"] ??
                throw new InvalidOperationException("SqlConnectionString not configured");

            var service = new SurveyDataService(connectionString);
            var surveyId = await service.CreateSurveyAsync(createRequest);

            return new OkObjectResult(new { surveyId, message = "Survey submitted successfully" });
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }
}
