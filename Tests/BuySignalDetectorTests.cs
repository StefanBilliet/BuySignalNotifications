using BuySignalNotifications;
using FakeItEasy;

namespace Tests;

public class BuySignalDetectorTests
{
    private readonly IWatchListDataService _watchlistDataService;
    private readonly IGetCandlesOfMostRecentTradingDayDataService _getCandlesOfMostRecentTradingDayDataService;
    private readonly BuySignalDetector _sut;

    public BuySignalDetectorTests()
    {
        _watchlistDataService = A.Fake<IWatchListDataService>();
        _getCandlesOfMostRecentTradingDayDataService = A.Fake<IGetCandlesOfMostRecentTradingDayDataService>();
        _sut = new BuySignalDetector(_watchlistDataService, _getCandlesOfMostRecentTradingDayDataService);
    }

    [Fact]
    public async Task GIVEN_watchlist_WHEN_SearchForBuySignals_THEN_returns_all_tickers_where_the_price_target_exceeds_the_close_of_the_most_recent_trading_day()
    {
        var watchlist = new Watchlist("joskevermeulen@icloud.com",
            new WatchlistEntry("DDOG", 133.81m),
            new WatchlistEntry("TTWO", 245.47m)
        );
        A.CallTo(() => _watchlistDataService.GetWatchLists(A<CancellationToken>._)).Returns([watchlist]);
        A.CallTo(() => _getCandlesOfMostRecentTradingDayDataService.Get(new[] { "DDOG", "TTWO" }, A<CancellationToken>._)).Returns([
            new Candle("DDOG", DateTime.UtcNow.Date.AddDays(-1), 155.06m),
            new Candle("TTWO", DateTime.UtcNow.Date.AddDays(-1), 240.21m)
        ]);
        
        var watchlists = await _sut.SearchForBuySignals(TestContext.Current.CancellationToken);

        var watchlistWithBuySignals = Assert.Single(watchlists, entry => entry.HasBuySignals());
        var buySignal = Assert.Single(watchlistWithBuySignals.BuySignals);
        Assert.Equivalent(new BuySignal("DDOG", 155.06m, 133.81m), buySignal);
    }
}