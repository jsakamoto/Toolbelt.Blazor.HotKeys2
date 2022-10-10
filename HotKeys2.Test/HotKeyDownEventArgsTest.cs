namespace Toolbelt.Blazor.HotKeys2.Test;

public class HotKeyDownEventArgsTest
{
    [Test]
    public void SetPreventDefault_to_True_Throws_Exception_on_BlazorServer()
    {
        var args = new HotKeyDownEventArgs(ModCodes.None, "INPUT", "text", isWasm: false, key: "a", code: "KeyA");
        Assert.Throws<InvalidOperationException>(() =>
        {
            args.PreventDefault = true;
        });
    }

    [Test]
    public void SetPreventDefault_to_False_on_BlazorServer()
    {
        var args = new HotKeyDownEventArgs(ModCodes.None, "INPUT", "text", isWasm: false, key: "a", code: "KeyA");
        args.PreventDefault.IsFalse();
        args.PreventDefault = false;
        args.PreventDefault.IsFalse();
    }

    [Test]
    public void SetPreventDefault_Success_on_BlazorWebAssembly()
    {
        var args = new HotKeyDownEventArgs(ModCodes.None, "INPUT", "text", isWasm: true, key: "a", code: "KeyA");
        args.PreventDefault.IsFalse();
        args.PreventDefault = true;
        args.PreventDefault.IsTrue();
    }
}
