namespace Toolbelt.Blazor.HotKeys2;

public class HotKeyEntryState
{
    /// <summary>
    /// Notifies when the property values of this state object has changed.
    /// </summary>
    internal Action? _NotifyStateChanged;

    private bool _IsDisabled;

    /// <summary>
    /// Controls if the current hot key is disabled or not.
    /// </summary>
    public virtual bool IsDisabled
    {
        get => this._IsDisabled;
        set { if (this._IsDisabled != value) { this._IsDisabled = value; this._NotifyStateChanged?.Invoke(); } }
    }
}
