namespace BuySignalNotifications;

public record BuySignalNotifierOptions
{
    public required string SenderEmailAddress { get; set; }
    public required string SmtpHost { get; set; }
    public required int SmtpPort { get; set; }
    public required bool EnableSsl { get; set; }
    public required string SmtpUsername { get; set; }
    public required string SmtpPassword { get; set; }
}