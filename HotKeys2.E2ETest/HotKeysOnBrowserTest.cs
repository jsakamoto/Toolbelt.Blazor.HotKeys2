using Toolbelt.Blazor.HotKeys2.E2ETest.Internals;

namespace Toolbelt.Blazor.HotKeys2.E2ETest;

public class HotKeysOnBrowserTest
{
    public static IEnumerable<HostingModel> AllHostingModels { get; } = [
        HostingModel.Wasm80,
        HostingModel.Wasm90,
        HostingModel.Wasm100,
        HostingModel.Server80,
        HostingModel.Server90,
        HostingModel.Server100,
    ];

    public static IEnumerable<HostingModel> WasmHostingModels { get; } = [
        HostingModel.Wasm80,
        HostingModel.Wasm90,
        HostingModel.Wasm100,
    ];

    [Test]
    [TestCaseSource(typeof(HotKeysOnBrowserTest), nameof(AllHostingModels))]
    public async Task HotKey_on_Body_Test(HostingModel hostingModel)
    {
        var context = TestContext.Instance;
        var host = await context.StartHostAsync(hostingModel);

        // Navigate to Home
        var page = await context.GetPageAsync();
        await page.GotoAndWaitForReadyAsync(host.GetUrl());
        await page.AssertUrlIsAsync(host.GetUrl("/"));

        // Go to the "Fetch Data" page by entering "F" key.
        await page.Keyboard.DownAsync("f");
        await page.Keyboard.UpAsync("f");
        await page.AssertUrlIsAsync(host.GetUrl("/fetchdata"));

        // Go to the "Home" page by entering "H" key.
        await page.Keyboard.DownAsync("h");
        await page.Keyboard.UpAsync("h");
        await page.AssertUrlIsAsync(host.GetUrl("/"));

        // Go to the "Counter" page by entering "H" key.
        await page.Keyboard.DownAsync("c");
        await page.Keyboard.UpAsync("c");
        await page.AssertUrlIsAsync(host.GetUrl("/counter"));

        // Increment the counter by entering "U" key.
        var counter = page.Locator("h1+p");

        await page.AssertEqualsAsync(_ => counter.TextContentAsync(), "Current count: 0");
        await page.Keyboard.DownAsync("u");
        await page.Keyboard.UpAsync("u");
        await page.AssertEqualsAsync(_ => counter.TextContentAsync(), "Current count: 1");
        await page.Keyboard.DownAsync("u");
        await page.Keyboard.UpAsync("u");
        await page.AssertEqualsAsync(_ => counter.TextContentAsync(), "Current count: 2");

        // Show and hide the cheatSheetElement by entering "?" and ESC key.
        var cheatSheetElement = page.Locator(".popup-container");
        (await cheatSheetElement.IsVisibleAsync()).IsFalse();

        await page.Keyboard.DownAsync("Shift"); // Enter "?"
        await page.Keyboard.DownAsync("Slash");
        await page.Keyboard.UpAsync("Slash");
        await page.Keyboard.UpAsync("Shift");
        await page.AssertEqualsAsync(_ => cheatSheetElement.IsVisibleAsync(), true);

        await page.Keyboard.DownAsync("Escape");
        await page.Keyboard.UpAsync("Escape");
        await page.AssertEqualsAsync(_ => cheatSheetElement.IsVisibleAsync(), false);

        // Double hit of Ctrl key makes jump to the "Home" page.
        await page.AssertUrlIsAsync(host.GetUrl("/counter"));
        await page.Keyboard.DownAsync("Control");
        await page.Keyboard.UpAsync("Control");
        await page.Keyboard.DownAsync("Control");
        await page.Keyboard.UpAsync("Control");
        await page.AssertUrlIsAsync(host.GetUrl("/"));
    }

