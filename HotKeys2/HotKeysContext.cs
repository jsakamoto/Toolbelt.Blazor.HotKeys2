using Microsoft.JSInterop;

namespace Toolbelt.Blazor.HotKeys2;

/// <summary>
/// Current active hotkeys set.
/// </summary>
public partial class HotKeysContext : IDisposable
{
    /// <summary>
    /// The collection of Hotkey entries.
    /// </summary>
    public List<HotKeyEntry> Keys { get; } = new List<HotKeyEntry>();

    private readonly Task<IJSObjectReference> _AttachTask;

    /// <summary>
    /// Initialize a new instance of the HotKeysContext class.
    /// </summary>
    internal HotKeysContext(Task<IJSObjectReference> attachTask)
    {
        this._AttachTask = attachTask;
    }

    // ===============================================================================================

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="key">The identifier of hotkey by key.</param>
    /// <param name="action">The callback action that will be invoked when user enter the key without any modifiers on the browser.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(Key key, Action action, string description = "", Exclude exclude = Exclude.Default)
    => this.Add(ModKeys.None, key, _ => { action(); return ValueTask.CompletedTask; }, description, exclude);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="key">The identifier of hotkey by key.</param>
    /// <param name="action">The callback action that will be invoked when user enter the key without any modifiers on the browser.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(Key key, Action<HotKeyEntryByKey> action, string description = "", Exclude exclude = Exclude.Default)
        => this.Add(ModKeys.None, key, arg => { action(arg); return ValueTask.CompletedTask; }, description, exclude);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="key">The identifier of hotkey by key.</param>
    /// <param name="action">The callback action that will be invoked when user enter the key without any modifiers on the browser.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(Key key, Func<ValueTask> action, string description = "", Exclude exclude = Exclude.Default)
        => this.Add(ModKeys.None, key, _ => action(), description, exclude);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="key">The identifier of hotkey by key.</param>
    /// <param name="action">The callback action that will be invoked when user enter the key without any modifiers on the browser.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(Key key, Func<HotKeyEntryByKey, ValueTask> action, string description = "", Exclude exclude = Exclude.Default)
        => this.Add(ModKeys.None, key, arg => action(arg), description, exclude);

    // -----------------------------------------------------------------------------------------------

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="key">The identifier of hotkey by key.</param>
    /// <param name="action">The callback action that will be invoked when user enter modifiers + key combination on the browser.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(ModKeys modifiers, Key key, Action action, string description = "", Exclude exclude = Exclude.Default)
        => this.Add(modifiers, key, _ => { action(); return ValueTask.CompletedTask; }, description, exclude);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="key">The identifier of hotkey by key.</param>
    /// <param name="action">The callback action that will be invoked when user enter modifiers + key combination on the browser.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(ModKeys modifiers, Key key, Action<HotKeyEntryByKey> action, string description = "", Exclude exclude = Exclude.Default)
        => this.Add(modifiers, key, arg => { action(arg); return ValueTask.CompletedTask; }, description, exclude);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="key">The identifier of hotkey by key.</param>
    /// <param name="action">The callback action that will be invoked when user enter modifiers + key combination on the browser.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(ModKeys modifiers, Key key, Func<ValueTask> action, string description = "", Exclude exclude = Exclude.Default)
        => this.Add(modifiers, key, _ => action(), description, exclude);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="key">The identifier of hotkey by key.</param>
    /// <param name="action">The callback action that will be invoked when user enter modifiers + key combination on the browser.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(ModKeys modifiers, Key key, Func<HotKeyEntryByKey, ValueTask> action, string description = "", Exclude exclude = Exclude.Default)
    {
        lock (this.Keys) this.Keys.Add(this.Register(new HotKeyEntryByKey(modifiers, key, exclude, description, action)));
        return this;
    }

    // ===============================================================================================

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="code">The identifier of hotkey by code.</param>
    /// <param name="action">The callback action that will be invoked when user enter the key without any modifiers on the browser.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(Code code, Action action, string description = "", Exclude exclude = Exclude.Default)
    => this.Add(ModCodes.None, code, arg => { action(); return ValueTask.CompletedTask; }, description, exclude);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="code">The identifier of hotkey by code.</param>
    /// <param name="action">The callback action that will be invoked when user enter the key without any modifiers on the browser.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(Code code, Action<HotKeyEntryByCode> action, string description = "", Exclude exclude = Exclude.Default)
        => this.Add(ModCodes.None, code, arg => { action(arg); return ValueTask.CompletedTask; }, description, exclude);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="code">The identifier of hotkey by code.</param>
    /// <param name="action">The callback action that will be invoked when user enter the key without any modifiers on the browser.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(Code code, Func<ValueTask> action, string description = "", Exclude exclude = Exclude.Default)
        => this.Add(ModCodes.None, code, _ => action(), description, exclude);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="code">The identifier of hotkey by code.</param>
    /// <param name="action">The callback action that will be invoked when user enter the key without any modifiers on the browser.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(Code code, Func<HotKeyEntryByCode, ValueTask> action, string description = "", Exclude exclude = Exclude.Default)
        => this.Add(ModCodes.None, code, action, description, exclude);

