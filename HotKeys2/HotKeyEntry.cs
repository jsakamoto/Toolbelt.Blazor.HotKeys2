using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

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
    /// Get Additional CSS selector for HTML elements that will not allow hotkey to work.
    /// </summary>
    public string ExcludeSelector { get; }

    /// <summary>
    /// Get the description of the meaning of this hot key entry.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Get the state data attached to this hot key entry.
    /// </summary>
    public HotKeyEntryState? State { get; }

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
    /// Get the instance of a Razor component that is an owner of the callback action method.
    /// </summary>
    private IHandleEvent? _OwnerComponent;

    /// <summary>
    /// Get the instance of a <see cref="HotKeysContext"/> that is an owner of this hot key entry.
    /// </summary>
    private HotKeysContext _HotKeysContext;

    private readonly ILogger? _Logger;

    /// <summary>
    /// Initialize a new instance of the HotKeyEntry class.
    /// </summary>
    /// <param name="context">The instance of <see cref="HotKeysContext"/> that is an owner of this hot key entry.</param>
    /// <param name="logger">The instance of <see cref="ILogger"/> that is used to log the error message.</param>
    /// <param name="mode">The mode that how to identificate the hot key.</param>
    /// <param name="typeOfModifiers"></param>
    /// <param name="modifiers">The combination of modifier flags</param>
    /// <param name="keyEntry">The key or code of the hot key</param>
    /// <param name="ownerOfAction">The instance of a Razor component that is an owner of the callback action method.</param>
    /// <param name="options">The options for this hotkey entry.</param>
    [DynamicDependency(nameof(InvokeAction), typeof(HotKeyEntry))]
    internal HotKeyEntry(HotKeysContext context, ILogger? logger, HotKeyMode mode, Type typeOfModifiers, int modifiers, string keyEntry, IHandleEvent? ownerOfAction, HotKeyOptions options)
    {
        this._HotKeysContext = context;
        this._Logger = logger;
        this.Mode = mode;
        this._Modifiers = modifiers;
        this._TypeOfModifiers = typeOfModifiers;
        this._KeyEntry = keyEntry;
        this._OwnerComponent = ownerOfAction;
        this.Description = options.Description;
        this.Exclude = options.Exclude;
        this.ExcludeSelector = options.ExcludeSelector;
        this.State = options.State;
        this._ObjectRef = DotNetObjectReference.Create(this);

        this.State?.SetHotKeyEntry(this);
    }

    protected abstract void InvokeCallbackAction();

    protected void CommonProcess(Func<ValueTask> action)
    {
        var task = action();
        var awaiter = task.GetAwaiter();
        awaiter.OnCompleted(() =>
        {
            if (!task.IsCompletedSuccessfully) try { awaiter.GetResult(); } catch (Exception ex) { this._Logger?.LogError(ex, ex.Message); }
            else if (this._OwnerComponent != null)
            {
                var task = this._OwnerComponent.HandleEventAsync(EventCallbackWorkItem.Empty, null);
                var awaiter = task.GetAwaiter();
                awaiter.OnCompleted(() =>
                {
                    if (task.IsCompletedSuccessfully) return;
                    try { awaiter.GetResult(); } catch (Exception ex) { this._Logger?.LogError(ex, ex.Message); }
                });
            }
        });
    }

    internal void UpdateDisabledState() => this._HotKeysContext?.UpdateHotKeyDisabledState(this);

    [JSInvokable(nameof(InvokeAction)), EditorBrowsable(EditorBrowsableState.Never)]
    public void InvokeAction()
    {
        if (!(this.State?.IsDisabled ?? false))
        {
            this.InvokeCallbackAction();
        }
    }

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
        var keyComboText = string.Join(" + ", this.ToStringKeys());
        return string.Format(format, keyComboText, this.Description);
    }

    /// <summary>
    /// Returns an array of String formatted keys.
    /// </summary>
    public string[] ToStringKeys()
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

        var keyDisplayName =
            this._KeyEntry.StartsWith("Key") ? this._KeyEntry.Substring(3) :
            this._KeyEntry.StartsWith("Digit") ? this._KeyEntry.Substring(5) :
            this._KeyEntry == "Control" ? "Ctrl" :
            this._KeyEntry;
        keyCombo.Add(keyDisplayName);

        return keyCombo.ToArray();
    }

    public void Dispose()
    {
        this.Id = -1;
        this._ObjectRef.Dispose();
    }
}
