using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbelt.Blazor.HotKeys2;

public class HotKeyEntryState
{
    private HotKeyEntry? hotKeyEntry;
    private bool isDisabled;

    /// <summary>
    /// Controls if the current hot key is disabled or not.
    /// </summary>
    public virtual bool IsDisabled
    {
        get => this.isDisabled;
        set
        {
            this.isDisabled = value;
            this.hotKeyEntry?.UpdateDisabledState();
        }
    }

    internal void SetHotKeyEntry(HotKeyEntry hotKeyEntry) =>
        this.hotKeyEntry = hotKeyEntry;
}
