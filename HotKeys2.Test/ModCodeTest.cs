namespace Toolbelt.Blazor.HotKeys2.Test;
internal class ModCodeTest
{
    [Test]
    public void SingleToString_Test()
    {
        ModCode.None.ToString().Is("None");
        ModCode.Shift.ToString().Is("Shift");
        ModCode.Ctrl.ToString().Is("Ctrl");
        ModCode.Alt.ToString().Is("Alt");
        ModCode.Meta.ToString().Is("Meta");
    }

    [Test]
    public void ComboToString_Test()
    {
        (ModCode.None + ModCode.Shift).ToString().Is("Shift");
        (ModCode.Ctrl + ModCode.Alt + ModCode.Meta).ToString().Is("Ctrl+Alt+Meta");
    }
}
