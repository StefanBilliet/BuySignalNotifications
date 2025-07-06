using Finance.Net.Interfaces;
using Finance.Net.Models.Yahoo;

namespace BuySignalNotifications;

public interface IGetCandlesOfMostRecentTradingDayDataService
{
    Task<IReadOnlyCollection<Candle>> Get(string[] tickers, CancellationToken currentCancellationToken);
}

public class GetCandlesOfMostRecentTradingDayDataService : IGetCandlesOfMostRecentTradingDayDataService
{
    private readonly IYahooFinanceService _yahooFinanceService;

    public GetCandlesOfMostRecentTradingDayDataService(IYahooFinanceService yahooFinanceService)
    {
        _yahooFinanceService = yahooFinanceService;
    }

    public async Task<IReadOnlyCollection<Candle>> Get(string[] tickers, CancellationToken currentCancellationToken)
    {
        var lastDayRecords = await GetLastDayRecords(tickers, currentCancellationToken);

        return lastDayRecords.Select(tuple => new Candle(tuple.Ticker, tuple.Record.Date, tuple.Record.Close, tuple.Record.Volume)).ToArray();
    }

    private async Task<IReadOnlyCollection<(string Ticker, Record Record)>> GetLastDayRecords(string[] tickers, CancellationToken currentCancellationToken)
    {
        var lastDayRecords = new List<(string Ticker, Record Record)>();
        foreach (var ticker in tickers)
        {
            var candles = (await _yahooFinanceService.GetRecordsAsync(ticker, DateTime.UtcNow.Date.AddDays(-5), null, currentCancellationToken)).OrderByDescending(record => record.Date);
            if (!candles.Any())
            {
                continue;
            }
            var last = candles.Last();
            lastDayRecords.Add((ticker, last));
        }

        return lastDayRecords;
    }
}