using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Toolbelt.Blazor.HotKeys2;

namespace Toolbelt.Blazor.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for adding HotKeys service.
/// </summary>
public static class HotKeysExtensions
{
    /// <summary>
    ///  Adds a HotKeys service to the specified Microsoft.Extensions.DependencyInjection.IServiceCollection.
    /// </summary>
    /// <param name="services">The Microsoft.Extensions.DependencyInjection.IServiceCollection to add the service to.</param>
    public static IServiceCollection AddHotKeys2(this IServiceCollection services)
    {
        return services.AddScoped(serviceProvider =>
        {
            var jsRuntime = serviceProvider.GetRequiredService<IJSRuntime>();
            var logger = serviceProvider.GetRequiredService<ILogger<HotKeys>>();
            return new HotKeys(jsRuntime, logger);
        });
    }
}
