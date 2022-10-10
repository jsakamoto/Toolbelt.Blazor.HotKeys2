using System;

namespace Toolbelt.Blazor.HotKeys2;

/// <summary>
/// The flags of modifier keys.
/// </summary>
[Flags]
public enum ModKeys
{
    None = 0,
    //Shift = 0b0001,
    Ctrl = 0b0010,
    Alt = 0b0100,
    Meta = 0b1000
}
