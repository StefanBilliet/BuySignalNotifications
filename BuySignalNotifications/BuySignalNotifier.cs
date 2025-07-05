using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Options;

namespace BuySignalNotifications;

public interface IBuySignalNotifier
{
    Task ProcessWatchLists(IReadOnlyCollection<Watchlist> watchlists, CancellationToken cancellationToken);
}

public class BuySignalNotifier : IBuySignalNotifier
{
    private readonly EmailClient _emailClient;
    private readonly IOptions<BuySignalNotifierOptions> _options;

    public BuySignalNotifier(EmailClient emailClient, IOptions<BuySignalNotifierOptions> options)
    {
        _emailClient = emailClient;
        _options = options;
    }

    public async Task ProcessWatchLists(IReadOnlyCollection<Watchlist> watchlists, CancellationToken cancellationToken)
    {
        foreach (var watchlist in watchlists.Where(watchlist => watchlist.HasBuySignals()))
        {
            var emailMessage = new EmailMessage(_options.Value.SenderEmailAddress, watchlist.EmailAddressOfOwner,
                new EmailContent($"Buy signals for {DateTime.UtcNow.Date:dd-MM-yyyy}"))
            {
                Content = { Html = BuySignalEmailTemplate.Expand(DateTime.UtcNow.Date, watchlist.BuySignals) }
            };
            await _emailClient.SendAsync(WaitUntil.Started, emailMessage, cancellationToken);
        }
    }
}