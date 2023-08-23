using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace Toolbelt.Blazor.HotKeys2;
public class HotKeyEntryByKey : HotKeyEntry
{
    /// <summary>
    /// Get the combination of modifier keys flags.
    /// </summary>
    public ModKey Modifiers { get; }

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
    public HotKeyEntryByKey(ModKey modKeys, Key key, Exclude exclude, string? description, Func<HotKeyEntryByKey, ValueTask> action)
        : base(null, HotKeyMode.ByKey, typeof(ModKey), (int)modKeys, key.ToString(), exclude, description, action.Target as IHandleEvent)
    {
        this.Modifiers = modKeys;
        this.Key = key;
        this._Action = action;
    }

    /// <summary>
    /// Initialize a new instance of the HotKeyEntryByKey class.
    /// </summary>
    /// <param name="logger">The instance of ILogger.</param>
    /// <param name="modKeys">The combination of modifier keys flags.</param>
    /// <param name="key">The identifier of hotkey.</param>
    /// <param name="action">The callback action that will be invoked when user enter modKeys + key combination on the browser.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <param name="ownerOfAction">The instance of a Razor component that is an owner of the callback action method.</param>
    internal HotKeyEntryByKey(ILogger logger, ModKey modKeys, Key key, Exclude exclude, string? description, Func<HotKeyEntryByKey, ValueTask> action, IHandleEvent? ownerOfAction)
        : base(logger, HotKeyMode.ByKey, typeof(ModKey), (int)modKeys, key.ToString(), exclude, description, ownerOfAction)
    {
        this.Modifiers = modKeys;
        this.Key = key;
        this._Action = action;
    }

    protected override void InvokeCallbackAction()
    {
        this.CommonProcess(() => this._Action.Invoke(this));
    }
}
