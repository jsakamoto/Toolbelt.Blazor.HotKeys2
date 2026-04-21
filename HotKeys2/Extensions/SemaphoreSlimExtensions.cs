using Microsoft.Extensions.Logging;

namespace Toolbelt.Blazor.HotKeys2.Extensions;

internal static class SemaphoreSlimExtensions
{
    public static async ValueTask<T> InvokeAsync<T>(this SemaphoreSlim semaphore, Func<ValueTask<T>> asyncAction, ILogger? logger = null)
    {
        await semaphore.WaitAsync();
        try { return await asyncAction.Invoke(); }
        catch (OperationCanceledException) { return default!; }
        catch (AggregateException ex) when (ex.InnerExceptions.All(e => e is OperationCanceledException or AggregateException { InnerException: OperationCanceledException })) { return default!; }
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
