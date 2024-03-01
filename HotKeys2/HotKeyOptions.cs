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

    /// <summary>State data attached to a hotkey.</summary>
    public HotKeyEntryState? State { get; set; }
}
