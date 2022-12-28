namespace Toolbelt.Blazor.HotKeys2.Test;
internal class KeyComboTest
{
    [Test]
    public void Cast_Test()
    {
        static KeyCombo f(KeyCombo combo) { return combo; }
        var combo = f(Key.PageUp);
        combo.Modifiers.Is(ModKey.None);
        combo.Key.ToString().Is("PageUp");
        combo.ToString().Is("PageUp");
    }

    [Test]
    public void Operator_Plus_Test()
    {
        var combo1 = ModKey.Ctrl + Key.a;
        combo1.Modifiers.Is(ModKey.Ctrl);
        combo1.Key.ToString().Is("a");
        combo1.ToString().Is("Ctrl+a");

        var combo2 = ModKey.Ctrl + ModKey.Alt + Key.Delete;
        combo2.Modifiers.Is(ModKey.Ctrl + ModKey.Alt);
        combo2.Key.ToString().Is("Delete");
        combo2.ToString().Is("Ctrl+Alt+Delete");
    }
}