    [Test]
    [TestCaseSource(typeof(HotKeysOnBrowserTest), nameof(AllHostingModels))]
    public async Task AllowIn_Input_Test(HostingModel hostingModel)
    {
        var context = TestContext.Instance;
        var host = await context.StartHostAsync(hostingModel);

        // Navigate to the "Test OnKeyDown" page,
        var page = await context.GetPageAsync();
        await page.GotoAndWaitForReadyAsync(host.GetUrl("/test/onkeydown"));
        await page.AssertUrlIsAsync(host.GetUrl("/test/onkeydown"));

        // and shows cheat sheet.
        var cheatSheetElement = page.Locator(".popup-container");
        await page.Keyboard.DownAsync("Shift"); // Enter "?"
        await page.Keyboard.DownAsync("Slash");
        await page.Keyboard.UpAsync("Slash");
        await page.Keyboard.UpAsync("Shift");
        await page.AssertEqualsAsync(_ => cheatSheetElement.IsVisibleAsync(), true);

        // Entering "C", "F", "Shift+H" in an input element has no effect.
        var inputElement = page.Locator(".hot-keys-cheat-sheet input[type=text]");
        await inputElement.FocusAsync();
        await page.Keyboard.DownAsync("c");
        await page.Keyboard.UpAsync("c");
        await page.Keyboard.DownAsync("f");
        await page.Keyboard.UpAsync("f");
        await page.Keyboard.DownAsync("Shift");
        await page.Keyboard.DownAsync("H");
        await page.Keyboard.UpAsync("H");
        await page.Keyboard.UpAsync("Shift");
        (await inputElement.InputValueAsync()).Is("cfH");
        await page.AssertUrlIsAsync(host.GetUrl("/test/onkeydown"));

        // Entering "H" key causes jumping to the "Home" page even though it happened in an input element.
        await inputElement.FocusAsync();
        await page.Keyboard.DownAsync("h");
        await page.Keyboard.UpAsync("h");
        await page.AssertUrlIsAsync(host.GetUrl("/"));
        (await inputElement.InputValueAsync()).Is("cfH"); // "H" key was captured, so input text has no change.
    }

    [Test]
    [TestCaseSource(typeof(HotKeysOnBrowserTest), nameof(AllHostingModels))]
    public async Task AllowIn_NonTextInput_Test(HostingModel hostingModel)
    {
        var context = TestContext.Instance;
        var host = await context.StartHostAsync(hostingModel);

        // Navigate to the "Test All Keys" page,
        var page = await context.GetPageAsync();
        await page.GotoAndWaitForReadyAsync(host.GetUrl("/test/onkeydown"));
        await page.AssertUrlIsAsync(host.GetUrl("/test/onkeydown"));

        // and shows cheat sheet.
        var cheatSheetElement = page.Locator(".popup-container");
        await page.Keyboard.DownAsync("Shift"); // Enter "?"
        await page.Keyboard.DownAsync("Slash");
        await page.Keyboard.UpAsync("Slash");
        await page.Keyboard.UpAsync("Shift");
        await page.AssertEqualsAsync(_ => cheatSheetElement.IsVisibleAsync(), true);

        // Entering "F" key in an input element has no effect.
        var inputElement = page.Locator(".hot-keys-cheat-sheet input[type=checkbox]");
        await inputElement.FocusAsync();
        await page.Keyboard.DownAsync("f");
        await page.Keyboard.UpAsync("f");
        await page.AssertUrlIsAsync(host.GetUrl("/test/onkeydown"));

        // Entering "C" key causes jumping to the "Counter" page even though it happened in an input element.
        await inputElement.FocusAsync();
        await page.Keyboard.DownAsync("c");
        await page.Keyboard.UpAsync("c");
        await page.AssertUrlIsAsync(host.GetUrl("/counter"));

        // Entering "H" key causes jumping to the "Counter" page even though it happened in an input element.
        await inputElement.FocusAsync();
        await page.Keyboard.DownAsync("h");
        await page.Keyboard.UpAsync("h");
        await page.AssertUrlIsAsync(host.GetUrl("/"));
    }

