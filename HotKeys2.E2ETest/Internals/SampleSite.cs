using Toolbelt.Diagnostics;
using static Toolbelt.Diagnostics.XProcess;

namespace Toolbelt.Blazor.HotKeys2.E2ETest;

public class SampleSite
{
    private readonly int ListenPort;

    private readonly string ProjectSubFolder;

    private readonly string TargetFramework;

    private XProcess? dotnetCLI;

    private WorkDirectory? WorkDir;

    public SampleSite(int listenPort, string projectSubFolder, string targetFramework)
    {
        this.ListenPort = listenPort;
        this.ProjectSubFolder = projectSubFolder;
        this.TargetFramework = targetFramework;
    }

    public string GetUrl() => $"http://localhost:{this.ListenPort}";

    internal string GetUrl(string subPath) => this.GetUrl() + "/" + subPath.TrimStart('/');

    public async ValueTask<SampleSite> StartAsync()
    {
        if (this.dotnetCLI != null) return this;

        var solutionDir = FileIO.FindContainerDirToAncestor("*.sln");
        var sampleSiteDir = Path.Combine(solutionDir, "SampleSites");
        this.WorkDir = WorkDirectory.CreateCopyFrom(sampleSiteDir, arg => arg.Name is not "obj" and not "bin");
        var projDir = Path.Combine(this.WorkDir, this.ProjectSubFolder);
        var publishDir = Path.Combine(projDir, "bin", "Release", this.TargetFramework, "publish");
        var wwwroothDir = Path.Combine(publishDir, "wwwroot");

        // Publish and...
        using var publishCommand = await Start(
            "dotnet",
            $"publish -f:{this.TargetFramework} -c:Release -p:BlazorEnableCompression=false -p:UsingBrowserRuntimeWorkload=false",
            projDir)
            .WaitForExitAsync();
        publishCommand.ExitCode.Is(0);

        // Serve it.
        var serverDllName = $"SampleSite.{this.ProjectSubFolder}.dll";

        // ... for Blazor Server
        if (File.Exists(Path.Combine(publishDir, serverDllName)))
            this.dotnetCLI = Start("dotnet", $"exec \"{serverDllName}\" --urls {this.GetUrl()}", publishDir);

        // ... for Blazor WebAssembly
        else
            this.dotnetCLI = Start("dotnet", $"serve -d:\"{wwwroothDir}\" -p:{this.ListenPort}", projDir);

        // Wait for listening
        var success = await this.dotnetCLI.WaitForOutputAsync(output => output.Contains(this.GetUrl()), millsecondsTimeout: 15000);
        if (!success)
        {
            try { this.dotnetCLI.Dispose(); } catch { }
            var output = this.dotnetCLI.Output;
            this.dotnetCLI = null;
            throw new TimeoutException($"\"dotnet run\" did not respond \"Now listening on: {this.GetUrl()}\".\r\n" + output);
        }

        Thread.Sleep(200);
        return this;
    }

    public void Stop()
    {
        this.dotnetCLI?.Dispose();
        this.WorkDir?.Dispose();
    }
}
