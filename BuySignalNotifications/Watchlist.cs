namespace BuySignalNotifications;

public record BuySignal(string Ticker, decimal ClosingPrice, decimal TargetPrice);

public record Watchlist(string EmailAddressOfOwner, params WatchlistEntry[] Entries)
{
    public BuySignal[] BuySignals { get; private set; } = [];
    
    public void RecordBuySignals(BuySignal[] buySignals)
    {
        BuySignals = buySignals;
    }

    public bool HasBuySignals()
    {
        return BuySignals.Length > 0;
    }
}