    [Test]
    [TestCaseSource(typeof(HotKeysOnBrowserTest), nameof(WasmHostingModels))]
    public async Task KeyDownEvent_PreventDefault_Test(HostingModel hostingModel)
    {
        var context = TestContext.Instance;
        var host = await context.StartHostAsync(hostingModel);

        // Navigate to the "Counter" page,
        var page = await context.GetPageAsync();
        await page.GotoAndWaitForReadyAsync(host.GetUrl("/counter"));
        await page.AssertUrlIsAsync(host.GetUrl("/counter"));

        // Ctrl+A does not no effect at the "Counter" page because it is prevented default.

        await page.AssertEqualsAsync(_ => page.IsContentsSelectedAsync(), false);
        await page.Keyboard.DownAsync("Control");
        await page.Keyboard.DownAsync("a");
        await page.Keyboard.UpAsync("a");
        await page.Keyboard.UpAsync("Control");
        await page.AssertEqualsAsync(_ => page.IsContentsSelectedAsync(), false);

        // Navigate to the "Fetch Data" page,
        await page.Keyboard.DownAsync("f");
        await page.Keyboard.UpAsync("f");
        await page.AssertUrlIsAsync(host.GetUrl("/fetchdata"));

        // Ctrl+A causes select all of contents.
        await page.AssertEqualsAsync(_ => page.IsContentsSelectedAsync(), false);
        await page.Keyboard.DownAsync("Control");
        await page.Keyboard.DownAsync("a");
        await page.Keyboard.UpAsync("a");
        await page.Keyboard.UpAsync("Control");
        await Task.Delay(200);
        await page.AssertEqualsAsync(_ => page.IsContentsSelectedAsync(), true);
    }

    [Test]
    [TestCaseSource(typeof(HotKeysOnBrowserTest), nameof(AllHostingModels))]
    public async Task HotKeyOptions_NotToPreventDefault_Test(HostingModel hostingModel)
    {
        var context = TestContext.Instance;
        var host = await context.StartHostAsync(hostingModel);

        // Navigate to the "Counter" page,
        var page = await context.GetPageAsync();
        await page.GotoAndWaitForReadyAsync(host.GetUrl("/counter"));
        await page.AssertUrlIsAsync(host.GetUrl("/counter"));
        var counter = page.Locator("h1+p");
        await page.AssertEqualsAsync(_ => counter.TextContentAsync(), "Current count: 0");

        // Set initial focus position
        var inputElement = page.Locator(".disabled-hotkeys input");
        await inputElement.FocusAsync();

        // Enter Tab key in the ".disabled-hotkeys" input element, then the focus will move to the next element.
        await page.Keyboard.PressAsync("Tab");
        await page.AssertIsFocusedAsync(".allow-default input");
        await page.AssertEqualsAsync(_ => counter.TextContentAsync(), "Current count: 0");

        // Enter Tab key in the ".allow-default" input element. Tab key is registered as hotkey to increment the counter,
        await page.Keyboard.PressAsync("Tab");
        // so the counter will be incremented,
        await page.AssertEqualsAsync(_ => counter.TextContentAsync(), "Current count: 1");
        // but this hot key is registered with the PreventDefault = false option, so the focus will move to the next element as usual.
        await page.AssertIsFocusedAsync("input.disabled-state-hotkeys");
    }

    [Test]
    [TestCaseSource(typeof(HotKeysOnBrowserTest), nameof(AllHostingModels))]
    public async Task HelperJavaScript_Namespace_Not_Conflict_Test(HostingModel hostingModel)
    {
        var context = TestContext.Instance;
        var host = await context.StartHostAsync(hostingModel);

        // Navigate to the "Save Text" page,
        var page = await context.GetPageAsync();
        await page.GotoAndWaitForReadyAsync(host.GetUrl("/save-text"));

        // Input random text into the text box,
        var text = Guid.NewGuid().ToString("N");
        await page.FocusAsync("#text-box-1");
        foreach (var c in text) { await page.Keyboard.DownAsync(c.ToString()); }
        await Task.Delay(200);

        // Enter Ctrl + S,
        await page.Keyboard.DownAsync("Control");
        await page.Keyboard.DownAsync("s");
        await page.Keyboard.UpAsync("s");
        await page.Keyboard.UpAsync("Control");

        // Then the inputted text will appear in the area for display
        // by works of the helper JavaScript code
        // that lives in the "Toolbelt.Blazor" namespace.
        //(If the namespace were conflicted, it wouldn't work.)
        var savedTextList = page.Locator("#saved-text-list");
        await page.AssertEqualsAsync(async _ => ((await savedTextList.TextContentAsync()) ?? "").Trim(), "\"" + text + "\"");
    }

