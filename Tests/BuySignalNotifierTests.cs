using Azure;
using Azure.Communication.Email;
using BuySignalNotifications;
using FakeItEasy;
using Microsoft.Extensions.Options;

namespace Tests;

public class BuySignalNotifierTests
{
    private readonly BuySignalNotifier _sut;
    private readonly EmailClient _emailClient;

    public BuySignalNotifierTests()
    {
        _emailClient = A.Fake<EmailClient>();
        _sut = new BuySignalNotifier(_emailClient, new OptionsWrapper<BuySignalNotifierOptions>(new BuySignalNotifierOptions
        {
            SenderEmailAddress = "donotreply@buysignalnotifications"
        }));
    }

    [Fact]
    public async Task WHEN_ProcessWatchLists_without_buy_signals_THEN_does_not_send_any_notifications()
    {
        await _sut.ProcessWatchLists([new Watchlist("joskevermuelen@icloud.com")], TestContext.Current.CancellationToken);

        A.CallTo(() => _emailClient.SendAsync(WaitUntil.Started, A<EmailMessage>._, TestContext.Current.CancellationToken)).MustNotHaveHappened();
    }

    [Fact]
    public async Task WHEN_ProcessWatchLists_with_buy_signals_THEN_sends_an_email_to_the_owner_of_the_watchlist_with_the_buy_signals()
    {
        var watchlist = new Watchlist("joskevermeulen@icloud.com");
        watchlist.RecordBuySignals([new BuySignal("AAPL", 155.06m, 133.81m), new BuySignal("GOOG", 151.06m, 113.81m)]);
        EmailMessage sentEmail = null!;
        A.CallTo(() => _emailClient.SendAsync(WaitUntil.Started, A<EmailMessage>._, TestContext.Current.CancellationToken)).Invokes(fakedCall => sentEmail = fakedCall.Arguments.Get<EmailMessage>(1)!);

        await _sut.ProcessWatchLists([watchlist], TestContext.Current.CancellationToken);

        var expectedEmailBody = BuySignalEmailTemplate.Expand(DateTime.UtcNow.Date, watchlist.BuySignals);
        Assert.Equal("donotreply@buysignalnotifications", sentEmail.SenderAddress);
        Assert.Equal($"Buy signals for {DateTime.UtcNow.Date:dd-MM-yyyy}", sentEmail.Content.Subject);
        Assert.Equal(watchlist.EmailAddressOfOwner, sentEmail.Recipients.To.Single().Address);
        Assert.Equal(expectedEmailBody, sentEmail.Content.Html);
    }
}