using System.Text.Json;
using Azure.Storage.Blobs;
using BuySignalNotifier;

namespace Tests;

public class WatchListDataServiceTests
{
    private readonly BlobContainerClient _watchlistsContainerClient;
    private readonly WatchListDataService _sut;

    public WatchListDataServiceTests(AzuriteFixture azuriteFixture)
    {
        _watchlistsContainerClient = azuriteFixture.BlobServiceClient.GetBlobContainerClient("watchlists");
        _sut = new WatchListDataService(_watchlistsContainerClient);
    }

    [Fact]
    public async Task GIVEN_watch_list_json_blob_WHEN_GetWatchLists_THEN_return_all_watch_lists()
    {
        var currentCancellationToken = TestContext.Current.CancellationToken;
        var seededWatchList = new Watchlist("joskevermeulen@huppeldepup.com",
            new WatchlistEntry("VRSN", 289m),
            new WatchlistEntry("CAKE", 60.51m)
        );
        var blobClient = _watchlistsContainerClient.GetBlobClient("buylist_may.json");
        await blobClient.UploadAsync(BinaryData.FromString(JsonSerializer.Serialize(seededWatchList)), overwrite: true, cancellationToken: currentCancellationToken);

        var watchLists = await _sut.GetWatchLists(currentCancellationToken);
        
        var watchlist = Assert.Single(watchLists);
        Assert.Equivalent(watchlist, seededWatchList);
    }
}