    [Test]
    [TestCaseSource(typeof(HotKeysOnBrowserTest), nameof(AllHostingModels))]
    public async Task ExcludeContentEditable_Test(HostingModel hostingModel)
    {
        var context = TestContext.Instance;
        var host = await context.StartHostAsync(hostingModel);

        // Navigate to the "Test - Exclude Content Editable" page,
        var page = await context.GetPageAsync();
        await page.GotoAndWaitForReadyAsync(host.GetUrl("/test/exclude-contenteditable"));

        // Set focus to the "contenteditable" div element.
        var editorArea = page.Locator("#editor-area");
        await page.WaitForAsync(async _ => ((await editorArea.TextContentAsync()) ?? "").Trim().StartsWith("In this area,"));
        await editorArea.FocusAsync();

        // Enter the "H" key, but the hokey for "H" should not be worked,
        // instead, the character "h" should be inserted into the contenteditable div area.
        await page.Keyboard.DownAsync("Home");
        await page.Keyboard.UpAsync("Home");
        await page.Keyboard.DownAsync("h");
        await page.Keyboard.UpAsync("h");
        await page.AssertUrlIsAsync(host.GetUrl("/test/exclude-contenteditable"));
        await page.WaitForAsync(async _ => ((await editorArea.TextContentAsync()) ?? "").Trim().StartsWith("hIn this area,"));

        // But, enter the "C" key, then the hokey for "C" should be worked.(go to the "Counter" page.)
        await page.Keyboard.DownAsync("c");
        await page.Keyboard.UpAsync("c");
        await page.AssertUrlIsAsync(host.GetUrl("/counter"));

        // Reenter the test page, and enter the "F" key,
        await page.GotoAndWaitForReadyAsync(host.GetUrl("/test/exclude-contenteditable"));
        await editorArea.FocusAsync();
        await page.Keyboard.DownAsync("f");
        await page.Keyboard.UpAsync("f");

        // ...then the hokey for "F" should be worked. (goto the "Fetch Data" page.)
        await page.AssertUrlIsAsync(host.GetUrl("/fetchdata"));
    }

    [Test]
    [TestCaseSource(typeof(HotKeysOnBrowserTest), nameof(AllHostingModels))]
    public async Task ExcludeSelector_Test(HostingModel hostingModel)
    {
        var context = TestContext.Instance;
        var host = await context.StartHostAsync(hostingModel);

        // Navigate to the "Counter" page,
        var page = await context.GetPageAsync();
        await page.GotoAndWaitForReadyAsync(host.GetUrl("/counter"));

        // Verify the counter is 0.
        var counter = page.Locator("h1+p");
        await page.AssertEqualsAsync(_ => counter.TextContentAsync(), "Current count: 0");

        // Set focus to the "Hotkeys are enabled in this field" input element, and type "U" key.
        // Then the counter should be incremented.
        var inputElement1 = page.GetByPlaceholder("Hotkeys are enabled in this field");
        await inputElement1.FocusAsync();
        await page.Keyboard.DownAsync("u");
        await page.Keyboard.UpAsync("u");
        await page.AssertEqualsAsync(_ => counter.TextContentAsync(), "Current count: 1");
        await page.Keyboard.DownAsync("u");
        await page.Keyboard.UpAsync("u");
        await page.AssertEqualsAsync(_ => counter.TextContentAsync(), "Current count: 2");

        // Set focus to the "Hotkeys are disabled in this field" input element, and type "U" key.
        // Then the counter should not be incremented.
        var inputElement2 = page.GetByPlaceholder("Hotkeys are disabled in this field");
        await inputElement2.FocusAsync();
        await page.Keyboard.DownAsync("u");
        await page.Keyboard.UpAsync("u");
        await page.AssertEqualsAsync(_ => counter.TextContentAsync(), "Current count: 2");
        await page.Keyboard.DownAsync("u");
        await page.Keyboard.UpAsync("u");
        await page.AssertEqualsAsync(_ => counter.TextContentAsync(), "Current count: 2");
        await page.AssertEqualsAsync(_ => inputElement2.InputValueAsync(), "uu");
    }

