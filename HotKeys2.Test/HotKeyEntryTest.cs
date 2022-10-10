namespace Toolbelt.Blazor.HotKeys2.Test;

public class HotKeyEntryTest
{
    [Test]
    public void Mode_for_ByKey_Test()
    {
        var hotKeyEntry = new HotKeyEntryByKey(ModKeys.None, Key.Question, Exclude.None, "Show help.", _ => ValueTask.CompletedTask);
        hotKeyEntry.Mode.Is(HotKeyMode.ByKey);
    }

    [Test]
    public void Mode_for_ByCode_Test()
    {
        var hotKeyEntry = new HotKeyEntryByCode(ModCodes.Ctrl | ModCodes.Alt, Code.F12, Exclude.None, "Set the volume level to 10.", _ => ValueTask.CompletedTask);
        hotKeyEntry.Mode.Is(HotKeyMode.ByCode);
    }

    [Test]
    public void ToString_for_ByKey_Test()
    {
        var hotKeyEntry = new HotKeyEntryByKey(ModKeys.None, Key.Question, Exclude.None, "Show help.", _ => ValueTask.CompletedTask);
        hotKeyEntry.ToString().Is("?: Show help.");
    }

    [Test]
    public void ToString_for_ByCode_Test()
    {
        var hotKeyEntry = new HotKeyEntryByCode(ModCodes.Ctrl | ModCodes.Alt, Code.F12, Exclude.None, "Set the volume level to 10.", _ => ValueTask.CompletedTask);
        hotKeyEntry.ToString().Is("Ctrl + Alt + F12: Set the volume level to 10.");
    }
}
