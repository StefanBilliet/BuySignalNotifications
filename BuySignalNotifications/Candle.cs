namespace BuySignalNotifications;

public record Candle(string Ticker, DateTime Date, decimal? Close, long? Volume);