    [Test]
    [TestCaseSource(typeof(HotKeysOnBrowserTest), nameof(AllHostingModels))]
    public async Task StateDisabled_Test(HostingModel hostingModel)
    {
        var context = TestContext.Instance;
        var host = await context.StartHostAsync(hostingModel);

        // Navigate to the "Counter" page,
        var page = await context.GetPageAsync();
        await page.GotoAndWaitForReadyAsync(host.GetUrl("/counter"));

        // Verify the counter is 0.
        var counter = page.Locator("h1+p");
        await page.AssertEqualsAsync(_ => counter.TextContentAsync(), "Current count: 0");

        // Set focus to the "Hotkeys are enabled in this field" input element, and type "U" key.
        // Then the counter should not be incremented.
        var inputElement1 = await page.QuerySelectorAsync(".disabled-state-hotkeys") ?? throw new InvalidOperationException("Test element is missing");
        await inputElement1.FocusAsync();
        await page.Keyboard.DownAsync("y");
        await page.Keyboard.UpAsync("y");
        await page.AssertEqualsAsync(_ => counter.TextContentAsync(), "Current count: 0");
        await page.Keyboard.DownAsync("y");
        await page.Keyboard.UpAsync("y");
        await page.AssertEqualsAsync(_ => counter.TextContentAsync(), "Current count: 0");
        await page.AssertEqualsAsync(_ => inputElement1.InputValueAsync(), "yy");
    }

    [Test]
    [TestCaseSource(typeof(HotKeysOnBrowserTest), nameof(AllHostingModels))]
    public async Task StateDisabledTrigger_Test(HostingModel hostingModel)
    {
        var context = TestContext.Instance;
        var host = await context.StartHostAsync(hostingModel);

        // Navigate to the "Counter" page,
        var page = await context.GetPageAsync();
        await page.GotoAndWaitForReadyAsync(host.GetUrl("/counter"));

        // Verify the counter is 0.
        var counter = page.Locator("h1+p");
        await page.AssertEqualsAsync(_ => counter.TextContentAsync(), "Current count: 0");

        // Set focus to the "Hotkeys are enabled in this field" input element, and type "y" key.
        // Then the counter should not be incremented.
        var inputElement1 = await page.QuerySelectorAsync(".disabled-state-hotkeys") ?? throw new InvalidOperationException("Test element is missing");
        await page.AssertEqualsAsync(_ => inputElement1.GetAttributeAsync("placeholder"), "Hotkey 'Y' is disabled in C#");
        await inputElement1.FocusAsync();
        await page.Keyboard.DownAsync("y");
        await page.Keyboard.UpAsync("y");
        await page.AssertEqualsAsync(_ => counter.TextContentAsync(), "Current count: 0");
        await page.Keyboard.DownAsync("y");
        await page.Keyboard.UpAsync("y");
        await page.AssertEqualsAsync(_ => counter.TextContentAsync(), "Current count: 0");
        await page.AssertEqualsAsync(_ => inputElement1.InputValueAsync(), "yy");

        // Trigger disabled state
        await page.ClickAsync(".state-trigger-button");
        await page.AssertEqualsAsync(_ => inputElement1.GetAttributeAsync("placeholder"), "Hotkey 'Y' is enabled in C#");

        // Refocus and test again
        // This time counter should increment
        await inputElement1.FocusAsync();

        await page.Keyboard.DownAsync("y");
        await page.Keyboard.UpAsync("y");
        await page.AssertEqualsAsync(_ => counter.TextContentAsync(), "Current count: 1");

        await page.Keyboard.DownAsync("y");
        await page.Keyboard.UpAsync("y");
        await page.AssertEqualsAsync(_ => counter.TextContentAsync(), "Current count: 2");

        await page.AssertEqualsAsync(_ => inputElement1.InputValueAsync(), "yy");
    }

