using System.Text.Json;
using Azure.Storage.Blobs;

namespace BuySignalNotifier;

public interface IWatchListDataService
{
    Task<IReadOnlyCollection<Watchlist>> GetWatchLists(CancellationToken cancellationToken);
}

public class WatchListDataService : IWatchListDataService
{
    private readonly BlobContainerClient _watchlistsContainerClient;

    public WatchListDataService(BlobContainerClient watchlistsContainerClient)
    {
        _watchlistsContainerClient = watchlistsContainerClient;
    }

    public async Task<IReadOnlyCollection<Watchlist>> GetWatchLists(CancellationToken cancellationToken)
    {
        var blobs = _watchlistsContainerClient.GetBlobsAsync(cancellationToken: cancellationToken);
        
        var downloadedWatchlists = new List<Watchlist?>();
        await foreach (var blob in blobs)
        {
            var blobClient = _watchlistsContainerClient.GetBlobClient(blob.Name);
            var response = await blobClient.DownloadAsync(cancellationToken: cancellationToken);
            downloadedWatchlists.Add(await JsonSerializer.DeserializeAsync<Watchlist>(response.Value.Content, cancellationToken: cancellationToken));
        }

        return downloadedWatchlists.Where(watchlist => watchlist != null).OfType<Watchlist>().ToArray();
    }
}