    // -----------------------------------------------------------------------------------------------

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="code">The identifier of hotkey by code.</param>
    /// <param name="action">The callback action that will be invoked when user enter modifiers + key combination on the browser.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(ModCodes modifiers, Code code, Action action, string description = "", Exclude exclude = Exclude.Default)
        => this.Add(modifiers, code, arg => { action(); return ValueTask.CompletedTask; }, description, exclude);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="code">The identifier of hotkey by code.</param>
    /// <param name="action">The callback action that will be invoked when user enter modifiers + key combination on the browser.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(ModCodes modifiers, Code code, Action<HotKeyEntryByCode> action, string description = "", Exclude exclude = Exclude.Default)
        => this.Add(modifiers, code, arg => { action(arg); return ValueTask.CompletedTask; }, description, exclude);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="code">The identifier of hotkey by code.</param>
    /// <param name="action">The callback action that will be invoked when user enter modifiers + key combination on the browser.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(ModCodes modifiers, Code code, Func<ValueTask> action, string description = "", Exclude exclude = Exclude.Default)
        => this.Add(modifiers, code, _ => action(), description, exclude);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="code">The identifier of hotkey by code.</param>
    /// <param name="action">The callback action that will be invoked when user enter modifiers + key combination on the browser.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(ModCodes modifiers, Code code, Func<HotKeyEntryByCode, ValueTask> action, string description = "", Exclude exclude = Exclude.Default)
    {
        lock (this.Keys) this.Keys.Add(this.Register(new HotKeyEntryByCode(modifiers, code, exclude, description, action)));
        return this;
    }

    // ===============================================================================================


    private HotKeyEntry Register(HotKeyEntry hotKeyEntry)
    {
        this._AttachTask.ContinueWith(t =>
        {
            if (t.IsCompleted && !t.IsFaulted)
            {
                return t.Result.InvokeAsync<int>(
                    "Toolbelt.Blazor.HotKeys2.register",
                    hotKeyEntry._ObjectRef, hotKeyEntry.Mode, hotKeyEntry._Modifiers, hotKeyEntry._KeyEntry, hotKeyEntry.Exclude).AsTask();
            }
            else
            {
                var tcs = new TaskCompletionSource<int>();
                tcs.TrySetException(t.Exception?.InnerExceptions ?? new[] { new Exception() }.AsEnumerable());
                return tcs.Task;
            }
        })
        .Unwrap()
        .ContinueWith(t =>
        {
            if (!t.IsCanceled && !t.IsFaulted) { hotKeyEntry.Id = t.Result; }
        });
        return hotKeyEntry;
    }

    private void Unregister(HotKeyEntry hotKeyEntry)
    {
        if (hotKeyEntry.Id == -1) return;

        this._AttachTask.ContinueWith(t =>
        {
            if (t.IsCompleted && !t.IsFaulted)
            {
                return t.Result.InvokeVoidAsync("Toolbelt.Blazor.HotKeys2.unregister", hotKeyEntry.Id).AsTask();
            }
            else
            {
                var tcs = new TaskCompletionSource<int>();
                tcs.TrySetException(t.Exception?.InnerExceptions ?? new[] { new Exception() }.AsEnumerable());
                return tcs.Task as Task;
            }
        })
        .ContinueWith(t =>
        {
            hotKeyEntry.Dispose();
        });
    }

    /// <summary>
    /// Remove one or more hotkey entries from this context.
    /// </summary>
    /// <param name="key">The identifier of hotkey.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Remove(Key key, string description = "", Exclude exclude = Exclude.Default) =>
        this.Remove(ModKeys.None, key, description, exclude);

    /// <summary>
    /// Remove one or more hotkey entries from this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="key">The identifier of hotkey.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Remove(ModKeys modifiers, Key key, string description = "", Exclude exclude = Exclude.Default)
    {
        var keyEntry = key.ToString();
        return this.Remove(keys => keys
            .OfType<HotKeyEntryByKey>()
            .Where(
                k => k.Modifiers == modifiers &&
                k.Key.ToString() == keyEntry &&
                k.Description == description &&
                k.Exclude == exclude));
    }

    /// <summary>
    /// Remove one or more hotkey entries from this context.
    /// </summary>
    /// <param name="code">The identifier of hotkey.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Remove(Code code, string description = "", Exclude exclude = Exclude.Default)
        => this.Remove(ModCodes.None, code, description, exclude);

    /// <summary>
    /// Remove one or more hotkey entries from this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="code">The identifier of hotkey.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Remove(ModCodes modifiers, Code code, string description = "", Exclude exclude = Exclude.Default)
    {
        var keyEntry = code.ToString();
        return this.Remove(keys => keys
            .OfType<HotKeyEntryByCode>()
            .Where(
                k => k.Modifiers == modifiers &&
                k.Code.ToString() == keyEntry &&
                k.Description == description &&
                k.Exclude == exclude));
    }

    private HotKeysContext Remove(Func<IEnumerable<HotKeyEntry>, IEnumerable<HotKeyEntry>> filter)
    {
        var entries = filter.Invoke(this.Keys).ToArray();
        foreach (var entry in entries)
        {
            this.Unregister(entry);
            lock (this.Keys) this.Keys.Remove(entry);
        }
        return this;
    }

    /// <summary>
    /// Deactivate the hot key entry contained in this context.
    /// </summary>
    public void Dispose()
    {
        foreach (var entry in this.Keys)
        {
            this.Unregister(entry);
        }
        this.Keys.Clear();
    }
}