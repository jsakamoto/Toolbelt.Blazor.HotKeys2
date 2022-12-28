namespace Toolbelt.Blazor.HotKeys2.Test;

internal class CodeComboTest
{
    [Test]
    public void Cast_Test()
    {
        static CodeCombo f(CodeCombo combo) { return combo; }
        var combo = f(Code.PageDown);
        combo.Modifiers.Is(ModCode.None);
        combo.Code.ToString().Is("PageDown");
        combo.ToString().Is("PageDown");
    }

    [Test]
    public void Operator_Plus_Test()
    {
        var combo1 = ModCode.Ctrl + Code.A;
        combo1.Modifiers.Is(ModCode.Ctrl);
        combo1.Code.ToString().Is("KeyA");
        combo1.ToString().Is("Ctrl+KeyA");

        var combo2 = ModCode.Ctrl + ModCode.Alt + Code.Delete;
        combo2.Modifiers.Is(ModCode.Ctrl + ModCode.Alt);
        combo2.Code.ToString().Is("Delete");
        combo2.ToString().Is("Ctrl+Alt+Delete");
    }
}
