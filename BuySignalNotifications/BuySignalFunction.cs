using Microsoft.Azure.Functions.Worker;

namespace BuySignalNotifications;

public class BuySignalFunction
{
    private readonly IBuySignalDetector _buySignalDetector;
    private readonly IBuySignalNotifier _buySignalNotifier;

    public BuySignalFunction(IBuySignalDetector buySignalDetector, IBuySignalNotifier buySignalNotifier)
    {
        _buySignalDetector = buySignalDetector;
        _buySignalNotifier = buySignalNotifier;
    }

    [Function("ProcessBuySignals")]
    public async Task ProcessBuySignals(
        [TimerTrigger("0 5 22 * * 1-5")] TimerInfo timerInfo,
        FunctionContext executionContext)
    {
        var processedWatchlists = await _buySignalDetector.SearchForBuySignals(executionContext.CancellationToken);
        await _buySignalNotifier.ProcessWatchLists(processedWatchlists, executionContext.CancellationToken);
    }
}