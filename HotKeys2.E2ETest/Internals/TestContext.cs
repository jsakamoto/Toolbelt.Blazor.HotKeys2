using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using Toolbelt.Blazor.HotKeys2.E2ETest.Internals;

namespace Toolbelt.Blazor.HotKeys2.E2ETest;

[SetUpFixture]
public class TestContext
{
    public static TestContext Instance { get; private set; } = null!;

    private readonly IReadOnlyDictionary<HostingModel, SampleSite> SampleSites = new Dictionary<HostingModel, SampleSite> {
            { HostingModel.Wasm50, new SampleSite(5012, "Client", "net5.0") },
            { HostingModel.Wasm60, new SampleSite(5013, "Client", "net6.0") },
            { HostingModel.Server50, new SampleSite(5015, "Server", "net5.0") },
            { HostingModel.Server60, new SampleSite(5016, "Server", "net6.0") },
        };

    private IPlaywright? _Playwrite;

    private IBrowser? _Browser;

    private IPage? _Page;

    private class TestOptions
    {
        public string Browser { get; set; } = "";

        public bool Headless { get; set; } = true;
    }

    private readonly TestOptions _Options = new();

    public ValueTask<SampleSite> StartHostAsync(HostingModel hostingModel)
    {
        return this.SampleSites[hostingModel].StartAsync();
    }

    public string GetHostUrl(HostingModel hostingModel)
    {
        return this.SampleSites[hostingModel].GetUrl();
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

        Microsoft.Playwright.Program.Main(new[] { "install" });
    }

    public async ValueTask<IPage> GetPageAsync()
    {
        this._Playwrite ??= await Playwright.CreateAsync();
        this._Browser ??= await this.LaunchBrowserAsync(this._Playwrite);
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
        this._Playwrite?.Dispose();
        Parallel.ForEach(this.SampleSites.Values, sampleSite => sampleSite.Stop());
    }
}