    [Test]
    [TestCaseSource(typeof(HotKeysOnBrowserTest), nameof(AllHostingModels))]
    public async Task ByNativeKey_Test(HostingModel hostingModel)
    {
        var context = TestContext.Instance;
        var host = await context.StartHostAsync(hostingModel);
        var page = await context.GetPageAsync();

        // Navigate to the "Home" page,
        await page.GotoAndWaitForReadyAsync(host.GetUrl("/"));
        var h1 = page.Locator("h1");
        await page.AssertEqualsAsync(_ => h1.TextContentAsync(), "Hello, world!");

        // US
        await page.FireOnKeyDown(key: "@", code: "Digit2", keyCode: 0x32, shiftKey: true);
        await page.AssertEqualsAsync(_ => h1.TextContentAsync(), "Hi, there!");

        // Japanese JIS
        await page.FireOnKeyDown(key: "@", code: "BlaceLeft", keyCode: 0xc0, shiftKey: false);
        await page.AssertEqualsAsync(_ => h1.TextContentAsync(), "How's it going?");
    }

    [Test]
    [TestCaseSource(typeof(HotKeysOnBrowserTest), nameof(AllHostingModels))]
    public async Task ByCode_Test(HostingModel hostingModel)
    {
        var context = TestContext.Instance;
        var host = await context.StartHostAsync(hostingModel);
        var page = await context.GetPageAsync();

        // Given: Navigate to the "Test by Code" page,
        await page.GotoAndWaitForReadyAsync(host.GetUrl("/test/bycode"));
        await page.AssertH1IsAsync("Test by Code");

        var list = page.Locator("tbody");

        // When
        await page.FireOnKeyDown(key: "Shift", code: "ShiftLeft", keyCode: 0x10, shiftKey: true);
        await page.FireOnKeyDown(key: "Shift", code: "ShiftRight", keyCode: 0x10, shiftKey: true);
        await page.FireOnKeyDown(key: "Shift", code: "ShiftRight", keyCode: 0x10, shiftKey: true);
        await page.FireOnKeyDown(key: "Control", code: "ControlLeft", keyCode: 0x11, ctrlKey: true);
        await page.FireOnKeyDown(key: "Alt", code: "AltLeft", keyCode: 0x12, altKey: true);
        await page.FireOnKeyDown(key: "Alt", code: "AltRight", keyCode: 0x12, altKey: true);
        await page.FireOnKeyDown(key: "Alt", code: "AltRight", keyCode: 0x12, altKey: true);
        await page.FireOnKeyDown(key: "Meta", code: "MetaLeft", keyCode: 0x5b, metaKey: true);
        await page.FireOnKeyDown(key: "Meta", code: "MetaLeft", keyCode: 0x5b, metaKey: true);
        await page.FireOnKeyDown(key: "Meta", code: "MetaRight", keyCode: 0x5b, metaKey: true);
        await page.FireOnKeyDown(key: "Control", code: "ControlRight", keyCode: 0x11, ctrlKey: true);

        // Then
        await page.AssertEqualsAsync(_ => list.InnerTextAsync(),
            "\t\tShiftLeft\tx 1" + "\n" +
            "\t\tShiftRight\tx 2" + "\n" +
            "\t\tControlLeft\tx 1" + "\n" +
            "\t\tAltLeft\tx 1" + "\n" +
            "\t\tAltRight\tx 2" + "\n" +
            "\t\tMetaLeft\tx 2" + "\n" +
            "\t\tMetaRight\tx 1" + "\n" +
            "\t\tControlRight\tx 1");
    }


    [Test]
    [TestCaseSource(typeof(HotKeysOnBrowserTest), nameof(AllHostingModels))]
    public async Task Remove_Test(HostingModel hostingModel)
    {
        var context = TestContext.Instance;
        var host = await context.StartHostAsync(hostingModel);

        // Navigate to the "Home" page,
        var page = await context.GetPageAsync();
        await page.GotoAndWaitForReadyAsync(host.GetUrl("/"));

        var h1 = page.Locator("h1");
        var h1TextBefore = await h1.TextContentAsync();
        h1TextBefore.Is("Hello, world!");

        // Verify the hot key "G" works correctly at this time. (Emulate to press the "G" key.)
        await page.Keyboard.DownAsync("g");
        await page.Keyboard.UpAsync("g");
        await page.WaitForAsync(async _ => (await h1.TextContentAsync()) == "Hi, there!");

        // But after the hot key was removed...
        await page.ClickAsync("text=Remove the hot key \"G\"");
        await Task.Delay(200);

        // The hot key "G" is no longer working.
        await page.Keyboard.DownAsync("g");
        await page.Keyboard.UpAsync("g");
        await Task.Delay(500);
        await page.WaitForAsync(async _ => (await h1.TextContentAsync()) == "Hi, there!");
    }

