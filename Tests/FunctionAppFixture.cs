using Azure.Communication.Email;
using BuySignalNotifications;
using FakeItEasy;
using Finance.Net.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Tests;

[CollectionDefinition(nameof(FunctionAppCollection))]
public class FunctionAppCollection : ICollectionFixture<AzuriteFixture>, ICollectionFixture<FunctionAppFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}


public sealed class FunctionAppFixture : IAsyncLifetime
{
    private readonly IHost _host;

    public IServiceProvider ServiceProvider => _host.Services;

    public FunctionAppFixture(AzuriteFixture azuriteFixture)
    {
        _host = new HostBuilder()
            .ConfigureAppConfiguration(config =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:AzureStorage"] = azuriteFixture.Container.GetConnectionString(),
                    ["ConnectionStrings:AcsEmailConnectionString"] = "endpoint=https://test.communication.azure.com/;accesskey=fake-key-for-testing",
                    ["BuySignalNotifier:SenderEmailAddress"] = "donotreply@buysignalnotifications.com"
                });
            })
            .ConfigureServices((context, services) => services
                .AddBuySignalNotificationServices(context.Configuration)
                .AddSingleton(A.Fake<IYahooFinanceService>())
                .AddSingleton(A.Fake<EmailClient>())
            )
            .Build();
    }

    public async ValueTask InitializeAsync() => await _host.StartAsync();

    public ValueTask DisposeAsync()
    {
        _host?.Dispose();
        return ValueTask.CompletedTask;
    }
}