using Microsoft.Extensions.Logging.Abstractions;

namespace Toolbelt.Blazor.HotKeys2.Test;

public class HotKeysContextTest
{
    [Test]
    public void GetHotKey_NotFound_Test()
    {
        // Given
        using var hotkeysContext = new HotKeysContext(null!, NullLogger.Instance)
            .Add(Code.A, () => { })
            .Add(Key.F1, () => { }, description: "Show the help document.") // This entry should be retrieved even though the description is unmatched.
            .Add(ModCode.Shift, Code.A, () => { })
            .Add(ModKey.Ctrl | ModKey.Alt, Key.F1, () => { });

        // When/Then
        Assert.Throws<KeyNotFoundException>(() => hotkeysContext.GetHotKey(Key.F2));
        Assert.Throws<KeyNotFoundException>(() => hotkeysContext.GetHotKey(Code.B));
        Assert.Throws<KeyNotFoundException>(() => hotkeysContext.GetHotKey(ModKey.Alt, Key.F1));
        Assert.Throws<KeyNotFoundException>(() => hotkeysContext.GetHotKey(ModCode.Ctrl | ModCode.Shift, Code.A));
    }

    [Test]
    public void GetHotKey_by_Key_Test()
    {
        // Given
        using var hotkeysContext = new HotKeysContext(null!, NullLogger.Instance)
            .Add(Code.A, () => { })
            .Add(Key.F1, () => { }, description: "Show the help document.")
            .Add(ModCode.Shift, Code.A, () => { })
            .Add(ModKey.Ctrl | ModKey.Alt, Key.F1, () => { });

        // When
        var hotKey = hotkeysContext.GetHotKey(Key.F1);

        // Then
        string.Join("+", hotKey.ToStringKeys()).Is("F1");
        hotKey.Description.Is("Show the help document.");
    }

    [Test]
    public void GetHotKey_by_Code_and_Mod_Test()
    {
        // Given
        using var hotkeysContext = new HotKeysContext(null!, NullLogger.Instance)
            .Add(Code.A, () => { })
            .Add(Key.F1, () => { })
            .Add(ModCode.Shift, Code.A, () => { }, exclude: Exclude.None) // This entry should be retrieved even though the exclude flag is unmatched.
            .Add(ModKey.Ctrl | ModKey.Alt, Key.F1, () => { });

        // When
        var hotKey = hotkeysContext.GetHotKey(ModCode.Shift, Code.A);

        // Then
        string.Join("+", hotKey.ToStringKeys()).Is("Shift+A");
    }

    [Test]
    public void GetHotKey_by_Key_and_Exclude_Test()
    {
        // Given
        using var hotkeysContext = new HotKeysContext(null!, NullLogger.Instance)
            .Add(Code.A, () => { })
            .Add(ModKey.Meta, Key.F1, () => { }, new() { Exclude = Exclude.ContentEditable }) // This entry should be retrieved because the exclude flag matches.
            .Add(ModCode.Shift, Code.A, () => { })
            .Add(ModKey.Meta, Key.F1, () => { });

        // When
        var hotKey = hotkeysContext.GetHotKey(ModKey.Meta, Key.F1, exclude: Exclude.ContentEditable);

        // Then
        string.Join("+", hotKey.ToStringKeys()).Is("Meta+F1");
        hotKey.Exclude.Is(Exclude.ContentEditable);
        hotKey.ExcludeSelector.Is("");
    }

    [Test]
    public void GetHotKey_by_Code_and_ExcludeSelector_Test()
    {
        // Given
        using var hotkeysContext = new HotKeysContext(null!, NullLogger.Instance)
            .Add(Code.A, () => { })
            .Add(ModKey.Meta, Key.F1, () => { })
            .Add(Code.A, () => { }, new() { ExcludeSelector = "[data-no-hotkeys]" }) // This entry should be retrieved because the exclude selector matches.
            .Add(ModKey.Meta, Key.F1, () => { });

        // When
        var hotKey = hotkeysContext.GetHotKey(Code.A, excludeSelector: "[data-no-hotkeys]");

        // Then
        string.Join("+", hotKey.ToStringKeys()).Is("A");
        hotKey.Exclude.Is(Exclude.Default);
        hotKey.ExcludeSelector.Is("[data-no-hotkeys]");
    }

    [Test]
    public void GetHotKey_by_Key_but_Ambiguous_Exception_Test()
    {
        // Given
        using var hotkeysContext = new HotKeysContext(null!, NullLogger.Instance)
            .Add(ModCode.Shift, Code.A, () => { })
            .Add(Key.F1, () => { }, exclude: Exclude.ContentEditable)
            .Add(ModCode.Shift, Code.A, () => { })
            .Add(Key.F1, () => { }, exclude: Exclude.InputNonText);

        // When/Then
        Assert.Throws<ArgumentException>(() => hotkeysContext.GetHotKey(Key.F1));
    }

    [Test]
    public void GetHotKey_by_Code_but_Ambiguous_Exception_Test()
    {
        // Given
        using var hotkeysContext = new HotKeysContext(null!, NullLogger.Instance)
            .Add(ModCode.Shift, Code.A, () => { }, exclude: Exclude.ContentEditable)
            .Add(Key.F1, () => { })
            .Add(ModCode.Shift, Code.A, () => { }, exclude: Exclude.InputNonText)
            .Add(Key.F1, () => { });

        // When/Then
        Assert.Throws<ArgumentException>(() => hotkeysContext.GetHotKey(ModCode.Shift, Code.A));
    }

