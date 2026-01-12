using System.ComponentModel;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Toolbelt.Blazor.HotKeys2.Extensions;

namespace Toolbelt.Blazor.HotKeys2;

/// <summary>
/// Current active hotkeys set.
/// </summary>
public partial class HotKeysContext : IDisposable, IAsyncDisposable
{
    private readonly List<HotKeyEntry> _Keys = [];

    /// <summary>
    /// The collection of Hotkey entries.
    /// </summary>
    public IEnumerable<HotKeyEntry> HotKeyEntries => this._Keys;

    [Obsolete("Use the HotKeyEntries instead."), EditorBrowsable(EditorBrowsableState.Never)]
    public List<HotKeyEntry> Keys => this._Keys;

    private readonly IJSRuntime _JSRuntime;

    private readonly ILogger _Logger;

    private readonly SemaphoreSlim _Syncer = new(1, 1);

    private readonly Task<IJSObjectReference> _JSContextTask;

    private bool _Disposed = false;

    /// <summary>
    /// Initialize a new instance of the HotKeysContext class.
    /// </summary>
    internal HotKeysContext(IJSRuntime jsRuntime, ILogger logger)
    {
        this._JSRuntime = jsRuntime;
        this._Logger = logger;
        this._JSContextTask = this.CreateJSContextAsync();
    }

    private async Task<IJSObjectReference> CreateJSContextAsync()
    {
        return await this._Syncer.InvokeAsync(async () =>
        {
            await using var module = await this._JSRuntime.ImportScriptAsync(this._Logger);
            return await module.InvokeAsync<IJSObjectReference>("createContext");
        });
    }

    // ===============================================================================================

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="key">The identifier of hotkey by key.</param>
    /// <param name="action">The callback action that will be invoked when user enter the key without any modifiers on the browser.</param>
    /// <param name="options">The options for this hotkey entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(Key key, Action action, HotKeyOptions options)
    => this.AddInternal(ModKey.None, key, _ => { action(); return ValueTask.CompletedTask; }, action.Target as IHandleEvent, options);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="key">The identifier of hotkey by key.</param>
    /// <param name="action">The callback action that will be invoked when user enter the key without any modifiers on the browser.</param>
    /// <param name="options">The options for this hotkey entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(Key key, Action<HotKeyEntryByKey> action, HotKeyOptions options)
        => this.AddInternal(ModKey.None, key, arg => { action(arg); return ValueTask.CompletedTask; }, action.Target as IHandleEvent, options);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="key">The identifier of hotkey by key.</param>
    /// <param name="action">The callback action that will be invoked when user enter the key without any modifiers on the browser.</param>
    /// <param name="options">The options for this hotkey entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(Key key, Func<ValueTask> action, HotKeyOptions options)
        => this.AddInternal(ModKey.None, key, _ => action(), action.Target as IHandleEvent, options);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="key">The identifier of hotkey by key.</param>
    /// <param name="action">The callback action that will be invoked when user enter the key without any modifiers on the browser.</param>
    /// <param name="options">The options for this hotkey entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(Key key, Func<Task> action, HotKeyOptions options)
        => this.AddInternal(ModKey.None, key, async _ => await action(), action.Target as IHandleEvent, options);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="key">The identifier of hotkey by key.</param>
    /// <param name="action">The callback action that will be invoked when user enter the key without any modifiers on the browser.</param>
    /// <param name="options">The options for this hotkey entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(Key key, Func<HotKeyEntryByKey, ValueTask> action, HotKeyOptions options)
        => this.AddInternal(ModKey.None, key, arg => action(arg), action.Target as IHandleEvent, options);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="key">The identifier of hotkey by key.</param>
    /// <param name="action">The callback action that will be invoked when user enter the key without any modifiers on the browser.</param>
    /// <param name="options">The options for this hotkey entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(Key key, Func<HotKeyEntryByKey, Task> action, HotKeyOptions options)
        => this.AddInternal(ModKey.None, key, async arg => await action(arg), action.Target as IHandleEvent, options);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="key">The identifier of hotkey by key.</param>
    /// <param name="action">The callback action that will be invoked when user enter the key without any modifiers on the browser.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(Key key, Action action, string description = "", Exclude exclude = Exclude.Default)
    => this.AddInternal(ModKey.None, key, _ => { action(); return ValueTask.CompletedTask; }, action.Target as IHandleEvent, new() { Description = description, Exclude = exclude });

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="key">The identifier of hotkey by key.</param>
    /// <param name="action">The callback action that will be invoked when user enter the key without any modifiers on the browser.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(Key key, Action<HotKeyEntryByKey> action, string description = "", Exclude exclude = Exclude.Default)
        => this.AddInternal(ModKey.None, key, arg => { action(arg); return ValueTask.CompletedTask; }, action.Target as IHandleEvent, new() { Description = description, Exclude = exclude });

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="key">The identifier of hotkey by key.</param>
    /// <param name="action">The callback action that will be invoked when user enter the key without any modifiers on the browser.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(Key key, Func<ValueTask> action, string description = "", Exclude exclude = Exclude.Default)
        => this.AddInternal(ModKey.None, key, _ => action(), action.Target as IHandleEvent, new() { Description = description, Exclude = exclude });

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="key">The identifier of hotkey by key.</param>
    /// <param name="action">The callback action that will be invoked when user enter the key without any modifiers on the browser.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(Key key, Func<Task> action, string description = "", Exclude exclude = Exclude.Default)
        => this.AddInternal(ModKey.None, key, async _ => await action(), action.Target as IHandleEvent, new() { Description = description, Exclude = exclude });

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="key">The identifier of hotkey by key.</param>
    /// <param name="action">The callback action that will be invoked when user enter the key without any modifiers on the browser.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(Key key, Func<HotKeyEntryByKey, ValueTask> action, string description = "", Exclude exclude = Exclude.Default)
        => this.AddInternal(ModKey.None, key, arg => action(arg), action.Target as IHandleEvent, new() { Description = description, Exclude = exclude });

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="key">The identifier of hotkey by key.</param>
    /// <param name="action">The callback action that will be invoked when user enter the key without any modifiers on the browser.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(Key key, Func<HotKeyEntryByKey, Task> action, string description = "", Exclude exclude = Exclude.Default)
        => this.AddInternal(ModKey.None, key, async arg => await action(arg), action.Target as IHandleEvent, new() { Description = description, Exclude = exclude });

