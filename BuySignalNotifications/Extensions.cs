using Azure.Communication.Email;
using Azure.Storage.Blobs;
using Finance.Net.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuySignalNotifications;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBuySignalNotificationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddSingleton(provider =>
            {
                var connectionString = configuration.GetConnectionString("AzureStorage");
                return new BlobServiceClient(connectionString);
            })
            .AddSingleton(provider =>
            {
                var blobServiceClient = provider.GetRequiredService<BlobServiceClient>();
                return blobServiceClient.GetBlobContainerClient("watchlists");
            })
            .AddSingleton(provider => new EmailClient(configuration.GetConnectionString("AcsEmailConnectionString")))
            .AddTransient<IWatchListDataService, WatchListDataService>()
            .AddTransient<IBuySignalDetector, BuySignalDetector>()
            .AddTransient<IBuySignalNotifier, BuySignalNotifier>()
            .AddTransient<IGetCandlesOfMostRecentTradingDayDataService, GetCandlesOfMostRecentTradingDayDataService>()
            .AddTransient<BuySignalFunction>()
            .AddHttpClient()
            .Configure<BuySignalNotifierOptions>(configuration.GetSection("BuySignalNotifier"))
            .AddFinanceNet();
        
        return services;
    }
}
