using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using Toolbelt.Blazor.HotKeys2.E2ETest.Internals;

namespace Toolbelt.Blazor.HotKeys2.E2ETest;

[SetUpFixture]
public class TestContext
{
    public static TestContext Instance { get; private set; } = null!;

    private readonly IReadOnlyDictionary<HostingModel, SampleSite> SampleSites = new Dictionary<HostingModel, SampleSite>
    {
        [HostingModel.Wasm80] = new("Client", "net8.0"),
        [HostingModel.Wasm90] = new("Client", "net9.0"),
        [HostingModel.Wasm100] = new("Client", "net10.0"),
        [HostingModel.Server80] = new("Server8", "net8.0"),
        [HostingModel.Server90] = new("Server8", "net9.0"),
        [HostingModel.Server100] = new("Server8", "net10.0"),
    };

    private IPlaywright? _Playwright;

    private IBrowser? _Browser;

    private IPage? _Page;

    private class TestOptions
    {
        public string Browser { get; set; } = "";

        public bool Headless { get; set; } = true;

        public bool SkipInstallBrowser { get; set; } = false;
    }

    private readonly TestOptions _Options = new();

    public ValueTask<SampleSite> StartHostAsync(HostingModel hostingModel)
    {
        return this.SampleSites[hostingModel].StartAsync();
    }

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables(prefix: "DOTNET_")
            .AddTestParameters()
            .Build();
        configuration.Bind(this._Options);

        Instance = this;

        if (!this._Options.SkipInstallBrowser)
        {
            Microsoft.Playwright.Program.Main(["install"]);
        }
    }

    public async ValueTask<IPage> GetPageAsync()
    {
        this._Playwright ??= await Playwright.CreateAsync();
        this._Browser ??= await this.LaunchBrowserAsync(this._Playwright);
        this._Page ??= await this._Browser.NewPageAsync();
        return this._Page;
    }

    private Task<IBrowser> LaunchBrowserAsync(IPlaywright playwright)
    {
        var browserType = this._Options.Browser.ToLower() switch
        {
            "firefox" => playwright.Firefox,
            "webkit" => playwright.Webkit,
            _ => playwright.Chromium
        };

        var channel = this._Options.Browser.ToLower() switch
        {
            "firefox" or "webkit" => "",
            _ => this._Options.Browser.ToLower()
        };

        return browserType.LaunchAsync(new()
        {
            Channel = channel,
            Headless = this._Options.Headless,
        });
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDownAsync()
    {
        if (this._Browser != null) await this._Browser.DisposeAsync();
        this._Playwright?.Dispose();
        Parallel.ForEach(this.SampleSites.Values, sampleSite => sampleSite.Stop());
    }
}