    [TestCaseSource(typeof(HotKeysOnBrowserTest), nameof(AllHostingModels))]
    public async Task Dispose_Test(HostingModel hostingModel)
    {
        var context = TestContext.Instance;
        var host = await context.StartHostAsync(hostingModel);

        // Navigate to the "Home" page,
        var page = await context.GetPageAsync();
        await page.GotoAndWaitForReadyAsync(host.GetUrl("/"));

        var h1 = page.Locator("h1");
        var h1TextBefore = await h1.TextContentAsync();
        h1TextBefore.Is("Hello, world!");

        // Verify the hot key "G" works correctly at this time. (Emulate to press the "G" key.)
        await page.Keyboard.DownAsync("g");
        await page.Keyboard.UpAsync("g");
        await page.WaitForAsync(async _ => (await h1.TextContentAsync()) == "Hi, there!");

        // But after the hot key was removed...
        await page.ClickAsync("text=Dispose the hot key context");
        await Task.Delay(200);

        // The hot key "G" is no longer working.
        await page.Keyboard.DownAsync("g");
        await page.Keyboard.UpAsync("g");
        await Task.Delay(500);
        await page.WaitForAsync(async _ => (await h1.TextContentAsync()) == "Hi, there!");
    }

    [TestCaseSource(typeof(HotKeysOnBrowserTest), nameof(AllHostingModels))]
    public async Task DisposeAfterCreateContextImmediately_Test(HostingModel hostingModel)
    {
        var context = TestContext.Instance;
        var host = await context.StartHostAsync(hostingModel);

        // Navigate to the "Home" page,
        var page = await context.GetPageAsync();
        await page.GotoAndWaitForReadyAsync(host.GetUrl("/"));

        var h1 = page.Locator("h1");
        var h1TextBefore = await h1.TextContentAsync();
        h1TextBefore.Is("Hello, world!");

        // The hot key "u" should not available at this time.
        await page.Keyboard.DownAsync("u");
        await page.Keyboard.UpAsync("u");
        await Task.Delay(500);
        await page.WaitForAsync(async _ => (await h1.TextContentAsync()) == "Hello, world!");
    }

    public record CustomElementTestCase(string TargetSelector, string ExcludedKey, string AvailableKey, string ExpectedURL);

    private static readonly IEnumerable<CustomElementTestCase> CustomElementTestCases = [
        new(TargetSelector: "#input-1", ExcludedKey: "f", AvailableKey: "h", ExpectedURL: "/" ),
        new(TargetSelector: "#text-area-1", ExcludedKey: "f", AvailableKey: "t", ExpectedURL: "/test/bykeyname" ),
        new(TargetSelector: "#select-1", ExcludedKey: "s", AvailableKey: "f", ExpectedURL: "/fetchdata" ),
        new(TargetSelector: "#checkbox-1", ExcludedKey: "f", AvailableKey: "c", ExpectedURL: "/counter" ),
        new(TargetSelector: "#radio-group-1", ExcludedKey: "t", AvailableKey: "h", ExpectedURL: "/" ),
        new(TargetSelector: "#button-1", ExcludedKey: "s", AvailableKey: "f", ExpectedURL: "/fetchdata" ),
        new(TargetSelector: "#input-button-1", ExcludedKey: "t", AvailableKey: "c", ExpectedURL: "/counter" ),
    ];

    private static IEnumerable<object[]> AllCustomElementTestCases { get; } =
        from testCase in CustomElementTestCases
        from hostingModel in AllHostingModels
        select new object[] { testCase, hostingModel };