    // -----------------------------------------------------------------------------------------------

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="key">The identifier of hotkey by key.</param>
    /// <param name="action">The callback action that will be invoked when user enter modifiers + key combination on the browser.</param>
    /// <param name="options">The options for this hotkey entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(ModKey modifiers, Key key, Action action, HotKeyOptions options)
        => this.AddInternal(modifiers, key, _ => { action(); return ValueTask.CompletedTask; }, action.Target as IHandleEvent, options);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="key">The identifier of hotkey by key.</param>
    /// <param name="action">The callback action that will be invoked when user enter modifiers + key combination on the browser.</param>
    /// <param name="options">The options for this hotkey entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(ModKey modifiers, Key key, Action<HotKeyEntryByKey> action, HotKeyOptions options)
        => this.AddInternal(modifiers, key, arg => { action(arg); return ValueTask.CompletedTask; }, action.Target as IHandleEvent, options);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="key">The identifier of hotkey by key.</param>
    /// <param name="action">The callback action that will be invoked when user enter modifiers + key combination on the browser.</param>
    /// <param name="options">The options for this hotkey entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(ModKey modifiers, Key key, Func<ValueTask> action, HotKeyOptions options)
        => this.AddInternal(modifiers, key, _ => action(), action.Target as IHandleEvent, options);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="key">The identifier of hotkey by key.</param>
    /// <param name="action">The callback action that will be invoked when user enter modifiers + key combination on the browser.</param>
    /// <param name="options">The options for this hotkey entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(ModKey modifiers, Key key, Func<Task> action, HotKeyOptions options)
        => this.AddInternal(modifiers, key, async _ => await action(), action.Target as IHandleEvent, options);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="key">The identifier of hotkey by key.</param>
    /// <param name="action">The callback action that will be invoked when user enter modifiers + key combination on the browser.</param>
    /// <param name="options">The options for this hotkey entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(ModKey modifiers, Key key, Func<HotKeyEntryByKey, ValueTask> action, HotKeyOptions options)
        => this.AddInternal(modifiers, key, action, action.Target as IHandleEvent, options);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="key">The identifier of hotkey by key.</param>
    /// <param name="action">The callback action that will be invoked when user enter modifiers + key combination on the browser.</param>
    /// <param name="options">The options for this hotkey entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(ModKey modifiers, Key key, Func<HotKeyEntryByKey, Task> action, HotKeyOptions options)
        => this.AddInternal(modifiers, key, async arg => await action(arg), action.Target as IHandleEvent, options);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="key">The identifier of hotkey by key.</param>
    /// <param name="action">The callback action that will be invoked when user enter modifiers + key combination on the browser.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(ModKey modifiers, Key key, Action action, string description = "", Exclude exclude = Exclude.Default)
        => this.AddInternal(modifiers, key, _ => { action(); return ValueTask.CompletedTask; }, action.Target as IHandleEvent, new() { Description = description, Exclude = exclude });

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="key">The identifier of hotkey by key.</param>
    /// <param name="action">The callback action that will be invoked when user enter modifiers + key combination on the browser.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(ModKey modifiers, Key key, Action<HotKeyEntryByKey> action, string description = "", Exclude exclude = Exclude.Default)
        => this.AddInternal(modifiers, key, arg => { action(arg); return ValueTask.CompletedTask; }, action.Target as IHandleEvent, new() { Description = description, Exclude = exclude });

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="key">The identifier of hotkey by key.</param>
    /// <param name="action">The callback action that will be invoked when user enter modifiers + key combination on the browser.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(ModKey modifiers, Key key, Func<ValueTask> action, string description = "", Exclude exclude = Exclude.Default)
        => this.AddInternal(modifiers, key, _ => action(), action.Target as IHandleEvent, new() { Description = description, Exclude = exclude });

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="key">The identifier of hotkey by key.</param>
    /// <param name="action">The callback action that will be invoked when user enter modifiers + key combination on the browser.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(ModKey modifiers, Key key, Func<Task> action, string description = "", Exclude exclude = Exclude.Default)
        => this.AddInternal(modifiers, key, async _ => await action(), action.Target as IHandleEvent, new() { Description = description, Exclude = exclude });

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="key">The identifier of hotkey by key.</param>
    /// <param name="action">The callback action that will be invoked when user enter modifiers + key combination on the browser.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(ModKey modifiers, Key key, Func<HotKeyEntryByKey, ValueTask> action, string description = "", Exclude exclude = Exclude.Default)
        => this.AddInternal(modifiers, key, action, action.Target as IHandleEvent, new() { Description = description, Exclude = exclude });

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="key">The identifier of hotkey by key.</param>
    /// <param name="action">The callback action that will be invoked when user enter modifiers + key combination on the browser.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(ModKey modifiers, Key key, Func<HotKeyEntryByKey, Task> action, string description = "", Exclude exclude = Exclude.Default)
        => this.AddInternal(modifiers, key, async arg => await action(arg), action.Target as IHandleEvent, new() { Description = description, Exclude = exclude });

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="key">The identifier of hotkey by key.</param>
    /// <param name="action">The callback action that will be invoked when user enter modifiers + key combination on the browser.</param>
    /// <param name="ownerOfAction">The owner of the action. If the owner is disposed, the hotkey will be removed automatically.</param>
    /// <param name="options">The options for this hotkey entry.</param>
    /// <returns>This context.</returns>
    private HotKeysContext AddInternal(ModKey modifiers, Key key, Func<HotKeyEntryByKey, ValueTask> action, IHandleEvent? ownerOfAction, HotKeyOptions options)
        => this.AddInternal(new HotKeyEntryByKey(this._Logger, modifiers, key, action, ownerOfAction, options));

