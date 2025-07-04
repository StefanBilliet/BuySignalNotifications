namespace BuySignalNotifications;

public static class BuySignalEmailTemplate
{
    public static string Expand(DateTime date, IReadOnlyCollection<BuySignal> buySignals)
    {
        var formattedDate = DateTime.UtcNow.ToString("dd-MM-yyyy");
        var rows = string.Join("", buySignals.Select(buySignal =>
            $"<tr><td>{buySignal.Ticker}</td><td>{buySignal.ClosingPrice:F2}</td><td>{buySignal.TargetPrice:F2}</td></tr>")
        );

        return $"""

                <!DOCTYPE html>
                <html>
                  <body style="font-family:Segoe UI,Arial,sans-serif;line-height:1.4;">
                    <p>Buy signals for <strong>{formattedDate}</strong>:</p>
                    <table cellpadding="4" cellspacing="0" border="1" style="border-collapse:collapse;">
                      <thead>
                        <tr><th>Ticker</th><th>Last Close</th><th>Target</th></tr>
                      </thead>
                      <tbody>
                        {rows}
                      </tbody>
                    </table>
                    <p>Happy trading!<br/>— Your Watchlist Bot</p>
                  </body>
                </html>
                """;
    }
}