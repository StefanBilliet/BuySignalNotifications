namespace BuySignalNotifications;

public record BuySignalNotifierOptions
{
    public required string SenderEmailAddress { get; set; }
    public string SmtpHost { get; set; }
    public int SmtpPort { get; set; }
    public bool EnableSsl { get; set; }
    public string SmtpUsername { get; set; }
    public string SmtpPassword { get; set; }
}