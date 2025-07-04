using System.Text.Json;
using Azure.Storage.Blobs;

namespace BuySignalNotifier;

public class WatchListDataService
{
    private readonly BlobContainerClient _watchListsContainerClient;

    public WatchListDataService(BlobContainerClient watchListsContainerClient)
    {
        _watchListsContainerClient = watchListsContainerClient;
    }

    public async Task<IReadOnlyCollection<Watchlist>> GetWatchLists(CancellationToken cancellationToken)
    {
        var blobs = _watchListsContainerClient.GetBlobsAsync(cancellationToken: cancellationToken);
        
        var downloadedWatchlists = new List<Watchlist?>();
        await foreach (var blob in blobs)
        {
            var blobClient = _watchListsContainerClient.GetBlobClient(blob.Name);
            var response = await blobClient.DownloadAsync(cancellationToken: cancellationToken);
            downloadedWatchlists.Add(await JsonSerializer.DeserializeAsync<Watchlist>(response.Value.Content, cancellationToken: cancellationToken));
        }

        return downloadedWatchlists.Where(watchlist => watchlist != null).OfType<Watchlist>().ToArray();
    }
}