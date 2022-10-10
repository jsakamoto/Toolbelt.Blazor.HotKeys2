using Toolbelt.Blazor.HotKeys2.E2ETest.Internals;

namespace Toolbelt.Blazor.HotKeys2.E2ETest;

public class HotKeysOnBrowserTest
{
    public static IEnumerable<HostingModel> AllHostingModels { get; } = new[] {
            HostingModel.Wasm50,
            HostingModel.Wasm60,
            HostingModel.Server50,
            HostingModel.Server60,
        };

    public static IEnumerable<HostingModel> WasmHostingModels { get; } = new[] {
            HostingModel.Wasm50,
            HostingModel.Wasm60,
        };

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

        // Show and hide the cheatSeetElement by entering "?" and ESC key.
        var cheatSeetElement = page.Locator(".popup-container");
        (await cheatSeetElement.IsVisibleAsync()).IsFalse();

        await page.Keyboard.DownAsync("Shift"); // Enter "?"
        await page.Keyboard.DownAsync("Slash");
        await page.Keyboard.UpAsync("Slash");
        await page.Keyboard.UpAsync("Shift");
        await page.AssertEqualsAsync(_ => cheatSeetElement.IsVisibleAsync(), true);

        await page.Keyboard.DownAsync("Escape");
        await page.Keyboard.UpAsync("Escape");
        await page.AssertEqualsAsync(_ => cheatSeetElement.IsVisibleAsync(), false);

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

        // Navigate to the "Test All Keys" page,
        var page = await context.GetPageAsync();
        await page.GotoAndWaitForReadyAsync(host.GetUrl("/test/onkeydown"));
        await page.AssertUrlIsAsync(host.GetUrl("/test/onkeydown"));

        // and shows cheat sheet.
        var cheatSeetElement = page.Locator(".popup-container");
        await page.Keyboard.DownAsync("Shift"); // Enter "?"
        await page.Keyboard.DownAsync("Slash");
        await page.Keyboard.UpAsync("Slash");
        await page.Keyboard.UpAsync("Shift");
        await page.AssertEqualsAsync(_ => cheatSeetElement.IsVisibleAsync(), true);

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
        var cheatSeetElement = page.Locator(".popup-container");
        await page.Keyboard.DownAsync("Shift"); // Enter "?"
        await page.Keyboard.DownAsync("Slash");
        await page.Keyboard.UpAsync("Slash");
        await page.Keyboard.UpAsync("Shift");
        await page.AssertEqualsAsync(_ => cheatSeetElement.IsVisibleAsync(), true);

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
    public async Task PreventDefault_Test(HostingModel hostingModel)
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
    public async Task HelperJavaScript_Namespace_Not_Conflict_Test(HostingModel hostingModel)
    {
        var context = TestContext.Instance;
        var host = await context.StartHostAsync(hostingModel);

        // Navigate to the "Save Text" page,
        var page = await context.GetPageAsync();
        await page.GotoAndWaitForReadyAsync(host.GetUrl("/save-text"));

        // Input random text into the testbox,
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
    public async Task ByNativeKey_Test(HostingModel hostingModel)
    {
        var context = TestContext.Instance;
        await context.StartHostAsync(hostingModel);

        // Navigate to the "Home" page,
        var page = await context.GetPageAsync();
        await page.GotoAndWaitForReadyAsync(context.GetHostUrl(hostingModel));

        var h1 = page.Locator("h1");
        var h1TextBefore = await h1.TextContentAsync();
        h1TextBefore.Is("Hello, world!");

        // US
        await page.EvaluateAsync("Toolbelt.Blazor.fireOnKeyDown", new
        {
            selector = "body",
            options = new { key = "@", code = "Digit2", keyCode = 0x32, shiftKey = true }
        });
        await page.WaitForAsync(async _ => (await h1.TextContentAsync()) == "Hi, there!");

        // Japanese JIS
        await page.EvaluateAsync("Toolbelt.Blazor.fireOnKeyDown", new
        {
            selector = "body",
            options = new { key = "@", code = "BlaceLeft", keyCode = 0xc0, shiftKey = false }
        });
        await page.WaitForAsync(async _ => (await h1.TextContentAsync()) == "How's it going?");
    }

    [Test]
    [TestCaseSource(typeof(HotKeysOnBrowserTest), nameof(AllHostingModels))]
    public async Task Remove_Test(HostingModel hostingModel)
    {
        var context = TestContext.Instance;
        await context.StartHostAsync(hostingModel);

        // Navigate to the "Home" page,
        var page = await context.GetPageAsync();
        await page.GotoAndWaitForReadyAsync(context.GetHostUrl(hostingModel));

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
}
