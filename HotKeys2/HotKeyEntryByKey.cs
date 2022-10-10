namespace Toolbelt.Blazor.HotKeys2;
public class HotKeyEntryByKey : HotKeyEntry
{
    /// <summary>
    /// Get the combination of modifier keys flags.
    /// </summary>
    public ModKeys Modifiers { get; }

    /// <summary>
    /// Get the identifier of hotkey.<br/>If this property return 0, it means the HotKey entry works based on DOM event's native key name.
    /// </summary>
    public Key Key { get; }

    /// <summary>
    /// Get the callback action that will be invoked when user enter modKeys + key combination on the browser.
    /// </summary>
    private readonly Func<HotKeyEntryByKey, ValueTask> _Action;

    /// <summary>
    /// Initialize a new instance of the HotKeyEntryByKey class.
    /// </summary>
    /// <param name="modKeys">The combination of modifier keys flags.</param>
    /// <param name="key">The identifier of hotkey.</param>
    /// <param name="action">The callback action that will be invoked when user enter modKeys + key combination on the browser.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    public HotKeyEntryByKey(ModKeys modKeys, Key key, Exclude exclude, string? description, Func<HotKeyEntryByKey, ValueTask> action)
        : base(HotKeyMode.ByKey, typeof(ModKeys), (int)modKeys, key.ToString(), exclude, description)
    {
        this.Modifiers = modKeys;
        this.Key = key;
        this._Action = action;
    }

    protected override void InvokeCallbackAction()
    {
        this._Action.Invoke(this);
    }
}
