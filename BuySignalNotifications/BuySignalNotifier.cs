using System.Text;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace BuySignalNotifications;

public interface IBuySignalNotifier
{
    Task ProcessWatchLists(IReadOnlyCollection<Watchlist> watchlists, CancellationToken cancellationToken);
}

public class BuySignalNotifier : IBuySignalNotifier
{
    private readonly ISmtpClient _smtpClient;
    private readonly IOptions<BuySignalNotifierOptions> _options;

    public BuySignalNotifier(ISmtpClient smtpClient, IOptions<BuySignalNotifierOptions> options)
    {
        _smtpClient = smtpClient;
        _options = options;
    }

    public async Task ProcessWatchLists(IReadOnlyCollection<Watchlist> watchlists, CancellationToken cancellationToken)
    {
        foreach (var watchlist in watchlists.Where(watchlist => watchlist.HasBuySignals()))
        {
            var message = new MimeMessage();
            var sender = new MailboxAddress("", _options.Value.SenderEmailAddress);
            message.Sender = sender;
            message.From.Add(sender);
            message.To.Add(new MailboxAddress("", watchlist.EmailAddressOfOwner));
            message.Subject = $"Buy signals for {DateTime.UtcNow.Date:dd-MM-yyyy}";
            
            message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = BuySignalEmailTemplate.Expand(DateTime.UtcNow.Date, watchlist.BuySignals)
            };

            if (!_smtpClient.IsConnected)
            {
                await _smtpClient.ConnectAsync(_options.Value.SmtpHost, _options.Value.SmtpPort, SecureSocketOptions.StartTls, cancellationToken);
                await _smtpClient.AuthenticateAsync(Encoding.UTF8, _options.Value.SmtpUsername,
                    _options.Value.SmtpPassword, cancellationToken);
            }

            await _smtpClient.SendAsync(message, cancellationToken);
        }
    }
}