# Blazor HotKeys2 [![NuGet Package](https://img.shields.io/nuget/v/Toolbelt.Blazor.HotKeys2.svg)](https://www.nuget.org/packages/Toolbelt.Blazor.HotKeys2/)

## Summary

This is a class library that provides configuration-centric keyboard shortcuts for your Blazor apps.  
**(This library is a successor of [the "Blazor HotKeys" library](https://github.com/jsakamoto/Toolbelt.Blazor.HotKeys).)**

![movie](https://raw.githubusercontent.com/jsakamoto/Toolbelt.Blazor.HotKeys2/master/.assets/movie-001.gif)

- [Live demo](https://jsakamoto.github.io/Toolbelt.Blazor.HotKeys2/)

You can declare associations of keyboard shortcut and callback action, like this code:

```csharp
// The method "OnSelectAll" will be invoked 
//  when the user typed Ctrl+A key combination.
this.HotKeysContext = this.HotKeys.CreateContext()
  .Add(ModCode.Ctrl, Code.A, OnSelectAll)
  .Add(...)
  ...;
```

## Supported Blazor versions

This library suppots ASP.NET Core Blazor version 5.0 or later.

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

**Step.1** Implement `IDisposable` interface to the component.

```razor
@implements IDisposable @* 👈 Add this at the top of the component.  *@
...

@code {
  ...
  public void Dispose() // 👈 Add "Dispose" method.
  {
  }
}
```

**Step.2** Open the `Toolbelt.Blazor.HotKeys2` namespace, and inject the `HotKeys` service into the component.

```razor
@implements IDisposable
@using Toolbelt.Blazor.HotKeys2 @* 👈 1. Add this *@
@inject HotKeys HotKeys @* 2. 👈 Add this *@
...
```

**Step.3** Invoke `CreateContext()` method of the `HotKeys` service instance to create and activate hot keys entries at startup of the component such as `OnInitialized()` method.

You can add the combination with key and action to the `HotKeysContext` object that is returned from `CreateContext()` method, using `Add()` method.

Please remember that you have to keep the `HotKeys Context` object in the component field.

```csharp
@code {

  HotKeysContext HotKeysContext;

  protected override void OnInitialized()
  {
    this.HotKeysContext = this.HotKeys.CreateContext()
      .Add(ModCode.Ctrl|ModCode.Shift, Code.A, FooBar, "do foo bar.")
      .Add(...)
      ...;
  }

  void FooBar() // 👈 This will be invoked when Ctrl+Shift+A typed.
  {
    ...
  }
}
```

> **Note**  
> You can also specify the async method to the callback action argument.

> **Note**  
> The method of the callback action can take an argument which is `HotKeyEntry` object.


**Step.4** Dispose the `HotKeysContext` object when the component is disposing, in the `Dispose()` method of the component.

```csharp
@code {
  ...
  public void Dispose()
  {
    this.HotKeysContext.Dispose(); // 👈 1. Add this
  }
}
```

The complete source code (.razor) of this component is bellow.

```csharp
@page "/"
@implements IDisposable
@using Toolbelt.Blazor.HotKeys2
@inject HotKeys HotKeys

@code {

  HotKeysContext HotKeysContext;

  protected override void OnInitialized()
  {
    this.HotKeysContext = this.HotKeys.CreateContext()
      .Add(ModCode.Ctrl|ModCode.Shift, Code.A, FooBar, "do foo bar.")
  }

  void FooBar()
  {
    // Do something here.
  }

  public void Dispose()
  {
    this.HotKeysContext.Dispose();
  }
}
```
### How to enable / disable hotkeys depending on which element has focus

You can specify enabling/disabling hotkeys depending on which kind of element has focus at the hotkeys registration via a combination of the `Exclude` flags in optional arguments of the `HotKeysContext.Add()` method.

By default, the `Exclude` flags argument is the following combination.

```csharp
Exclude.InputText | Exclude.InputNonText | Exclude.TextArea
```

This means, by default, hotkeys are disabled when the focus is in `<input>` (with any `type`) or `<textarea>` elements.

If you want to enable hotkeys even when an `<input type="text"/>` has focus, you can do it as below.

```csharp
... this.HotKeys.CreateContext()
  .Add(Code.A, OnKeyDownA, "...", 
    // 👇 Specify the "exclude" argument.
    exclude: Exclude.InputNonText | Exclude.TextArea)
  ...
```

And you can specify the `Exclude.ContentEditable` to register the unavailable hotkey when any "contenteditable" applied elements have focus.

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

Instead, the `HotKeysContext` object provides `Keys` property, so you can implement your own "Cheat Sheet" UI, like this code:

```razor
<ul>
    @foreach (var key in this.HotKeysContext.Keys)
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
