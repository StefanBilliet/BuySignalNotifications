using Azure.Storage.Blobs;
using BuySignalNotifications;
using Finance.Net.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights()
    .AddSingleton(provider =>
    {
        var connectionString = builder.Configuration.GetConnectionString("AzureStorage");
        return new BlobServiceClient(connectionString);
    })
    .AddSingleton(provider =>
    {
        var blobServiceClient = provider.GetRequiredService<BlobServiceClient>();
        return blobServiceClient.GetBlobContainerClient(builder.Configuration.GetValue<string>("WatchlistContainerName") ?? "watchlists");
    })
    .AddTransient<WatchListDataService>()
    .AddTransient<BuySignalDetector>()
    .AddTransient<BuySignalNotifier>()
    .AddTransient<GetCandlesOfMostRecentTradingDayDataService>()
    .AddHttpClient()
    .Configure<BuySignalNotifierOptions>(builder.Configuration.GetSection("BuySignalNotifier"))
    .AddFinanceNet();

builder.Build().Run();