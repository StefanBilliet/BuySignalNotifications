namespace BuySignalNotifier;

public record Watchlist(string EmailAddressOfOwner, params WatchlistEntry[] Entries);