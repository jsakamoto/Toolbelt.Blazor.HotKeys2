using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;

namespace Toolbelt.Blazor.HotKeys2.Test;

public class HotKeysContextTest
{
    [Test]
    public void Remove_by_Key_Test()
    {
        // Given
        using var hotkeysContext = new HotKeysContext(Task.FromResult(default(IJSObjectReference)!), NullLogger.Instance)
            .Add(Code.A, () => { })
            .Add(Key.F1, () => { }, description: "Show the help document.") // This entry should be removed even though the description is unmatched.
            .Add(ModCode.Shift, Code.A, () => { })
            .Add(ModKey.Ctrl | ModKey.Alt, Key.F1, () => { });

        // When
        hotkeysContext.Remove(Key.F1);

        // Then
        hotkeysContext.Keys
            .Select(hotkey => string.Join("+", hotkey.ToStringKeys()))
            .Is("A", "Shift+A", "Ctrl+Alt+F1");
    }

    [Test]
    public void Remove_by_Code_and_Mod_Test()
    {
        // Given
        using var hotkeysContext = new HotKeysContext(Task.FromResult(default(IJSObjectReference)!), NullLogger.Instance)
            .Add(Code.A, () => { })
            .Add(Key.F1, () => { })
            .Add(ModCode.Shift, Code.A, () => { }, exclude: Exclude.None) // This entry should be removed even though the exclude flag is unmatched.
            .Add(ModKey.Ctrl | ModKey.Alt, Key.F1, () => { });

        // When
        hotkeysContext.Remove(ModCode.Shift, Code.A);

        // Then
        hotkeysContext.Keys
            .Select(hotkey => string.Join("+", hotkey.ToStringKeys()))
            .Is("A", "F1", "Ctrl+Alt+F1");
    }

    [Test]
    public void Remove_by_Key_and_Exclude_Test()
    {
        // Given
        using var hotkeysContext = new HotKeysContext(Task.FromResult(default(IJSObjectReference)!), NullLogger.Instance)
            .Add(Code.A, () => { })
            .Add(ModKey.Meta, Key.F1, () => { }, new() { Exclude = Exclude.ContentEditable })
            .Add(ModCode.Shift, Code.A, () => { })
            .Add(ModKey.Meta, Key.F1, () => { });

        // When
        hotkeysContext.Remove(ModKey.Meta, Key.F1, exclude: Exclude.ContentEditable);

        // Then
        hotkeysContext.Keys
            .Select(hotkey => string.Join("+", hotkey.ToStringKeys()))
            .Is("A", "Shift+A", "Meta+F1");
    }

    [Test]
    public void Remove_by_Code_and_ExcludeSelector_Test()
    {
        // Given
        using var hotkeysContext = new HotKeysContext(Task.FromResult(default(IJSObjectReference)!), NullLogger.Instance)
            .Add(Code.A, () => { })
            .Add(ModKey.Meta, Key.F1, () => { })
            .Add(Code.A, () => { }, new() { ExcludeSelector = "[data-no-hotkeys]" })
            .Add(ModKey.Meta, Key.F1, () => { });

        // When
        hotkeysContext.Remove(Code.A, excludeSelector: "[data-no-hotkeys]");

        // Then
        hotkeysContext.Keys
            .Select(hotkey => string.Join("+", hotkey.ToStringKeys()))
            .Is("A", "Meta+F1", "Meta+F1");
    }

    [Test]
    public void Remove_by_Key_but_Ambiguous_Exception_Test()
    {
        // Given
        using var hotkeysContext = new HotKeysContext(Task.FromResult(default(IJSObjectReference)!), NullLogger.Instance)
            .Add(ModCode.Shift, Code.A, () => { })
            .Add(Key.F1, () => { }, exclude: Exclude.ContentEditable)
            .Add(ModCode.Shift, Code.A, () => { })
            .Add(Key.F1, () => { }, exclude: Exclude.InputNonText);

        // When
        Assert.Throws<ArgumentException>(() => hotkeysContext.Remove(Key.F1));

        // Then
        hotkeysContext.Keys
            .Select(hotkey => string.Join("+", hotkey.ToStringKeys()))
            .Is("Shift+A", "F1", "Shift+A", "F1");
    }

    [Test]
    public void Remove_by_Code_but_Ambiguous_Exception_Test()
    {
        // Given
        using var hotkeysContext = new HotKeysContext(Task.FromResult(default(IJSObjectReference)!), NullLogger.Instance)
            .Add(ModCode.Shift, Code.A, () => { }, exclude: Exclude.ContentEditable)
            .Add(Key.F1, () => { })
            .Add(ModCode.Shift, Code.A, () => { }, exclude: Exclude.InputNonText)
            .Add(Key.F1, () => { });

        // When
        Assert.Throws<ArgumentException>(() => hotkeysContext.Remove(ModCode.Shift, Code.A));

        // Then
        hotkeysContext.Keys
            .Select(hotkey => string.Join("+", hotkey.ToStringKeys()))
            .Is("Shift+A", "F1", "Shift+A", "F1");
    }

    [Test]
    public void Remove_by_Filter_Test()
    {
        // Given
        using var hotkeysContext = new HotKeysContext(Task.FromResult(default(IJSObjectReference)!), NullLogger.Instance)
            .Add(ModCode.Shift, Code.A, () => { }, exclude: Exclude.ContentEditable)
            .Add(Key.F1, () => { })
            .Add(ModCode.Shift, Code.A, () => { }, exclude: Exclude.InputNonText)
            .Add(Key.F1, () => { });

        // When
        hotkeysContext.Remove(entries =>
        {
            return entries.Where(e => e is HotKeyEntryByCode codeEntry && codeEntry.Code == Code.A);
        });

        // Then
        hotkeysContext.Keys
            .Select(hotkey => string.Join("+", hotkey.ToStringKeys()))
            .Is("F1", "F1");
    }
}
