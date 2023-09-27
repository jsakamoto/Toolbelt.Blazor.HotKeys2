namespace Toolbelt.Blazor.HotKeys2.Test;

public class HotKeyEntryTest
{
    [Test]
    public void Mode_for_ByKey_Test()
    {
        var hotKeyEntry = new HotKeyEntryByKey(ModKey.None, Key.Question, Exclude.None, "", "Show help.", _ => ValueTask.CompletedTask);
        hotKeyEntry.Mode.Is(HotKeyMode.ByKey);
    }

    [Test]
    public void Mode_for_ByCode_Test()
    {
        var hotKeyEntry = new HotKeyEntryByCode(ModCode.Ctrl | ModCode.Alt, Code.F12, Exclude.None, "", "Set the volume level to 10.", _ => ValueTask.CompletedTask);
        hotKeyEntry.Mode.Is(HotKeyMode.ByCode);
    }

    [Test]
    public void ToString_for_ByKey_Test()
    {
        var hotKeyEntry = new HotKeyEntryByKey(ModKey.None, Key.Question, Exclude.None, "", "Show help.", _ => ValueTask.CompletedTask);
        hotKeyEntry.ToString().Is("?: Show help.");
    }

    [Test]
    public void ToString_for_ByCode_Test()
    {
        var ctrl_alt_f12 = new HotKeyEntryByCode(ModCode.Ctrl | ModCode.Alt, Code.F12, Exclude.None, "", "Set the volume level to 10.", _ => ValueTask.CompletedTask);
        ctrl_alt_f12.ToString().Is("Ctrl + Alt + F12: Set the volume level to 10.");

        var meta_one = new HotKeyEntryByCode(ModCode.Meta, Code.Num1, Exclude.None, "", "Launch the notepad.", _ => ValueTask.CompletedTask);
        meta_one.ToString().Is("Meta + 1: Launch the notepad.");

        var u = new HotKeyEntryByCode(ModCode.None, Code.U, Exclude.None, "", "Increment counter.", _ => ValueTask.CompletedTask);
        u.ToString().Is("U: Increment counter.");
    }
}