    // ===============================================================================================

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="code">The identifier of hotkey by code.</param>
    /// <param name="action">The callback action that will be invoked when user enter the key without any modifiers on the browser.</param>
    /// <param name="options">The options for this hotkey entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(Code code, Action action, HotKeyOptions options)
    => this.AddInternal(ModCode.None, code, arg => { action(); return ValueTask.CompletedTask; }, action.Target as IHandleEvent, options);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="code">The identifier of hotkey by code.</param>
    /// <param name="action">The callback action that will be invoked when user enter the key without any modifiers on the browser.</param>
    /// <param name="options">The options for this hotkey entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(Code code, Action<HotKeyEntryByCode> action, HotKeyOptions options)
        => this.AddInternal(ModCode.None, code, arg => { action(arg); return ValueTask.CompletedTask; }, action.Target as IHandleEvent, options);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="code">The identifier of hotkey by code.</param>
    /// <param name="action">The callback action that will be invoked when user enter the key without any modifiers on the browser.</param>
    /// <param name="options">The options for this hotkey entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(Code code, Func<ValueTask> action, HotKeyOptions options)
        => this.AddInternal(ModCode.None, code, _ => action(), action.Target as IHandleEvent, options);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="code">The identifier of hotkey by code.</param>
    /// <param name="action">The callback action that will be invoked when user enter the key without any modifiers on the browser.</param>
    /// <param name="options">The options for this hotkey entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(Code code, Func<Task> action, HotKeyOptions options)
        => this.AddInternal(ModCode.None, code, async _ => await action(), action.Target as IHandleEvent, options);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="code">The identifier of hotkey by code.</param>
    /// <param name="action">The callback action that will be invoked when user enter the key without any modifiers on the browser.</param>
    /// <param name="options">The options for this hotkey entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(Code code, Func<HotKeyEntryByCode, ValueTask> action, HotKeyOptions options)
        => this.AddInternal(ModCode.None, code, action, action.Target as IHandleEvent, options);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="code">The identifier of hotkey by code.</param>
    /// <param name="action">The callback action that will be invoked when user enter the key without any modifiers on the browser.</param>
    /// <param name="options">The options for this hotkey entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(Code code, Func<HotKeyEntryByCode, Task> action, HotKeyOptions options)
        => this.AddInternal(ModCode.None, code, async arg => await action(arg), action.Target as IHandleEvent, options);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="code">The identifier of hotkey by code.</param>
    /// <param name="action">The callback action that will be invoked when user enter the key without any modifiers on the browser.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(Code code, Action action, string description = "", Exclude exclude = Exclude.Default)
    => this.AddInternal(ModCode.None, code, arg => { action(); return ValueTask.CompletedTask; }, action.Target as IHandleEvent, new() { Description = description, Exclude = exclude });

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="code">The identifier of hotkey by code.</param>
    /// <param name="action">The callback action that will be invoked when user enter the key without any modifiers on the browser.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(Code code, Action<HotKeyEntryByCode> action, string description = "", Exclude exclude = Exclude.Default)
        => this.AddInternal(ModCode.None, code, arg => { action(arg); return ValueTask.CompletedTask; }, action.Target as IHandleEvent, new() { Description = description, Exclude = exclude });

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="code">The identifier of hotkey by code.</param>
    /// <param name="action">The callback action that will be invoked when user enter the key without any modifiers on the browser.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(Code code, Func<ValueTask> action, string description = "", Exclude exclude = Exclude.Default)
        => this.AddInternal(ModCode.None, code, _ => action(), action.Target as IHandleEvent, new() { Description = description, Exclude = exclude });

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="code">The identifier of hotkey by code.</param>
    /// <param name="action">The callback action that will be invoked when user enter the key without any modifiers on the browser.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(Code code, Func<Task> action, string description = "", Exclude exclude = Exclude.Default)
        => this.AddInternal(ModCode.None, code, async _ => await action(), action.Target as IHandleEvent, new() { Description = description, Exclude = exclude });

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="code">The identifier of hotkey by code.</param>
    /// <param name="action">The callback action that will be invoked when user enter the key without any modifiers on the browser.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(Code code, Func<HotKeyEntryByCode, ValueTask> action, string description = "", Exclude exclude = Exclude.Default)
        => this.AddInternal(ModCode.None, code, action, action.Target as IHandleEvent, new() { Description = description, Exclude = exclude });

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="code">The identifier of hotkey by code.</param>
    /// <param name="action">The callback action that will be invoked when user enter the key without any modifiers on the browser.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(Code code, Func<HotKeyEntryByCode, Task> action, string description = "", Exclude exclude = Exclude.Default)
        => this.AddInternal(ModCode.None, code, async arg => await action(arg), action.Target as IHandleEvent, new() { Description = description, Exclude = exclude });

