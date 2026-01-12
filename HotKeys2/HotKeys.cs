using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Toolbelt.Blazor.HotKeys2.Extensions;

namespace Toolbelt.Blazor.HotKeys2;

/// <summary>
/// A service that provides Hotkey feature.
/// </summary>
public class HotKeys : IAsyncDisposable
{
    private readonly ILogger<HotKeys> _Logger;

    private readonly IJSRuntime _JSRuntime;

    internal readonly DotNetObjectReference<HotKeys> _ObjectRef;

    private readonly SemaphoreSlim _Syncer = new(1, 1);

    private EventHandler<HotKeyDownEventArgs>? _KeyDown;

    /// <summary>
    /// Occurs when the user enter any keys on the browser.
    /// </summary>
    public event EventHandler<HotKeyDownEventArgs>? KeyDown
    {
        add
        {
            this._KeyDown += value;
            var _ = this.EnsureAttachedAsync();
        }
        remove
        {
            this._KeyDown -= value;
            if (this._KeyDown == null) { var _ = this.DetachAsync(); }
        }
    }

    private IJSObjectReference? _KeyEventHandler;

    /// <summary>
    /// Initialize a new instance of the HotKeys class.
    /// </summary>
    [DynamicDependency(nameof(OnKeyDown), typeof(HotKeys))]
    internal HotKeys(IJSRuntime jSRuntime, ILogger<HotKeys> logger)
    {
        this._ObjectRef = DotNetObjectReference.Create(this);
        this._JSRuntime = jSRuntime;
        this._Logger = logger;
    }

    private async ValueTask EnsureAttachedAsync()
    {
        await this._Syncer.InvokeAsync(async () =>
        {
            if (this._KeyEventHandler != null) return true;

            await JS.InvokeSafeAsync(async () =>
            {
                await using var module = await this._JSRuntime.ImportScriptAsync(this._Logger);
                this._KeyEventHandler = await module.InvokeAsync<IJSObjectReference>(
                    "handleKeyEvent",
                    this._ObjectRef,
                    OperatingSystem.IsBrowser());
            }, this._Logger);

            return true;
        }, this._Logger);
    }

    private async ValueTask DetachAsync()
    {
        await this._Syncer.InvokeAsync(async () =>
        {
            if (this._KeyEventHandler != null)
            {
                await JS.InvokeSafeAsync(async () =>
                {
                    await this._KeyEventHandler.InvokeVoidAsync("dispose");
                    await this._KeyEventHandler.DisposeAsync();
                }, this._Logger);
                this._KeyEventHandler = null;
            }
            return true;
        }, this._Logger);
    }

    /// <summary>
    /// Create hotkey entries context, and activate it.
    /// </summary>
    /// <returns></returns>
    public HotKeysContext CreateContext()
    {
        return new HotKeysContext(this._JSRuntime, this._Logger);
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
        var args = new HotKeyDownEventArgs(modifiers, srcElementTagName, srcElementTypeName, OperatingSystem.IsBrowser(), key, code);
        this._KeyDown?.Invoke(null, args);
        return args.PreventDefault;
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        await this.DetachAsync();
        this._ObjectRef.Dispose();
    }
}
