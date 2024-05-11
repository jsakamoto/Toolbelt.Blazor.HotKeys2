namespace Toolbelt.Blazor.HotKeys2;

internal static class Extensions
{
    public static async ValueTask<T> InvokeAsync<T>(this SemaphoreSlim semaphore, Func<ValueTask<T>> asyncAction)
    {
        await semaphore.WaitAsync();
        try { return await asyncAction.Invoke(); }
        finally { semaphore.Release(); }
    }
}
