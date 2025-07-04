using Finance.Net.Interfaces;

namespace BuySignalNotifier;

public class GetCandlesOfMostRecentTradingDayDataService
{
    private readonly IYahooFinanceService _yahooFinanceService;

    public GetCandlesOfMostRecentTradingDayDataService(IYahooFinanceService yahooFinanceService)
    {
        _yahooFinanceService = yahooFinanceService;
    }

    public async Task<IReadOnlyCollection<Candle>> GetCandlesOfMostRecentTradingDay(string[] tickers, CancellationToken currentCancellationToken)
    {
        var lastDayRecords = await GetLastDayRecords(tickers, currentCancellationToken);

        return lastDayRecords.Select(record => new Candle(record.Date, record.Close, record.Volume)).ToArray();
    }

    private async Task<IReadOnlyCollection<Finance.Net.Models.Yahoo.Record>> GetLastDayRecords(string[] tickers, CancellationToken currentCancellationToken)
    {
        var lastDayRecords = new List<Finance.Net.Models.Yahoo.Record>();
        foreach (var ticker in tickers)
        {
            lastDayRecords.AddRange(await _yahooFinanceService.GetRecordsAsync(ticker, DateTime.UtcNow.Date.AddDays(-1), null, currentCancellationToken));
        }

        return lastDayRecords;
    }
}