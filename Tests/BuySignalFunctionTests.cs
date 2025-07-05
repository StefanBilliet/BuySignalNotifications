using System.Text.Json;
using AutoFixture.Xunit3;
using Azure;
using Azure.Communication.Email;
using Azure.Storage.Blobs;
using BuySignalNotifications;
using FakeItEasy;
using Finance.Net.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Record = Finance.Net.Models.Yahoo.Record;

namespace Tests;

[Collection(nameof(FunctionAppCollection))]
public class BuySignalFunctionTests
{
    private readonly BuySignalFunction _sut;
    private readonly BlobContainerClient _watchlistsContainerClient;
    private readonly IYahooFinanceService _yahooFinanceService;
    private readonly EmailClient _emailClient;

    public BuySignalFunctionTests(FunctionAppFixture fixture)
    {
        _yahooFinanceService = fixture.ServiceProvider.GetRequiredService<IYahooFinanceService>();
        _watchlistsContainerClient = fixture.ServiceProvider.GetRequiredService<BlobContainerClient>();
        _emailClient = fixture.ServiceProvider.GetRequiredService<EmailClient>();
        _sut = fixture.ServiceProvider.GetRequiredService<BuySignalFunction>();
    }

    [Theory, AutoData]
    public async Task GIVEN_watchlist_WHEN_ProcessBuySignals_THEN_send_email_with_buy_signals(Record appleRecord, Record palantirRecord)
    {
        var seededWatchList = await GivenSeededWatchlist(appleRecord, palantirRecord);
        EmailMessage sentEmail = null!;
        A.CallTo(() => _emailClient.SendAsync(WaitUntil.Started, A<EmailMessage>._, TestContext.Current.CancellationToken))
            .Invokes(fakedCall => sentEmail = fakedCall.Arguments.Get<EmailMessage>(1)!);
        var mockContext = GivenFunctionContext();

        await _sut.ProcessBuySignals(new TimerInfo(), mockContext);

        Assert.Equal("donotreply@buysignalnotifications.com", sentEmail.SenderAddress);
        Assert.Equal($"Buy signals for {DateTime.UtcNow.Date:dd-MM-yyyy}", sentEmail.Content.Subject);
        Assert.Equal(seededWatchList.EmailAddressOfOwner, sentEmail.Recipients.To.Single().Address);
        Assert.Contains("AAPL", sentEmail.Content.Html);
    }

    private static FunctionContext GivenFunctionContext()
    {
        var mockContext = A.Fake<FunctionContext>();
        A.CallTo(() => mockContext.CancellationToken).Returns(TestContext.Current.CancellationToken);
        return mockContext;
    }

    private async Task<Watchlist> GivenSeededWatchlist(Record appleRecord, Record palantirRecord)
    {
        var startDate = DateTime.UtcNow.Date.AddDays(-1);
        A.CallTo(() => _yahooFinanceService.GetRecordsAsync("AAPL", startDate, null, TestContext.Current.CancellationToken)).Returns(
            new List<Record>
            {
                appleRecord with { Close = 290m }
            });
        A.CallTo(() => _yahooFinanceService.GetRecordsAsync("PLTR", startDate, null, TestContext.Current.CancellationToken)).Returns(
            new List<Record>
            {
                palantirRecord with { Close = 142.15m }
            });
        var seededWatchList = new Watchlist("joskevermeulen@huppeldepup.com",
            new WatchlistEntry("AAPL", 289m),
            new WatchlistEntry("PLTR", 150.51m)
        );
        var blobClient = _watchlistsContainerClient.GetBlobClient("buylist_may.json");
        await blobClient.UploadAsync(BinaryData.FromString(JsonSerializer.Serialize(seededWatchList)), overwrite: true,
            cancellationToken: TestContext.Current.CancellationToken);
        return seededWatchList;
    }
}