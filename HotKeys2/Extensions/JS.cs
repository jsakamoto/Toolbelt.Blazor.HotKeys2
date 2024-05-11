using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace Toolbelt.Blazor.HotKeys2.Extensions;

internal static class JS
{
    private static string GetVersionText()
    {
        var assembly = typeof(JS).Assembly;
        return assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? assembly.GetName().Version?.ToString() ?? "0.0.0";
    }

    public static async ValueTask<IJSObjectReference> ImportScriptAsync(this IJSRuntime jsRuntime, ILogger logger)
    {
        var scriptPath = "./_content/Toolbelt.Blazor.HotKeys2/script.min.js";
        try
        {
            var isOnLine = await jsRuntime.InvokeAsync<bool>("Toolbelt.Blazor.getProperty", "navigator.onLine");
            if (isOnLine) scriptPath += $"?v={GetVersionText()}";
        }
        catch (JSException e) { logger.LogError(e, e.Message); }

        return await jsRuntime.InvokeAsync<IJSObjectReference>("import", scriptPath);
    }

    public static async ValueTask InvokeSafeAsync(Func<ValueTask> action, ILogger logger)
    {
        try { await action(); }
        catch (JSDisconnectedException) { } // Ignore this exception because it is thrown when the user navigates to another page.
        catch (Exception ex) { logger.LogError(ex, ex.Message); }
    }
}