    [Test]
    [TestCaseSource(typeof(HotKeysOnBrowserTest), nameof(AllCustomElementTestCases))]
    public async Task ExcludeCustomElements_Test(CustomElementTestCase testCase, HostingModel hostingModel)
    {
        var context = TestContext.Instance;
        var host = await context.StartHostAsync(hostingModel);

        // Navigate to the "Test Custom Elements" page,
        var page = await context.GetPageAsync();
        await page.GotoAndWaitForReadyAsync(host.GetUrl("/test/custom-elements"));

        // Set focus to test target element.
        // (NOTE: Playwright's `FocusAsync` method does not work for custom elements, so we need to use custom implementation.) 
        await page.FocusByScriptAsync(testCase.TargetSelector);

        // Enter the excluded hot key, but the excluded hokey should not be worked,
        // so it stays on the current page.
        await page.Keyboard.DownAsync(testCase.ExcludedKey);
        await page.Keyboard.UpAsync(testCase.ExcludedKey);
        await page.AssertUrlIsAsync(host.GetUrl("/test/custom-elements"));

        // But, enter the available key, then the available hokey should be worked.(go to the expected page.)
        await page.Keyboard.DownAsync(testCase.AvailableKey);
        await page.Keyboard.UpAsync(testCase.AvailableKey);
        await page.AssertUrlIsAsync(host.GetUrl(testCase.ExpectedURL));
    }

    [Test]
    [TestCaseSource(typeof(HotKeysOnBrowserTest), nameof(AllHostingModels))]
    public async Task Numpad_NumLock_Shift_Keys_Test(HostingModel hostingModel)
    {
        var context = TestContext.Instance;
        var host = await context.StartHostAsync(hostingModel);

        // Navigate to the "Counter" page,
        var page = await context.GetPageAsync();
        await page.GotoAndWaitForReadyAsync(host.GetUrl("/counter"));

        // Verify the counter is 0.
        var counterText = page.Locator("h1+p");
        await page.AssertEqualsAsync(_ => counterText.TextContentAsync(), "Current count: 0");

        // The key list of the number pad keys.
        var numPadKeys = new[] {
            new{ Code = "Numpad0",       KeyA = "0", KeyB = "Insert" },
            new{ Code = "Numpad1",       KeyA = "1", KeyB = "End" },
            new{ Code = "Numpad2",       KeyA = "2", KeyB = "ArrowDown" },
            new{ Code = "Numpad3",       KeyA = "3", KeyB = "PageDown" },
            new{ Code = "Numpad4",       KeyA = "4", KeyB = "ArrowLeft" },
            new{ Code = "Numpad5",       KeyA = "5", KeyB = "Clear" },
            new{ Code = "Numpad6",       KeyA = "6", KeyB = "ArrowRight" },
            new{ Code = "Numpad7",       KeyA = "7", KeyB = "Home" },
            new{ Code = "Numpad8",       KeyA = "8", KeyB = "ArrowUp" },
            new{ Code = "Numpad9",       KeyA = "9", KeyB = "PageUp" },
            new{ Code = "NumpadDecimal", KeyA = ".", KeyB = "Delete" },
        };

        var counter = 0;

        // NumLock ON & enter number pad keys -> increment counter
        foreach (var key in numPadKeys)
        {
            await page.FireOnKeyDown(numLock: true, key: key.KeyA, code: key.Code, keyCode: 0);
            counter++;
            await page.AssertEqualsAsync(_ => counterText.TextContentAsync(), $"Current count: {counter}");
        }

        // NumLock ON & enter number pad keys with SHIFT -> decrement counter
        foreach (var key in numPadKeys)
        {
            await page.FireOnKeyDown(numLock: true, key: key.KeyB, code: key.Code, keyCode: 0);
            counter--;
            await page.AssertEqualsAsync(_ => counterText.TextContentAsync(), $"Current count: {counter}");
        }

        // NumLock OFF & enter number pad keys -> increment counter
        foreach (var key in numPadKeys)
        {
            await page.FireOnKeyDown(numLock: false, key: key.KeyB, code: key.Code, keyCode: 0);
            counter++;
            await page.AssertEqualsAsync(_ => counterText.TextContentAsync(), $"Current count: {counter}");
        }

        // NumLock OFF & enter number pad keys with SHIFT -> decrement counter
        foreach (var key in numPadKeys)
        {
            await page.FireOnKeyDown(numLock: false, key: key.KeyA, code: key.Code, keyCode: 0, shiftKey: true);
            counter--;
            await page.AssertEqualsAsync(_ => counterText.TextContentAsync(), $"Current count: {counter}");
        }
    }
}
