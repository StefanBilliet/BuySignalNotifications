using Azure.Storage.Blobs;
using Testcontainers.Azurite;
[assembly: AssemblyFixture(typeof(Tests.AzuriteFixture))]

namespace Tests;

public class AzuriteFixture : IAsyncLifetime
{
    public AzuriteContainer Container { get; private set; } = null!;
    public BlobServiceClient BlobServiceClient { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        Container = new AzuriteBuilder()
            .WithImage("mcr.microsoft.com/azure-storage/azurite:latest")
            .Build();
        await Container.StartAsync();

        BlobServiceClient = new BlobServiceClient(Container.GetConnectionString());
        
        var containerClient = BlobServiceClient.GetBlobContainerClient("watchlists");
        await containerClient.CreateIfNotExistsAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await Container.StopAsync();
    }
}