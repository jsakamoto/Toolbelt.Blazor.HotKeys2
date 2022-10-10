using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.JSInterop;
using static System.ComponentModel.EditorBrowsableState;

namespace Toolbelt.Blazor.HotKeys2;

/// <summary>
/// Association of key combination and callback action.
/// </summary>
public abstract class HotKeyEntry : IDisposable
{
    /// <summary>
    /// Get the mode that how to identificate the hot key.
    /// </summary>
    public HotKeyMode Mode { get; }

    /// <summary>
    /// Get the combination of HTML element flags that will be not allowed hotkey works.
    /// </summary>
    public Exclude Exclude { get; }

    /// <summary>
    /// Get the description of the meaning of this hot key entry.
    /// </summary>
    public string? Description { get; }

    internal int Id = -1;

    internal readonly DotNetObjectReference<HotKeyEntry> _ObjectRef;

    /// <summary>
    /// Get the combination of modifier flags.
    /// </summary>
    internal readonly int _Modifiers;

    internal readonly Type _TypeOfModifiers;

    /// <summary>
    /// Get the key or code of the hot key.
    /// </summary>
    internal readonly string _KeyEntry;

    /// <summary>
    /// Initialize a new instance of the HotKeyEntry class.
    /// </summary>
    /// <param name="mode">The mode that how to identificate the hot key.</param>
    /// <param name="typeOfModifiers"></param>
    /// <param name="modifiers">The combination of modifier flags</param>
    /// <param name="keyEntry">The key or code of the hot key</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <param name="exclude">The combination of HTML element flags that will be not allowed hotkey works.</param>
    [DynamicDependency(nameof(InvokeAction), typeof(HotKeyEntry))]
    internal HotKeyEntry(HotKeyMode mode, Type typeOfModifiers, int modifiers, string keyEntry, Exclude exclude, string? description)
    {
        this.Mode = mode;
        this._Modifiers = modifiers;
        this._TypeOfModifiers = typeOfModifiers;
        this._KeyEntry = keyEntry;
        this.Exclude = exclude;
        this.Description = description;
        this._ObjectRef = DotNetObjectReference.Create(this);
    }

    protected abstract void InvokeCallbackAction();

    [JSInvokable(nameof(InvokeAction)), EditorBrowsable(Never), RequiresUnreferencedCode("This is JSInvokable method")]
    public void InvokeAction() => this.InvokeCallbackAction();

    /// <summary>
    /// Returns a String that combined key combination and description of this entry, like "Ctrl+A: Select All."
    /// </summary>
    public override string ToString() => this.ToString("{0}: {1}");

    /// <summary>
    /// Returns a String formatted with specified format string.
    /// </summary>
    /// <param name="format">{0} will be replaced with key combination text, and {1} will be replaced with description of this hotkey entry object.</param>
    public string ToString(string format)
    {
        var keyCombo = new List<string>();
        if (this._Modifiers != 0)
        {
            foreach (var modFlag in Enum.GetValues(this._TypeOfModifiers))
            {
                if ((this._Modifiers & (int)modFlag) == 0) continue;
                keyCombo.Add(Enum.GetName(this._TypeOfModifiers, modFlag) ?? ("" + modFlag));
            }
        }
        keyCombo.Add(this._KeyEntry);
        var keyComboText = string.Join(" + ", keyCombo);
        return string.Format(format, keyComboText, this.Description);
    }

    public void Dispose()
    {
        this.Id = -1;
        this._ObjectRef.Dispose();
    }
}
