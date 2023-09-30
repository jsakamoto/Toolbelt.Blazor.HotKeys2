using System.ComponentModel;
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
    [Obsolete, EditorBrowsable(EditorBrowsableState.Never)]
    public HotKeyEntryByCode(ModCode modCodes, Code code, Exclude exclude, string? description, Func<HotKeyEntryByCode, ValueTask> action)
        : this(modCodes, code, exclude, excludeSelector: "", description, action)
    {
    }

    /// <summary>
    /// Initialize a new instance of the HotKeyEntryByKey class.
    /// </summary>
    /// <param name="modCodes">The combination of modifier keys flags.</param>
    /// <param name="code">The identifier of hotkey.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <param name="excludeSelector">Additional CSS selector for HTML elements that will not allow hotkey to work.</param>
    /// <param name="action">The callback action that will be invoked when user enter modKeys + key combination on the browser.</param>
    public HotKeyEntryByCode(ModCode modCodes, Code code, Exclude exclude, string excludeSelector, string? description, Func<HotKeyEntryByCode, ValueTask> action)
        : base(null, HotKeyMode.ByCode, typeof(ModCode), (int)modCodes, code.ToString(), action.Target as IHandleEvent, new()
        {
            Description = description ?? "",
            Exclude = exclude,
            ExcludeSelector = excludeSelector,
        })
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
    /// <param name="action">The callback action that will be invoked when user enter modKeys + key combination on the browser.</param>
    /// <param name="ownerOfAction">The instance of a Razor component that is an owner of the callback action method.</param>
    /// <param name="options">The options for a hotkey entry.</param>
    internal HotKeyEntryByCode(ILogger logger, ModCode modCodes, Code code, Func<HotKeyEntryByCode, ValueTask> action, IHandleEvent? ownerOfAction, HotKeyOptions options)
        : base(logger, HotKeyMode.ByCode, typeof(ModCode), (int)modCodes, code.ToString(), ownerOfAction, options)
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
