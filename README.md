# Blazor HotKeys2

[![NuGet Package](https://img.shields.io/nuget/v/Toolbelt.Blazor.HotKeys2.svg)](https://www.nuget.org/packages/Toolbelt.Blazor.HotKeys2/) [![unit tests](https://github.com/jsakamoto/Toolbelt.Blazor.HotKeys2/actions/workflows/unit-tests.yml/badge.svg)](https://github.com/jsakamoto/Toolbelt.Blazor.HotKeys2/actions/workflows/unit-tests.yml) [![Discord](https://img.shields.io/discord/798312431893348414?style=flat&logo=discord&logoColor=white&label=Blazor%20Community&labelColor=5865f2&color=gray)](https://discord.com/channels/798312431893348414/1202165955900473375)

## Summary

This is a class library that provides configuration-centric keyboard shortcuts for your Blazor apps.  
**(This library is a successor of [the "Blazor HotKeys" library](https://github.com/jsakamoto/Toolbelt.Blazor.HotKeys).)**

![movie](https://raw.githubusercontent.com/jsakamoto/Toolbelt.Blazor.HotKeys2/master/.assets/movie-001.gif)

- [Live demo](https://jsakamoto.github.io/Toolbelt.Blazor.HotKeys2/)

You can declare associations of keyboard shortcut and callback action, like this code:

```csharp
// The method "OnSelectAll" will be invoked 
//  when the user typed Ctrl+A key combination.
_hotKeysContext = this.HotKeys.CreateContext()
  .Add(ModCode.Ctrl, Code.A, OnSelectAll)
  .Add(...)
  ...;
```

## Supported Blazor versions

This library suppots ASP.NET Core Blazor version 8.0, 9.0, or later.

## How to install and use?

### 1. Installation and Registration

**Step.1** Install the library via NuGet package, like this.

```shell
> dotnet add package Toolbelt.Blazor.HotKeys2
```

**Step.2** Register "HotKeys" service into the DI container.

```csharp
// Program.cs
using Toolbelt.Blazor.Extensions.DependencyInjection; // 👈 1. Add this line
...
builder.Services.AddHotKeys2(); // 👈 2. Add this line
...
```

### 2. Usage in your Blazor component (.razor)

**Step.1** Implement `IAsyncDisposable` interface to the component.

```razor
@implements IAsyncDisposable @* 👈 Add this at the top of the component.  *@
...

@code {
  ...
  public async ValueTask DisposeAsync() // 👈 Add "DisposeAsync" method.
  {
  }
}
```

**Step.2** Open the `Toolbelt.Blazor.HotKeys2` namespace, and inject the `HotKeys` service into the component.

```razor
@implements IAsyncDisposable
@using Toolbelt.Blazor.HotKeys2 @* 👈 1. Add this *@
@inject HotKeys HotKeys @* 2. 👈 Add this *@
...
```

**Step.3** Invoke the `CreateContext()` method of the `HotKeys` service instance at the timing for the first time the component renders, such as the `OnAfterRender()` method, to create and activate hotkey entries. Please make sure to keep the `HotKeysContext` object, which is returned from the `CreateContext()` method, in the component field.

Then, you can add the combination with key and action to the `HotKeysContext` object using the `Add()` method.

```csharp
@code {

  private HotKeysContext? _hotKeysContext;

  protected override void OnAfterRender(bool firstRender)
  {
    if (firstRender) {
      _hotKeysContext = this.HotKeys.CreateContext()
        .Add(ModCode.Ctrl|ModCode.Shift, Code.A, FooBar, new() { Description = "do foo bar." })
        .Add(...)
        ...;
    }
  }

  private void FooBar() // 👈 This will be invoked when Ctrl+Shift+A typed.
  {
    ...
  }
}
```

> [!NOTE]  
> You can also specify the async method to the callback action argument.

> [!NOTE]  
> The method of the callback action can take an argument which is `HotKeyEntryByCode` or `HotKeyEntryByKey` object.


**Step.4** Dispose the `HotKeysContext` object when the component is disposing, in the `DisposeAsync()` method of the component.

```csharp
@code {
  ...
  public async ValueTask DisposeAsync()
  {
    // 👇 Add this
    if (_hotKeysContext != null) {
      await _hotKeysContext.DisposeAsync(); 
    }
  }
}
```

The complete source code (.razor) of this component is bellow.

```csharp
@page "/"
@implements IAsyncDisposable
@using Toolbelt.Blazor.HotKeys2
@inject HotKeys HotKeys

@code {

  private HotKeysContext? _hotKeysContext;

  protected override void OnAfterRender(bool firstRender)
  {
    if (firstRender) {
      _hotKeysContext = this.HotKeys.CreateContext()
        .Add(ModCode.Ctrl|ModCode.Shift, Code.A, FooBar, new() { Description = "do foo bar." });
    }
  }

  private void FooBar()
  {
    // Do something here.
  }

  public async ValueTask DisposeAsync()
  {
    if (_hotKeysContext != null) {
      await _hotKeysContext.DisposeAsync(); 
    }
  }
}
```

### How to enable / disable hotkeys depending on which element has focus

You can specify enabling/disabling hotkeys depending on which kind of element has focus at the hotkeys registration via a combination of the `Exclude` flags in the property of the option object argument of the `HotKeysContext.Add()` method.

The default value of the option object's `Exclude` flag property is the following combination.

```csharp
Exclude.InputText | Exclude.InputNonText | Exclude.TextArea
```

This means hotkeys are disabled when the focus is in `<input>` (with any `type`) or `<textarea>` elements by default.

If you want to enable hotkeys even when an `<input type="text"/>` has focus, you can implement it as below.

```csharp
... this.HotKeys.CreateContext()
  .Add(Code.A, OnKeyDownA, new() { 
    // 👇 Specify the "Exclude" property of the options.
    Exclude = Exclude.InputNonText | Exclude.TextArea })
  ...
```

You can also specify the elements that are disabled hotkeys by CSS query selector string via the `ExcludeSelector` property of the options object.

```csharp
... this.HotKeys.CreateContext()
  .Add(Code.A, OnKeyDownA, new() { 
    // 👇 Specify the CSS query selector to the "ExcludeSelector" property of the options.
    ExcludeSelector = ".disabled-hotkeys-area" })
  ...
```

And you can specify the `Exclude.ContentEditable` to register the unavailable hotkey when any "contenteditable" applied elements have focus.

### How to enable / disable hotkeys depending on application states

You can also specify enabling/disabling hotkeys depending on the application states through the `Disabled` property of the `HotKeyEntryState` object included by a `HotKeyEntry` as its `State` property. You can initialize the `State` property of the `HotKeyEntry` object when you call the `HotKeysContext.Add()` method.

```csharp
...
private HotKeyEntryState _state = new() { Disabled = true };

protected override void OnAfterRender(bool firstRender)
{
  if (firstRender) {
    _hotKeysContext = this.HotKeys.CreateContext()
      // 👇 Specify the "State" property of the option object.
      .Add(Code.A, OnHotKeyA, new() { State = _state });
  }
}
...
```

And you can change the `Disabled` property of the `HotKeyEntryState` object to enable/disable the hotkey whenever you want.

```csharp
private void OnClickEnableHotKeyA()
{
  _state.Disabled = false;
}
```

You can also control enable/disable a hotkey more declaratively by updating the `Disabled` property in the `OnAfterRender()` lifecycle method.

```csharp
protected override void OnAfterRender(bool firstRender)
{
  // Update the state of the hotkey entry every time 
  // the component is rendered.
  // Because the causing of rendering means that
  // some of the states of the component have been changed.
  _state.Disabled = _showDialog || _panelPopuped;
}
```

### How to remove hotkeys

You can remove hotkkey entries by calling the `Remove()` method of the `HotKeysContext` object, like this.

```csharp
_hotKeysContext.Remove(ModCode.Ctrl, Code.A);
```

Please remember that the `Remove` method will remove a hotkey entry identified by the `key`, `code`, and `modifiers` parameters even if other parameters are unmatched by the registered hotkey entry as long as it can identify a single hotkey entry.

```csharp
... 
  _hotKeysContext = this.HotKeys.CreateContext()
    .Add(Code.A, OnKeyDownA, exclude: Exclude.InputNonText | Exclude.TextArea);
...
// The following code will remove the hotkey entry registered by the above code
// even though the "exclude" option is different.
_hotKeysContext.Remove(Code.A);
```

If the parameters for the `Remove` method can not determine a single hotkey entry, the `ArgumentException` exception will be thrown.


```csharp
... 
_hotKeysContext = this.HotKeys.CreateContext()
  .Add(Code.A, OnKeyDownAForTextArea, exclude: Exclude.InputNonText | Exclude.InputText)
  .Add(Code.A, OnKeyDownAForInputText, exclude: Exclude.InputNonText | Exclude.TextArea);
...
// The following code will throw an ArgumentException exception
// because the "Remove" method can not determine a single hotkey entry.
_hotKeysContext.Remove(Code.A);
...
// The following code will successfully remove the hotkey entry in the second one.
_hotKeysContext.Remove(Code.A, exclude: Exclude.InputNonText | Exclude.TextArea);
```

If the `key`, `code`, and `modifires` parameters cannot find any hotkey entry, the `Remove`  method will return without exception.

The `HotKeysContext` also provides another `Remove` method overload version that accepts a filter function as an argument to determine which hotkey entries to remove. This method will remove all hotkey entries in which the filter function returns.

```csharp
// The following code will remove all hotkey entries registered by the "Code. A",
// regardless of what modifiers, exclude options, etc.
_hotKeysContext.Remove(entries =>
{
  return entries.Where(e => e is HotKeyEntryByCode codeEntry && codeEntry.Code == Code.A);
});
```

## `Code` vs. `Key` - which way should I use to?

There are two ways to register hotkeys in the `HotKeysContext`.

One of them is registration by the `Code` class, and another one is registration by the `Key` class.

### `Code`

Hotkeys registration by the `Code` class is based on **the physical location of keys**.

For example, if you register a hotkey by `Add(ModCodes.Shift, Code.A, callback)`, the `callback` will be invoked when a user presses the "Shift" and the "A" keys. In this case, that hotkey doesn't depend on the Caps Lock key condition. Regardless of whether the Caps Lock key is on or off, the `callback` will be invoked whenever a user press the "Shift + A". This means the hotkey registered by the `Code` class works based on the location of the pressed key. It is no matter what character will be inputted, both "a" lower case and "A" upper case, as long as the key printed "A" on its key top is pressed.

I recommend using the `Code` class for hotkeys registration in the following cases.

- The hotkeys are based on alphabetical or numerical keytops.
- The hotkeys are based on the difference of left or right of the Shift, Control, Alt, and Meta keys.
- The hotkeys are based on a combination with the Shift key.

### `Key`

Hotkeys registration by the `Key` class is based on **the character** inputted by key pressing.

For example, if you register a hotkey by `Add(Key.Question, callback)`, the `callback` will be invoked when a user presses a key combination that will input the `?` character. In this case, that hotkey doesn't depend on the physical layout of the keys. Generally, the `?` character will be inputted by pressing the "Shift+/" on a US layout keyboard. But, on a Czech Republic layout keyboard, the `?` character will be inputted by pressing the "Shift+,". Therefore, you should not register the hotkey for the `?` by the `Code` class based on physical key locations, like `Add(ModCodes.Shift, Code.Slash, ...)`. 

In addition,  the combination with the Shift key can not use on the registration hotkeys by the `Key` class. This is a limitation for the hotkeys to be independent on physical keyboard layout.

I recommend using the `Key` class for hotkeys registration in the following cases.

- The hotkeys are based on symbols, such as `?`, `@`, `#`, etc.
- The hotkeys don't mind whether the Shift key is pressed or not.

## Limitations

### No "Cheat Sheet"

Unlike ["angular-hotkeys"](https://github.com/chieffancypants/angular-hotkeys), this library doesn't provide "cheat sheet" feature, at this time.

Instead, the `HotKeysContext` object provides `HotKeyEntries` property, so you can implement your own "Cheat Sheet" UI, like this code:

```razor
<ul>
    @foreach (var key in _hotKeysContext.HotKeyEntries)
    {
        <li>@key</li>
    }
</ul>
```

The rendering result:

```
- Shift+Ctrl+A: do foo bar.
- ...
```

## Release Note

[Release notes](https://github.com/jsakamoto/Toolbelt.Blazor.HotKeys2/blob/master/RELEASE-NOTES.txt)

## License

[Mozilla Public License Version 2.0](https://github.com/jsakamoto/Toolbelt.Blazor.HotKeys2/blob/master/LICENSE)
