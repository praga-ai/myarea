using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MobileApp.Api.Models;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        var keyVaultUri = new Uri(
            Environment.GetEnvironmentVariable("KeyVaultUri")
            ?? throw new InvalidOperationException("KeyVaultUri is not set"));

        // Managed Identity — no credentials in code
        var credential = new DefaultAzureCredential();
        services.AddSingleton(new SecretClient(keyVaultUri, credential));

        services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();
    })
    .Build();

host.Run();
