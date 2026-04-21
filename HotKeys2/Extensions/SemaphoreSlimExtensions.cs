using Microsoft.Extensions.Logging;

namespace Toolbelt.Blazor.HotKeys2.Extensions;

internal static class SemaphoreSlimExtensions
{
    public static async ValueTask<T> InvokeAsync<T>(this SemaphoreSlim semaphore, Func<ValueTask<T>> asyncAction, ILogger? logger = null)
    {
        await semaphore.WaitAsync();
        try { return await asyncAction.Invoke(); }
        // NOTE: The following catches assume cancellations only originate from Blazor circuit disconnect.
        // If a public API that accepts a user-supplied CancellationToken is ever added,
        // these catches will also silently swallow user-driven cancellations - redesign required.
        catch (OperationCanceledException) { return default!; }
        catch (AggregateException ex) when (ex.Flatten().InnerExceptions.All(e => e is OperationCanceledException)) { return default!; }
        catch (Exception ex)
        {
            if (logger != null)
            {
                logger.LogError(ex, ex.Message);
                return default!;
            }
            throw;
        }
        finally { semaphore.Release(); }
    }
}
