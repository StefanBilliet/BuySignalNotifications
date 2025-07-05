namespace BuySignalNotifications;

public interface IBuySignalDetector
{
    Task<IReadOnlyCollection<Watchlist>> SearchForBuySignals(CancellationToken cancellationToken);
}

public class BuySignalDetector : IBuySignalDetector
{
    private readonly IWatchListDataService _watchlistDataService;
    private readonly IGetCandlesOfMostRecentTradingDayDataService _getCandlesOfMostRecentTradingDayDataService;

    public BuySignalDetector(IWatchListDataService watchlistDataService,
        IGetCandlesOfMostRecentTradingDayDataService getCandlesOfMostRecentTradingDayDataService)
    {
        _watchlistDataService = watchlistDataService;
        _getCandlesOfMostRecentTradingDayDataService = getCandlesOfMostRecentTradingDayDataService;
    }

    public async Task<IReadOnlyCollection<Watchlist>> SearchForBuySignals(CancellationToken cancellationToken)
    {
        var watchlists = await _watchlistDataService.GetWatchLists(cancellationToken);

        foreach (var watchlist in watchlists)
        {
            var targetPricesPerTicker = watchlist.Entries.ToDictionary(entry => entry.Ticker, entry => entry.TargetPrice);
            var candles = await _getCandlesOfMostRecentTradingDayDataService.Get(watchlist.Entries.Select(entry => entry.Ticker).ToArray(), cancellationToken);

            var buySignals = candles.Where(candle => candle.Close > targetPricesPerTicker[candle.Ticker]).Select(candle => new BuySignal(candle.Ticker, (decimal)candle.Close!, targetPricesPerTicker[candle.Ticker])).ToArray();
            watchlist.RecordBuySignals(buySignals);
        }

        return watchlists;
    }
}