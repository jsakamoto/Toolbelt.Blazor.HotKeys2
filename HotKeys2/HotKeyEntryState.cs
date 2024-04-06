using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbelt.Blazor.HotKeys2;

public class HotKeyEntryState
{
    /// <summary>
    /// Controls if the current hot key is disabled or not.
    /// </summary>
    public virtual bool IsDisabled { get; set; }
}