    // -----------------------------------------------------------------------------------------------

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="code">The identifier of hotkey by code.</param>
    /// <param name="action">The callback action that will be invoked when user enter modifiers + key combination on the browser.</param>
    /// <param name="options">The options for this hotkey entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(ModCode modifiers, Code code, Action action, HotKeyOptions options)
        => this.AddInternal(modifiers, code, arg => { action(); return ValueTask.CompletedTask; }, action.Target as IHandleEvent, options);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="code">The identifier of hotkey by code.</param>
    /// <param name="action">The callback action that will be invoked when user enter modifiers + key combination on the browser.</param>
    /// <param name="options">The options for this hotkey entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(ModCode modifiers, Code code, Action<HotKeyEntryByCode> action, HotKeyOptions options)
        => this.AddInternal(modifiers, code, arg => { action(arg); return ValueTask.CompletedTask; }, action.Target as IHandleEvent, options);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="code">The identifier of hotkey by code.</param>
    /// <param name="action">The callback action that will be invoked when user enter modifiers + key combination on the browser.</param>
    /// <param name="options">The options for this hotkey entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(ModCode modifiers, Code code, Func<ValueTask> action, HotKeyOptions options)
        => this.AddInternal(modifiers, code, _ => action(), action.Target as IHandleEvent, options);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="code">The identifier of hotkey by code.</param>
    /// <param name="action">The callback action that will be invoked when user enter modifiers + key combination on the browser.</param>
    /// <param name="options">The options for this hotkey entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(ModCode modifiers, Code code, Func<Task> action, HotKeyOptions options)
        => this.AddInternal(modifiers, code, async _ => await action(), action.Target as IHandleEvent, options);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="code">The identifier of hotkey by code.</param>
    /// <param name="action">The callback action that will be invoked when user enter modifiers + key combination on the browser.</param>
    /// <param name="options">The options for this hotkey entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(ModCode modifiers, Code code, Func<HotKeyEntryByCode, ValueTask> action, HotKeyOptions options)
        => this.AddInternal(modifiers, code, action, action.Target as IHandleEvent, options);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="code">The identifier of hotkey by code.</param>
    /// <param name="action">The callback action that will be invoked when user enter modifiers + key combination on the browser.</param>
    /// <param name="options">The options for this hotkey entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(ModCode modifiers, Code code, Func<HotKeyEntryByCode, Task> action, HotKeyOptions options)
        => this.AddInternal(modifiers, code, async arg => await action(arg), action.Target as IHandleEvent, options);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="code">The identifier of hotkey by code.</param>
    /// <param name="action">The callback action that will be invoked when user enter modifiers + key combination on the browser.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(ModCode modifiers, Code code, Action action, string description = "", Exclude exclude = Exclude.Default)
        => this.AddInternal(modifiers, code, arg => { action(); return ValueTask.CompletedTask; }, action.Target as IHandleEvent, new() { Description = description, Exclude = exclude });

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="code">The identifier of hotkey by code.</param>
    /// <param name="action">The callback action that will be invoked when user enter modifiers + key combination on the browser.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(ModCode modifiers, Code code, Action<HotKeyEntryByCode> action, string description = "", Exclude exclude = Exclude.Default)
        => this.AddInternal(modifiers, code, arg => { action(arg); return ValueTask.CompletedTask; }, action.Target as IHandleEvent, new() { Description = description, Exclude = exclude });

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="code">The identifier of hotkey by code.</param>
    /// <param name="action">The callback action that will be invoked when user enter modifiers + key combination on the browser.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(ModCode modifiers, Code code, Func<ValueTask> action, string description = "", Exclude exclude = Exclude.Default)
        => this.AddInternal(modifiers, code, _ => action(), action.Target as IHandleEvent, new() { Description = description, Exclude = exclude });

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="code">The identifier of hotkey by code.</param>
    /// <param name="action">The callback action that will be invoked when user enter modifiers + key combination on the browser.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(ModCode modifiers, Code code, Func<Task> action, string description = "", Exclude exclude = Exclude.Default)
        => this.AddInternal(modifiers, code, async _ => await action(), action.Target as IHandleEvent, new() { Description = description, Exclude = exclude });

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="code">The identifier of hotkey by code.</param>
    /// <param name="action">The callback action that will be invoked when user enter modifiers + key combination on the browser.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(ModCode modifiers, Code code, Func<HotKeyEntryByCode, ValueTask> action, string description = "", Exclude exclude = Exclude.Default)
        => this.AddInternal(modifiers, code, action, action.Target as IHandleEvent, new() { Description = description, Exclude = exclude });

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="code">The identifier of hotkey by code.</param>
    /// <param name="action">The callback action that will be invoked when user enter modifiers + key combination on the browser.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(ModCode modifiers, Code code, Func<HotKeyEntryByCode, Task> action, string description = "", Exclude exclude = Exclude.Default)
        => this.AddInternal(modifiers, code, async arg => await action(arg), action.Target as IHandleEvent, new() { Description = description, Exclude = exclude });

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="code">The identifier of hotkey by code.</param>
    /// <param name="action">The callback action that will be invoked when user enter modifiers + key combination on the browser.</param>
    /// <param name="ownerOfAction">The instance of a Razor component that is an owner of the callback action method.</param>
    /// <param name="options">The options for this hotkey entry.</param>
    /// <returns>This context.</returns>
    private HotKeysContext AddInternal(ModCode modifiers, Code code, Func<HotKeyEntryByCode, ValueTask> action, IHandleEvent? ownerOfAction, HotKeyOptions options)
        => this.AddInternal(new HotKeyEntryByCode(this._Logger, modifiers, code, action, ownerOfAction, options));

