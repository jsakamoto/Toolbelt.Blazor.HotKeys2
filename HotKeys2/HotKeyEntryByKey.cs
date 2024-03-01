﻿using System.ComponentModel;
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
    /// <param name="context">The instance of <see cref="HotKeysContext"/> that is an owner of this hot key entry.</param>
    /// <param name="modKeys">The combination of modifier keys flags.</param>
    /// <param name="key">The identifier of hotkey.</param>
    /// <param name="action">The callback action that will be invoked when user enter modKeys + key combination on the browser.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    [Obsolete, EditorBrowsable(EditorBrowsableState.Never)]
    public HotKeyEntryByKey(HotKeysContext context, ModKey modKeys, Key key, Exclude exclude, string? description, Func<HotKeyEntryByKey, ValueTask> action)
        : this(context, modKeys, key, exclude, excludeSelector: "", description, action)
    {
    }

    /// <summary>
    /// Initialize a new instance of the HotKeyEntryByKey class.
    /// </summary>
    /// <param name="context">The instance of <see cref="HotKeysContext"/> that is an owner of this hot key entry.</param>
    /// <param name="modKeys">The combination of modifier keys flags.</param>
    /// <param name="key">The identifier of hotkey.</param>
    /// <param name="action">The callback action that will be invoked when user enter modKeys + key combination on the browser.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    /// <param name="excludeSelector">Additional CSS selector for HTML elements that will not allow hotkey to work.</param>
    public HotKeyEntryByKey(HotKeysContext context, ModKey modKeys, Key key, Exclude exclude, string excludeSelector, string? description, Func<HotKeyEntryByKey, ValueTask> action)
        : base(context, null, HotKeyMode.ByKey, typeof(ModKey), (int)modKeys, key.ToString(), action.Target as IHandleEvent, new()
        {
            Description = description ?? "",
            Exclude = exclude,
            ExcludeSelector = excludeSelector,
        })
    {
        this.Modifiers = modKeys;
        this.Key = key;
        this._Action = action;
    }

    /// <summary>
    /// Initialize a new instance of the HotKeyEntryByKey class.
    /// </summary>
    /// <param name="context">The instance of <see cref="HotKeysContext"/> that is an owner of this hot key entry.</param>
    /// <param name="logger">The instance of ILogger.</param>
    /// <param name="modKeys">The combination of modifier keys flags.</param>
    /// <param name="key">The identifier of hotkey.</param>
    /// <param name="action">The callback action that will be invoked when user enter modKeys + key combination on the browser.</param>
    /// <param name="ownerOfAction">The instance of a Razor component that is an owner of the callback action method.</param>
    /// <param name="options">The options for a hotkey entry.</param>
    internal HotKeyEntryByKey(HotKeysContext context, ILogger logger, ModKey modKeys, Key key, Func<HotKeyEntryByKey, ValueTask> action, IHandleEvent? ownerOfAction, HotKeyOptions options)
        : base(context, logger, HotKeyMode.ByKey, typeof(ModKey), (int)modKeys, key.ToString(), ownerOfAction, options)
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
