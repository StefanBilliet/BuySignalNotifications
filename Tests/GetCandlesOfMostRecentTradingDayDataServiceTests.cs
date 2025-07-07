using AutoFixture.Xunit3;
using BuySignalNotifications;
using FakeItEasy;
using Finance.Net.Interfaces;
using Record = Finance.Net.Models.Yahoo.Record;

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
    public async Task GIVEN_data_for_several_tickers_WHEN_Get_THEN_only_returns_data_for_the_requested_tickers(Record appleRecord,
        Record palantirRecord)
    {
        var startDate = DateTime.UtcNow.Date.AddDays(-5);
        A.CallTo(() => _yahooFinanceService.GetRecordsAsync("AAPL", startDate, null, TestContext.Current.CancellationToken)).Returns(
            new List<Record>
            {
                appleRecord
            });
        A.CallTo(() => _yahooFinanceService.GetRecordsAsync("PLTR", startDate, null, TestContext.Current.CancellationToken)).Returns(
            new List<Record>
            {
                palantirRecord
            });

        var closingCandles = await _sut.Get(["AAPL"], TestContext.Current.CancellationToken);

        var closingCandle = Assert.Single(closingCandles);
        Assert.Equivalent(new Candle("AAPL", appleRecord.Date, appleRecord.Close, appleRecord.Volume), closingCandle);
    }

    [Fact]
    public async Task WHEN_Get_THEN_only_returns_data_for_the_last_trading_day()
    {
        var startDate = DateTime.UtcNow.Date.AddDays(-5);
        var oldestRecord = new Record() { Date = DateTime.UtcNow.AddDays(-2), Open = 160, Close = 160 };
        var recordOfMostRecentTradingDay = new Record { Date = DateTime.UtcNow.AddDays(-1), Open = 157, Close = 158 };
        A.CallTo(() => _yahooFinanceService.GetRecordsAsync("AAPL", startDate, null, TestContext.Current.CancellationToken)).Returns(
            new List<Record>
            {
                recordOfMostRecentTradingDay,
                oldestRecord
            });

        var closingCandles = await _sut.Get(["AAPL"], TestContext.Current.CancellationToken);

        var closingCandle = Assert.Single(closingCandles);
        Assert.Equivalent(new Candle("AAPL", recordOfMostRecentTradingDay.Date, recordOfMostRecentTradingDay.Close, recordOfMostRecentTradingDay.Volume), closingCandle);
    }
}