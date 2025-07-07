using AutoFixture;
using BuySignalNotifications;
using FakeItEasy;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Tests;

public class BuySignalNotifierTests
{
    private readonly BuySignalNotifier _sut;
    private readonly ISmtpClient _emailClient;

    public BuySignalNotifierTests()
    {
        _emailClient = A.Fake<ISmtpClient>();
        var fixture = new Fixture();
        _sut = new BuySignalNotifier(_emailClient, new OptionsWrapper<BuySignalNotifierOptions>(fixture.Build<BuySignalNotifierOptions>()
            .With(options => options.SenderEmailAddress, "donotreply@buysignalnotifications")
            .Create()
        ));
    }

    [Fact]
    public async Task WHEN_ProcessWatchLists_without_buy_signals_THEN_does_not_send_any_notifications()
    {
        await _sut.ProcessWatchLists([new Watchlist("joskevermuelen@icloud.com")], TestContext.Current.CancellationToken);

        A.CallTo(() => _emailClient.SendAsync(A<MimeMessage>._, TestContext.Current.CancellationToken, null)).MustNotHaveHappened();
    }

    [Fact]
    public async Task WHEN_ProcessWatchLists_with_buy_signals_THEN_sends_an_email_to_the_owner_of_the_watchlist_with_the_buy_signals()
    {
        var watchlist = new Watchlist("joskevermeulen@icloud.com");
        watchlist.RecordBuySignals([new BuySignal("AAPL", 155.06m, 133.81m), new BuySignal("GOOG", 151.06m, 113.81m)]);
        MimeMessage sentEmail = null!;
        A.CallTo(() => _emailClient.SendAsync(A<MimeMessage>._, TestContext.Current.CancellationToken, null))
            .Invokes(fakedCall => sentEmail = fakedCall.Arguments.Get<MimeMessage>(0)!);

        await _sut.ProcessWatchLists([watchlist], TestContext.Current.CancellationToken);

        var expectedEmailBody = BuySignalEmailTemplate.Expand(DateTime.UtcNow.Date, watchlist.BuySignals);
        Assert.Equal("donotreply@buysignalnotifications", sentEmail.Sender.Address);
        Assert.Equal($"Buy signals for {DateTime.UtcNow.Date:dd-MM-yyyy}", sentEmail.Subject);
        var recipientEmailAddress = Assert.IsType<MailboxAddress>(sentEmail.To.Single());
        Assert.Equal(watchlist.EmailAddressOfOwner, recipientEmailAddress.Address);
        Assert.Equal(expectedEmailBody, sentEmail.HtmlBody);
    }
}