using System.ComponentModel;

namespace Toolbelt.Blazor.HotKeys2;

/// <summary>
/// The options for a hotkey entry.
/// </summary>
public class HotKeyOptions
{
    /// <summary>The description of the meaning of this hot key entry.</summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// The combination of HTML element flags that will be not allowed hotkey works. The default value is <see cref="Exclude.Default"/>.
    /// </summary>
    public Exclude Exclude { get; set; } = Exclude.Default;

    /// <summary>Additional CSS selector for HTML elements that will not allow hotkey to work.</summary>
    public string ExcludeSelector { get; set; } = "";

    /// <summary>
    /// Controls if the current hot key is disabled or not. The default value is false.
    /// </summary>
    public bool Disabled
    {
#pragma warning disable CS0618 // Type or member is obsolete
        get => this.State.Disabled;
        set => this.State.Disabled = value;
#pragma warning restore CS0618 // Type or member is obsolete
    }

    /// <summary>
    /// Controls if the default action of the keyboard event should be prevented when this hot key is triggered. The default value is true.
    /// </summary>
    public bool PreventDefault
    {
#pragma warning disable CS0618 // Type or member is obsolete
        get => this.State.PreventDefault;
        set => this.State.PreventDefault = value;
#pragma warning restore CS0618 // Type or member is obsolete
    }

    /// <summary>State data attached to a hotkey.</summary>
    [Obsolete("Use the 'Disabled' and 'PreventDefault' properties of HotKeyOptions directly instead."), EditorBrowsable(EditorBrowsableState.Never)]
    public HotKeyEntryState State { get; set; } = new HotKeyEntryState();
}
