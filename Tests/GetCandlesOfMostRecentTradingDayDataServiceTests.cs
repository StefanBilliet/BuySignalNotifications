using AutoFixture.Xunit3;
using BuySignalNotifier;
using FakeItEasy;
using Finance.Net.Interfaces;

namespace Tests;

public class GetCandlesOfMostRecentTradingDayDataServiceTests
{
    private readonly GetCandlesOfMostRecentTradingDayDataService _sut;
    private readonly IYahooFinanceService _yahooFinanceService;

    public GetCandlesOfMostRecentTradingDayDataServiceTests()
    {
        _yahooFinanceService = A.Fake<IYahooFinanceService>();
        _sut = new GetCandlesOfMostRecentTradingDayDataService(_yahooFinanceService);
    }

    [Theory, AutoData]
    public async Task GIVEN_data_for_several_tickers_WHEN_GetCandlesOfMostRecentTradingDay_THEN_only_returns_data_for_the_requested_tickers(Finance.Net.Models.Yahoo.Record appleRecord, Finance.Net.Models.Yahoo.Record palantirRecord)
    {
        var startDate = DateTime.UtcNow.Date.AddDays(-1);
        A.CallTo(() => _yahooFinanceService.GetRecordsAsync("AAPL", startDate, null, TestContext.Current.CancellationToken)).Returns(new List<Finance.Net.Models.Yahoo.Record>
        {
            appleRecord
        });
        A.CallTo(() => _yahooFinanceService.GetRecordsAsync("PLTR", startDate, null, TestContext.Current.CancellationToken)).Returns(new List<Finance.Net.Models.Yahoo.Record>
        {
            palantirRecord
        });

        var closingCandles = await _sut.GetCandlesOfMostRecentTradingDay(["AAPL"], TestContext.Current.CancellationToken);

        var closingCandle = Assert.Single(closingCandles);
        Assert.Equivalent(new Candle(appleRecord.Date, appleRecord.Close, appleRecord.Volume), closingCandle);
    }
}