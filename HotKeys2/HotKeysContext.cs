using System.ComponentModel;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace Toolbelt.Blazor.HotKeys2;

/// <summary>
/// Current active hotkeys set.
/// </summary>
public partial class HotKeysContext : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// The collection of Hotkey entries.
    /// </summary>
    public List<HotKeyEntry> Keys { get; } = new List<HotKeyEntry>();

    private readonly Task<IJSObjectReference> _AttachTask;

    private readonly ILogger _Logger;

    private readonly SemaphoreSlim _Syncer = new(1, 1);

    private bool _IsDisposed = false;

    /// <summary>
    /// Initialize a new instance of the HotKeysContext class.
    /// </summary>
    internal HotKeysContext(Task<IJSObjectReference> attachTask, ILogger logger)
    {
        this._AttachTask = attachTask;
        this._Logger = logger;
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
        lock (this.Keys) this.Keys.Add(hotkeyEntry);
        this.RegisterAsync(hotkeyEntry);
        hotkeyEntry._NotifyStateChanged = this.OnNotifyStateChanged;
        return this;
    }

    // ===============================================================================================


    private void RegisterAsync(HotKeyEntry hotKeyEntry)
    {
        var _ = this._Syncer.InvokeAsync(async () =>
        {
            await this.InvokeJsSafeAsync(async () =>
            {
                if (this._IsDisposed) return;

                var module = await this._AttachTask;
                hotKeyEntry.Id = await module.InvokeAsync<int>(
                    "Toolbelt.Blazor.HotKeys2.register",
                    hotKeyEntry._ObjectRef, hotKeyEntry.Mode, hotKeyEntry._Modifiers, hotKeyEntry._KeyEntry, hotKeyEntry.Exclude, hotKeyEntry.ExcludeSelector, hotKeyEntry.State.Disabled);
            });
            return true;
        });
    }

    private void OnNotifyStateChanged(HotKeyEntry hotKeyEntry)
    {
        var _ = this.InvokeJsSafeAsync(async () =>
        {
            var module = await this._AttachTask;
            await module.InvokeVoidAsync("Toolbelt.Blazor.HotKeys2.update", hotKeyEntry.Id, hotKeyEntry.State.Disabled);
        });
    }

    private async ValueTask UnregisterAsync(HotKeyEntry hotKeyEntry)
    {
        await this.InvokeJsSafeAsync(async () =>
        {
            var module = await this._AttachTask;
            await module.InvokeVoidAsync("Toolbelt.Blazor.HotKeys2.unregister", hotKeyEntry.Id);
        });

        await this.InvokeJsSafeAsync(() => { hotKeyEntry.Dispose(); return ValueTask.CompletedTask; });
    }

    private async ValueTask InvokeJsSafeAsync(Func<ValueTask> action)
    {
        try { await action(); }
        catch (JSDisconnectedException) { } // Ignore this exception because it is thrown when the user navigates to another page.
        catch (Exception ex) { this._Logger.LogError(ex, ex.Message); }
    }

    private const string _AMBIGUOUS_PARAMETER_EXCEPTION_MESSAGE = "Specified parameters are ambiguous to identify the single hotkey entry that should be removed.";

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
    public HotKeysContext Remove(Key key, string description = "", Exclude exclude = Exclude.Default, string excludeSelector = "") =>
        this.Remove(ModKey.None, key, description, exclude, excludeSelector);


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
    {
        var keyEntry = key.ToString();
        return this.Remove(keys =>
        {
            var removeCandidates = keys.OfType<HotKeyEntryByKey>().Where(k => k.Modifiers == modifiers && k.Key.ToString() == keyEntry).ToArray();
            if (removeCandidates.Length <= 1) return removeCandidates;
            removeCandidates = removeCandidates.Where(k => k.Exclude == exclude && k.ExcludeSelector == excludeSelector).ToArray();
            if (removeCandidates.Length == 1) return removeCandidates;
            if (removeCandidates.Length == 0) throw new ArgumentException(_AMBIGUOUS_PARAMETER_EXCEPTION_MESSAGE);
            removeCandidates = removeCandidates.Where(k => k.Description == description).ToArray();
            if (removeCandidates.Length == 1) return removeCandidates;
            throw new ArgumentException(_AMBIGUOUS_PARAMETER_EXCEPTION_MESSAGE);
        });
    }

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
    {
        var keyEntry = code.ToString();
        return this.Remove(keys =>
        {
            var removeCandidates = keys.OfType<HotKeyEntryByCode>().Where(k => k.Modifiers == modifiers && k.Code.ToString() == keyEntry).ToArray();
            if (removeCandidates.Length <= 1) return removeCandidates;
            removeCandidates = removeCandidates.Where(k => k.ExcludeSelector == excludeSelector && k.Exclude == exclude).ToArray();
            if (removeCandidates.Length == 1) return removeCandidates;
            if (removeCandidates.Length == 0) throw new ArgumentException(_AMBIGUOUS_PARAMETER_EXCEPTION_MESSAGE);
            removeCandidates = removeCandidates.Where(k => k.Description == description).ToArray();
            if (removeCandidates.Length == 1) return removeCandidates;
            throw new ArgumentException(_AMBIGUOUS_PARAMETER_EXCEPTION_MESSAGE);
        });
    }

    /// <summary>
    /// Remove all hotkey entries from this context where the <paramref name="filter"/> function returns <c>true</c>.
    /// </summary>
    /// <param name="filter"></param>
    public HotKeysContext Remove(Func<IEnumerable<HotKeyEntry>, IEnumerable<HotKeyEntry>> filter)
    {
        var entries = filter.Invoke(this.Keys).ToArray();
        foreach (var entry in entries)
        {
            var _ = this.UnregisterAsync(entry);
            lock (this.Keys) this.Keys.Remove(entry);
            entry._NotifyStateChanged = null;
        }
        return this;
    }

    /// <summary>
    /// Deactivate the hot key entry contained in this context.
    /// </summary>
    [Obsolete("Use the DisposeAsync instead."), EditorBrowsable(EditorBrowsableState.Never)]
    public void Dispose() { var _ = this.DisposeAsync(); }

    /// <summary>
    /// Deactivate the hot key entry contained in this context.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        await this._Syncer.InvokeAsync(async () =>
        {
            this._IsDisposed = true;
            foreach (var entry in this.Keys)
            {
                await this.UnregisterAsync(entry);
                entry._NotifyStateChanged = null;
            }
            this.Keys.Clear();
            return true;
        });
    }
}