    // ===============================================================================================

    private HotKeysContext AddInternal(HotKeyEntry hotkeyEntry)
    {
        lock (this._Keys) this._Keys.Add(hotkeyEntry);
        this.RegisterAsync(hotkeyEntry);
        hotkeyEntry._NotifyStateChanged = this.OnNotifyStateChanged;
        return this;
    }

    // ===============================================================================================


    private async void RegisterAsync(HotKeyEntry hotKeyEntry)
    {
        await this._Syncer.InvokeAsync(async () =>
        {
            if (this._Disposed) return false;

            await JS.InvokeSafeAsync(async () =>
            {
                var context = await this._JSContextTask;
                hotKeyEntry.Id = await context.InvokeAsync<int>(
                    "register",
                    hotKeyEntry._ObjectRef, hotKeyEntry.Mode, hotKeyEntry._Modifiers, hotKeyEntry._KeyEntry, hotKeyEntry.Exclude, hotKeyEntry.ExcludeSelector, hotKeyEntry.State.Disabled);
            }, this._Logger);

            return true;
        });
    }

    private async void OnNotifyStateChanged(HotKeyEntry hotKeyEntry)
    {
        await this._Syncer.InvokeAsync(async () =>
        {
            if (this._Disposed) return false;

            await JS.InvokeSafeAsync(async () =>
            {
                var context = await this._JSContextTask;
                await context.InvokeVoidAsync("update", hotKeyEntry.Id, hotKeyEntry.State.Disabled);
            }, this._Logger);
            return true;
        });
    }

