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

    private bool _Disabled;

    /// <summary>
    /// Controls if the current hot key is disabled or not.
    /// </summary>
    public virtual bool Disabled
    {
        get => this._Disabled;
        set { if (this._Disabled != value) { this._Disabled = value; this._NotifyStateChanged?.Invoke(); } }
    }
}