    [Test]
    public void Remove_NotFound_Test()
    {
        // Given
        using var hotkeysContext = new HotKeysContext(null!, NullLogger.Instance)
            .Add(Code.A, () => { })
            .Add(Key.F1, () => { }, description: "Show the help document.") // This entry should be retrieved even though the description is unmatched.
            .Add(ModCode.Shift, Code.A, () => { })
            .Add(ModKey.Ctrl | ModKey.Alt, Key.F1, () => { });

        // When/Then
        Assert.Throws<KeyNotFoundException>(() => hotkeysContext.Remove(Key.F2));
        Assert.Throws<KeyNotFoundException>(() => hotkeysContext.Remove(Code.B));
        Assert.Throws<KeyNotFoundException>(() => hotkeysContext.Remove(ModKey.Alt, Key.F1));
        Assert.Throws<KeyNotFoundException>(() => hotkeysContext.Remove(ModCode.Ctrl | ModCode.Shift, Code.A));
    }

    [Test]
    public void Remove_by_Key_Test()
    {
        // Given
        using var hotkeysContext = new HotKeysContext(null!, NullLogger.Instance)
            .Add(Code.A, () => { })
            .Add(Key.F1, () => { }, description: "Show the help document.") // This entry should be removed even though the description is unmatched.
            .Add(ModCode.Shift, Code.A, () => { })
            .Add(ModKey.Ctrl | ModKey.Alt, Key.F1, () => { });

        // When
        hotkeysContext.Remove(Key.F1);

        // Then
        hotkeysContext.HotKeyEntries
            .Select(hotkey => string.Join("+", hotkey.ToStringKeys()))
            .Is("A", "Shift+A", "Ctrl+Alt+F1");
    }

    [Test]
    public void Remove_by_Code_and_Mod_Test()
    {
        // Given
        using var hotkeysContext = new HotKeysContext(null!, NullLogger.Instance)
            .Add(Code.A, () => { })
            .Add(Key.F1, () => { })
            .Add(ModCode.Shift, Code.A, () => { }, exclude: Exclude.None) // This entry should be removed even though the exclude flag is unmatched.
            .Add(ModKey.Ctrl | ModKey.Alt, Key.F1, () => { });

        // When
        hotkeysContext.Remove(ModCode.Shift, Code.A);

        // Then
        hotkeysContext.HotKeyEntries
            .Select(hotkey => string.Join("+", hotkey.ToStringKeys()))
            .Is("A", "F1", "Ctrl+Alt+F1");
    }

    [Test]
    public void Remove_by_Key_and_Exclude_Test()
    {
        // Given
        using var hotkeysContext = new HotKeysContext(null!, NullLogger.Instance)
            .Add(Code.A, () => { })
            .Add(ModKey.Meta, Key.F1, () => { }, new() { Exclude = Exclude.ContentEditable })
            .Add(ModCode.Shift, Code.A, () => { })
            .Add(ModKey.Meta, Key.F1, () => { });

        // When
        hotkeysContext.Remove(ModKey.Meta, Key.F1, exclude: Exclude.ContentEditable);

        // Then
        hotkeysContext.HotKeyEntries
            .Select(hotkey => string.Join("+", hotkey.ToStringKeys()))
            .Is("A", "Shift+A", "Meta+F1");
    }

    [Test]
    public void Remove_by_Code_and_ExcludeSelector_Test()
    {
        // Given
        using var hotkeysContext = new HotKeysContext(null!, NullLogger.Instance)
            .Add(Code.A, () => { })
            .Add(ModKey.Meta, Key.F1, () => { })
            .Add(Code.A, () => { }, new() { ExcludeSelector = "[data-no-hotkeys]" })
            .Add(ModKey.Meta, Key.F1, () => { });

        // When
        hotkeysContext.Remove(Code.A, excludeSelector: "[data-no-hotkeys]");

        // Then
        hotkeysContext.HotKeyEntries
            .Select(hotkey => string.Join("+", hotkey.ToStringKeys()))
            .Is("A", "Meta+F1", "Meta+F1");
    }

    [Test]
    public void Remove_by_Key_but_Ambiguous_Exception_Test()
    {
        // Given
        using var hotkeysContext = new HotKeysContext(null!, NullLogger.Instance)
            .Add(ModCode.Shift, Code.A, () => { })
            .Add(Key.F1, () => { }, exclude: Exclude.ContentEditable)
            .Add(ModCode.Shift, Code.A, () => { })
            .Add(Key.F1, () => { }, exclude: Exclude.InputNonText);

        // When
        Assert.Throws<ArgumentException>(() => hotkeysContext.Remove(Key.F1));

        // Then
        hotkeysContext.HotKeyEntries
            .Select(hotkey => string.Join("+", hotkey.ToStringKeys()))
            .Is("Shift+A", "F1", "Shift+A", "F1");
    }

    [Test]
    public void Remove_by_Code_but_Ambiguous_Exception_Test()
    {
        // Given
        using var hotkeysContext = new HotKeysContext(null!, NullLogger.Instance)
            .Add(ModCode.Shift, Code.A, () => { }, exclude: Exclude.ContentEditable)
            .Add(Key.F1, () => { })
            .Add(ModCode.Shift, Code.A, () => { }, exclude: Exclude.InputNonText)
            .Add(Key.F1, () => { });

        // When
        Assert.Throws<ArgumentException>(() => hotkeysContext.Remove(ModCode.Shift, Code.A));

        // Then
        hotkeysContext.HotKeyEntries
            .Select(hotkey => string.Join("+", hotkey.ToStringKeys()))
            .Is("Shift+A", "F1", "Shift+A", "F1");
    }

    [Test]
    public void Remove_by_Filter_Test()
    {
        // Given
        using var hotkeysContext = new HotKeysContext(null!, NullLogger.Instance)
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
        hotkeysContext.HotKeyEntries
            .Select(hotkey => string.Join("+", hotkey.ToStringKeys()))
            .Is("F1", "F1");
    }
}