    private async ValueTask UnregisterAsync(HotKeyEntry hotKeyEntry)
    {
        await JS.InvokeSafeAsync(async () =>
        {
            var context = await this._JSContextTask;
            await context.InvokeVoidAsync("unregister", hotKeyEntry.Id);
        }, this._Logger);
        hotKeyEntry.Dispose();
    }

    private const string _AMBIGUOUS_PARAMETER_EXCEPTION_MESSAGE = "Specified parameters are ambiguous to identify the single hotkey entry.";

    private TKeyEntryType FindHotKeyEntry<TKeyEntryType, TModifiers, TKeyEntry>(Enum modifiers, TKeyEntry keyEntry, string description, Exclude exclude, string excludeSelector)
        where TKeyEntryType : HotKeyEntry
        where TModifiers : Enum
    {
        var keyEntryString = keyEntry?.ToString();
        var modifiresInt = Convert.ToInt32(modifiers);
        var filteredKeys = this._Keys.OfType<TKeyEntryType>().ToArray();

        filteredKeys = filteredKeys.Where(k => k._Modifiers == modifiresInt && k._KeyEntry == keyEntryString).ToArray();
        if (filteredKeys.Length == 0) throw new KeyNotFoundException();

        if (filteredKeys.Length > 1) filteredKeys = filteredKeys.Where(k => k.ExcludeSelector == excludeSelector && k.Exclude == exclude).ToArray();
        if (filteredKeys.Length > 1) filteredKeys = filteredKeys.Where(k => k.Description == description).ToArray();
        if (filteredKeys.Length == 1) return filteredKeys.First();
        throw new ArgumentException(_AMBIGUOUS_PARAMETER_EXCEPTION_MESSAGE);
    }

    /// <summary>
    /// Retrieves the hot key entry associated with the specified key, and optional criteria.
    /// </summary>
    /// <param name="key">The key for which to retrieve the hot key entry.</param>
    /// <param name="description">An optional description to associate with the hot key entry. If not specified, the description is empty.</param>
    /// <param name="exclude">Specifies exclusion criteria for the search. The default is <see cref="Exclude.Default"/>.</param>
    /// <param name="excludeSelector">An optional selector string used to further refine the exclusion criteria. If not specified, no additional selector is applied.</param>
    /// <returns>A <see cref="HotKeyEntryByKey"/> instance that matches the specified criteria, or throws <see cref="KeyNotFoundException"/> if no matching entry is found.</returns>
    public HotKeyEntryByKey GetHotKey(Key key, string description = "", Exclude exclude = Exclude.Default, string excludeSelector = "")
        => this.GetHotKey(ModKey.None, key, description, exclude, excludeSelector);

