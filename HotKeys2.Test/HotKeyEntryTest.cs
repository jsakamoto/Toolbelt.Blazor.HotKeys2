namespace Toolbelt.Blazor.HotKeys2.Test;

public class HotKeyEntryTest
{
    [Test]
    public void Mode_for_ByKey_Test()
    {
        var hotKeyEntry = new HotKeyEntryByKey(null!, ModKey.None, Key.Question, _ => ValueTask.CompletedTask, null, new());
        hotKeyEntry.Mode.Is(HotKeyMode.ByKey);
    }

    [Test]
    public void Mode_for_ByCode_Test()
    {
        var hotKeyEntry = new HotKeyEntryByCode(null!, ModCode.Ctrl | ModCode.Alt, Code.F12, _ => ValueTask.CompletedTask, null, new());
        hotKeyEntry.Mode.Is(HotKeyMode.ByCode);
    }

    [Test]
    public void ToString_for_ByKey_Test()
    {
        var hotKeyEntry = new HotKeyEntryByKey(null!, ModKey.None, Key.Question, _ => ValueTask.CompletedTask, null, new() { Description = "Show help." });
        hotKeyEntry.ToString().Is("?: Show help.");
    }

    [Test]
    public void ToString_for_ByCode_Test()
    {
        var ctrl_alt_f12 = new HotKeyEntryByCode(null!, ModCode.Ctrl | ModCode.Alt, Code.F12, _ => ValueTask.CompletedTask, null, new() { Description = "Set the volume level to 10." });
        ctrl_alt_f12.ToString().Is("Ctrl + Alt + F12: Set the volume level to 10.");

        var meta_one = new HotKeyEntryByCode(null!, ModCode.Meta, Code.Num1, _ => ValueTask.CompletedTask, null, new() { Description = "Launch the notepad." });
        meta_one.ToString().Is("Meta + 1: Launch the notepad.");

        var u = new HotKeyEntryByCode(null!, ModCode.None, Code.U, _ => ValueTask.CompletedTask, null, new() { Description = "Increment counter." });
        u.ToString().Is("U: Increment counter.");
    }
}
