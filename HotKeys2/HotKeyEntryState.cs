namespace Toolbelt.Blazor.HotKeys2;

/// <summary>
/// Represents the state of a hot key entry.
/// </summary>
public class HotKeyEntryState
{
    /// <summary>
    /// Notifies when the property values of this state object has changed.
    /// </summary>
    internal Action? _NotifyStateChanged;

    private bool _Disabled = false;

    /// <summary>
    /// Controls if the current hot key is disabled or not. The default value is false.
    /// </summary>
    public virtual bool Disabled
    {
        get => this._Disabled;
        set { if (this._Disabled != value) { this._Disabled = value; this._NotifyStateChanged?.Invoke(); } }
    }

    private bool _PreventDefault = true;

    /// <summary>
    /// Controls if the default action of the keyboard event should be prevented when this hot key is triggered. The default value is true.
    /// </summary>
    public virtual bool PreventDefault
    {
        get => this._PreventDefault;
        set { if (this._PreventDefault != value) { this._PreventDefault = value; this._NotifyStateChanged?.Invoke(); } }
    }
}