    /// <summary>
    /// Retrieves a hotkey entry that matches the specified modifier, key, and optional criteria.
    /// </summary>
    /// <param name="modifiers">The modifier key to match. Specify <see cref="ModKey.None"/> to indicate no modifier.</param>
    /// <param name="key">The primary key to match for the hotkey entry.</param>
    /// <param name="description">An optional description to further filter the hotkey entry. If not specified, all descriptions are considered.</param>
    /// <param name="exclude">Specifies exclusion criteria for the search. The default is <see cref="Exclude.Default"/>.</param>
    /// <param name="excludeSelector">An optional selector string used to refine the exclusion logic. If not specified, no additional exclusion is applied.</param>
    /// <returns>A <see cref="HotKeyEntryByKey"/> instance that matches the specified criteria, or throws <see cref="KeyNotFoundException"/> if no matching entry is found.</returns>
    public HotKeyEntryByKey GetHotKey(ModKey modifiers, Key key, string description = "", Exclude exclude = Exclude.Default, string excludeSelector = "")
        => this.FindHotKeyEntry<HotKeyEntryByKey, ModKey, Key>(modifiers, key, description, exclude, excludeSelector);

    /// <summary>
    /// Retrieves the hot key entry associated with the specified code, and optional criteria.
    /// </summary>
    /// <param name="code">The code representing the hot key to retrieve.</param>
    /// <param name="description">An optional description to associate with the hot key. If not specified, the description is empty.</param>
    /// <param name="exclude">Specifies exclusion criteria for the search. The default is <see cref="Exclude.Default"/>.</param>
    /// <param name="excludeSelector">An optional selector string used to refine the exclusion logic. If not specified, no additional exclusion is applied.</param>
    /// <returns>A <see cref="HotKeyEntryByCode"/> instance that matches the specified criteria, or throws <see cref="KeyNotFoundException"/> if no matching entry is found.</returns>
    public HotKeyEntryByCode GetHotKey(Code code, string description = "", Exclude exclude = Exclude.Default, string excludeSelector = "")
        => this.GetHotKey(ModCode.None, code, description, exclude, excludeSelector);

    /// <summary>
    /// Retrieves the hotkey entry that matches the specified modifier, code, and optional criteria.
    /// </summary>
    /// <param name="none">The modifier code to match. Represents the required modifier keys (such as Ctrl, Alt, or Shift) for the hotkey.</param>
    /// <param name="code">The key code to match. Specifies the primary key for the hotkey combination.</param>
    /// <param name="description">An optional description used to further filter or identify the hotkey entry. The default is an empty string.</param>
    /// <param name="exclude">Specifies exclusion criteria for the search. The default is <see cref="Exclude.Default"/>.</param>
    /// <param name="excludeSelector">An optional selector string used to refine the exclusion logic. If not specified, no additional exclusion is applied.</param>
    /// <returns>A <see cref="HotKeyEntryByCode"/> instance that matches the specified criteria, or throws <see cref="KeyNotFoundException"/> if no matching entry is found.</returns>
    public HotKeyEntryByCode GetHotKey(ModCode none, Code code, string description = "", Exclude exclude = Exclude.Default, string excludeSelector = "")
        => this.FindHotKeyEntry<HotKeyEntryByCode, ModCode, Code>(none, code, description, exclude, excludeSelector);


    /// <summary>
    /// Remove a hotkey entriy from this context.<br/>
    /// If the <paramref name="key"/> parameter cannot find any hotkey entry, this method will return without exception.<br/>
    /// If only one hotkey entry can be identified by the <paramref name="key"/> parameter, it will be removed even if the other parameters are not matched.<br/>
    /// If the <paramref name="key"/> parameter identifies two or more hotkey entries, the other parameters are referenced to identify the single hotkey entry to be removed.<br/>
    /// If the parameters can not determine a single hotkey entry, the <see cref="ArgumentException"/> exception will be thrown.
    /// </summary>
    /// <param name="key">The identifier of hotkey.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <param name="excludeSelector">Additional CSS selector for HTML elements that will not allow hotkey to work.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Remove(Key key, string description = "", Exclude exclude = Exclude.Default, string excludeSelector = "")
        => this.Remove(ModKey.None, key, description, exclude, excludeSelector);

