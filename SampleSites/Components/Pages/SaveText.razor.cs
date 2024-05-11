using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Toolbelt.Blazor.HotKeys2;

namespace SampleSite.Components.Pages;

public partial class SaveText : IAsyncDisposable
{
    [Inject] public HotKeys HotKeys { get; init; } = default!;

    [Inject] public IJSRuntime JS { get; init; } = default!;

    private HotKeysContext? _hotKeysContext;

    private ElementReference _inputElement;

    private string _inpuText = "";

    private readonly List<string> _savedTexts = new();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            this._hotKeysContext = this.HotKeys.CreateContext()
                .Add(ModCode.Ctrl, Code.S, this.OnSaveText, exclude: Exclude.TextArea);
            await this._inputElement.FocusAsync();
        }
    }

    private async ValueTask OnSaveText()
    {
        await this.JS.InvokeVoidAsync("Toolbelt.Blazor.fireOnChange", this._inputElement);

        this._savedTexts.Add(this._inpuText);
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        if (this._hotKeysContext is not null)
        {
            await this._hotKeysContext.DisposeAsync();
        }
    }
}
