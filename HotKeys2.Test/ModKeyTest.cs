namespace Toolbelt.Blazor.HotKeys2.Test;
internal class ModKeyTest
{
    [Test]
    public void SingleToString_Test()
    {
        ModKey.None.ToString().Is("None");
        ModKey.Ctrl.ToString().Is("Ctrl");
        ModKey.Alt.ToString().Is("Alt");
        ModKey.Meta.ToString().Is("Meta");
    }

    [Test]
    public void ComboToString_Test()
    {
        (ModKey.None + ModKey.Ctrl).ToString().Is("Ctrl");
        (ModKey.Ctrl + ModKey.Alt + ModKey.Meta).ToString().Is("Ctrl+Alt+Meta");
    }
}