    /// <summary>
    /// Remove a hotkey entriy from this context.<br/>
    /// If the combination of the <paramref name="modifiers"/> and the <paramref name="key"/> parameters cannot find any hotkey entry, this method will return without exception.<br/>
    /// If only one hotkey entry can be identified by the combination of the <paramref name="modifiers"/> and the <paramref name="key"/> parameters, it will be removed even if the other parameters are not matched.<br/>
    /// If the combination of the <paramref name="modifiers"/> and the <paramref name="key"/> parameters identifies two or more hotkey entries, the other parameters are referenced to identify the single hotkey entry to be removed.<br/>
    /// If the parameters can not determine a single hotkey entry, the <see cref="ArgumentException"/> exception will be thrown.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="key">The identifier of hotkey.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <param name="excludeSelector">Additional CSS selector for HTML elements that will not allow hotkey to work.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Remove(ModKey modifiers, Key key, string description = "", Exclude exclude = Exclude.Default, string excludeSelector = "")
        => this.Remove(this.GetHotKey(modifiers, key, description, exclude, excludeSelector));

    /// <summary>
    /// Remove a hotkey entriy from this context.<br/>
    /// If the <paramref name="code"/> parameter cannot find any hotkey entry, this method will return without exception.<br/>
    /// If only one hotkey entry can be identified by the <paramref name="code"/> parameter, it will be removed even if the other parameters are not matched.<br/>
    /// If the <paramref name="code"/> parameter identifies two or more hotkey entries, the other parameters are referenced to identify the single hotkey entry to be removed.<br/>
    /// If the parameters can not determine a single hotkey entry, the <see cref="ArgumentException"/> exception will be thrown.
    /// </summary>
    /// <param name="code">The identifier of hotkey.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <param name="excludeSelector">Additional CSS selector for HTML elements that will not allow hotkey to work.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Remove(Code code, string description = "", Exclude exclude = Exclude.Default, string excludeSelector = "")
        => this.Remove(ModCode.None, code, description, exclude, excludeSelector);

    /// <summary>
    /// Remove a hotkey entriy from this context.<br/>
    /// If the combination of the <paramref name="modifiers"/> and the <paramref name="code"/> parameters cannot find any hotkey entry, this method will return without exception.<br/>
    /// If only one hotkey entry can be identified by the combination of the <paramref name="modifiers"/> and the <paramref name="code"/> parameters, it will be removed even if the other parameters are not matched.<br/>
    /// If the combination of the <paramref name="modifiers"/> and the <paramref name="code"/> parameters identifies two or more hotkey entries, the other parameters are referenced to identify the single hotkey entry to be removed.<br/>
    /// If the parameters can not determine a single hotkey entry, the <see cref="ArgumentException"/> exception will be thrown.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="code">The identifier of hotkey.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <param name="excludeSelector">Additional CSS selector for HTML elements that will not allow hotkey to work.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Remove(ModCode modifiers, Code code, string description = "", Exclude exclude = Exclude.Default, string excludeSelector = "")
        => this.Remove(this.GetHotKey(modifiers, code, description, exclude, excludeSelector));

    /// <summary>
    /// Remove all hotkey entries from this context where the <paramref name="filter"/> function returns <c>true</c>.
    /// </summary>
    /// <param name="filter"></param>
    public HotKeysContext Remove(Func<IEnumerable<HotKeyEntry>, IEnumerable<HotKeyEntry>> filter)
    {
        var entries = filter.Invoke(this._Keys).ToArray();
        foreach (var entry in entries) this.Remove(entry);
        return this;
    }

    private HotKeysContext Remove(HotKeyEntry entry)
    {
        lock (this._Keys) this._Keys.Remove(entry);
        entry._NotifyStateChanged = null;

        var _ = this._Syncer.InvokeAsync(async () =>
        {
            if (this._Disposed) return false;
            await this.UnregisterAsync(entry);
            return true;
        }, this._Logger);
        return this;
    }

    /// <summary>
    /// Deactivate the hot key entry contained in this context.
    /// </summary>
    [Obsolete("Use the DisposeAsync instead."), EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
    public void Dispose() { var _ = this.DisposeAsync(); }
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize

    /// <summary>
    /// Deactivate the hot key entry contained in this context.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        await this._Syncer.InvokeAsync(async () =>
        {
            if (this._Disposed) return false;

            var context = await this._JSContextTask;
            this._Disposed = true;
            foreach (var entry in this._Keys)
            {
                await this.UnregisterAsync(entry);
                entry._NotifyStateChanged = null;
            }
            lock (this._Keys) this._Keys.Clear();
            await JS.InvokeSafeAsync(async () =>
            {
                await context.InvokeVoidAsync("dispose");
                await context.DisposeAsync();
            }, this._Logger);
            return true;
        });
    }
}