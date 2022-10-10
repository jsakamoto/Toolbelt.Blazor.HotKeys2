using System;

namespace Toolbelt.Blazor.HotKeys2;

/// <summary>
/// The flags of HTML element that will be not allowed hotkey works.
/// </summary>
[Flags]
public enum Exclude
{
    None = 0,
    InputText = 0b0001,
    InputNonText = 0b0010,
    TextArea = 0b0100,
    ContentEditable = 0b1000,
    Default = InputText | InputNonText | TextArea
}
