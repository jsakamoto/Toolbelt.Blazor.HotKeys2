using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace Toolbelt.Blazor.HotKeys2;

/// <summary>
/// A service that provides Hotkey feature.
/// </summary>
public class HotKeys : IAsyncDisposable
{
    private volatile bool _Attached = false;

    private readonly ILogger<HotKeys> _Logger;

    private readonly IJSRuntime _JSRuntime;

    private IJSObjectReference? _JSModule = null;

    private readonly SemaphoreSlim _Syncer = new(1, 1);

    private readonly bool _IsWasm = RuntimeInformation.OSDescription == "web" || RuntimeInformation.OSDescription == "Browser";

    /// <summary>
    /// Occurs when the user enter any keys on the browser.
    /// </summary>
    public event EventHandler<HotKeyDownEventArgs>? KeyDown;

    /// <summary>
    /// Initialize a new instance of the HotKeys class.
    /// </summary>
    [DynamicDependency(nameof(OnKeyDown), typeof(HotKeys))]
    internal HotKeys(IJSRuntime jSRuntime, ILogger<HotKeys> logger)
    {
        this._JSRuntime = jSRuntime;
        this._Logger = logger;
    }

    /// <summary>
    /// Attach this HotKeys service instance to JavaScript DOM event handler.
    /// </summary>
    private async Task<IJSObjectReference> AttachAsync()
    {
        var module = await this.GetJsModuleAsync();
        if (this._Attached) return module;
        await this._Syncer.WaitAsync();
        try
        {
            if (this._Attached) return module;

            await module.InvokeAsync<object>("Toolbelt.Blazor.HotKeys2.attach", DotNetObjectReference.Create(this), this._IsWasm);

            this._Attached = true;
            return module;
        }
        finally { this._Syncer.Release(); }
    }

    private string GetVersionText()
    {
        var assembly = this.GetType().Assembly;
        return assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? assembly.GetName().Version?.ToString() ?? "0.0.0";
    }

    private async ValueTask<IJSObjectReference> GetJsModuleAsync()
    {
        if (this._JSModule == null)
        {
            var scriptPath = "./_content/Toolbelt.Blazor.HotKeys2/script.min.js";
            try
            {
                var isOnLine = await this._JSRuntime.InvokeAsync<bool>("Toolbelt.Blazor.getProperty", "navigator.onLine");
                if (isOnLine) scriptPath += $"?v={this.GetVersionText()}";
            }
            catch (JSException e) { this._Logger.LogError(e, e.Message); }

            this._JSModule = await this._JSRuntime.InvokeAsync<IJSObjectReference>("import", scriptPath);
        }
        return this._JSModule;
    }

    /// <summary>
    /// Create hotkey entries context, and activate it.
    /// </summary>
    /// <returns></returns>
    public HotKeysContext CreateContext()
    {
        var attachTask = this.AttachAsync();
        return new HotKeysContext(attachTask, this._Logger);
    }

    /// <summary>
    /// The method that will be invoked from JavaScript keydown event handler.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="srcElementTagName">The tag name of HTML element that is source of the DOM event.</param>
    /// <param name="srcElementTypeName">The <code>type</code>attribute, if any, of the HTML element that is source of the DOM event</param>
    /// <param name="key">The value of the "key" property in the DOM event object</param>
    /// <param name="code">The value of the "code" property in the DOM event object</param>
    /// <returns></returns>
    [JSInvokable(nameof(OnKeyDown)), EditorBrowsable(EditorBrowsableState.Never)]
    public bool OnKeyDown(ModCode modifiers, string srcElementTagName, string srcElementTypeName, string key, string code)
    {
        var args = new HotKeyDownEventArgs(modifiers, srcElementTagName, srcElementTypeName, this._IsWasm, key, code);
        KeyDown?.Invoke(null, args);
        return args.PreventDefault;
    }

    public async ValueTask DisposeAsync()
    {
        try { if (this._JSModule != null) await this._JSModule.DisposeAsync(); }
        catch (Exception ex) when (ex.GetType().FullName == "Microsoft.JSInterop.JSDisconnectedException") { }
        catch (Exception ex) { this._Logger.LogError(ex, ex.Message); }
    }
}
