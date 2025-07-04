namespace BuySignalNotifier;

public record Candle(string Ticker, DateTime Date, decimal? Close, long? Volume);