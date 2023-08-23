using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace Toolbelt.Blazor.HotKeys2;

public class HotKeyEntryByCode : HotKeyEntry
{
    /// <summary>
    /// Get the combination of modifier codes flags.
    /// </summary>
    public ModCode Modifiers { get; }

    public Code Code { get; }

    /// <summary>
    /// Get the callback action that will be invoked when user enter modKeys + key combination on the browser.
    /// </summary>
    private readonly Func<HotKeyEntryByCode, ValueTask> _Action;

    /// <summary>
    /// Initialize a new instance of the HotKeyEntryByKey class.
    /// </summary>
    /// <param name="modCodes">The combination of modifier keys flags.</param>
    /// <param name="code">The identifier of hotkey.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <param name="action">The callback action that will be invoked when user enter modKeys + key combination on the browser.</param>
    public HotKeyEntryByCode(ModCode modCodes, Code code, Exclude exclude, string? description, Func<HotKeyEntryByCode, ValueTask> action)
        : base(null, HotKeyMode.ByCode, typeof(ModCode), (int)modCodes, code.ToString(), exclude, description, action.Target as IHandleEvent)
    {
        this.Modifiers = modCodes;
        this.Code = code;
        this._Action = action;
    }

    /// <summary>
    /// Initialize a new instance of the HotKeyEntryByKey class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="modCodes">The combination of modifier keys flags.</param>
    /// <param name="code">The identifier of hotkey.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <param name="action">The callback action that will be invoked when user enter modKeys + key combination on the browser.</param>
    /// <param name="ownerOfAction">The instance of a Razor component that is an owner of the callback action method.</param>
    internal HotKeyEntryByCode(ILogger logger, ModCode modCodes, Code code, Exclude exclude, string? description, Func<HotKeyEntryByCode, ValueTask> action, IHandleEvent? ownerOfAction)
        : base(logger, HotKeyMode.ByCode, typeof(ModCode), (int)modCodes, code.ToString(), exclude, description, ownerOfAction)
    {
        this.Modifiers = modCodes;
        this.Code = code;
        this._Action = action;
    }

    protected override void InvokeCallbackAction()
    {
        this.CommonProcess(() => this._Action.Invoke(this));
    }
}
