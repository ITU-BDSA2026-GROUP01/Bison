using System.Diagnostics;
using Microsoft.Playwright;

namespace BisonE2ETest;

public class BisonAppFixture : IAsyncLifetime
{
    public const string BaseUrl = "http://127.0.0.1:5099";

    public string DbPath { get; } =
        Path.Combine(Path.GetTempPath(), $"bison-e2e-{Guid.NewGuid():N}.db");

    public IBrowser Browser { get; private set; } = null!;

    private Process? _server;
    private IPlaywright? _playwright;

    public async Task InitializeAsync()
    {
        var webProject = Path.Combine(FindSolutionRoot(), "src", "Bison.Web");

        var startInfo = new ProcessStartInfo("dotnet",
            $"run --project \"{webProject}\" --no-build --no-launch-profile --urls {BaseUrl}")
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        startInfo.Environment["BISONDBPATH"] = DbPath;
        startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";

        _server = Process.Start(startInfo)!;
        _server.OutputDataReceived += (_, _) => { };
        _server.ErrorDataReceived += (_, _) => { };
        _server.BeginOutputReadLine();
        _server.BeginErrorReadLine();

        await WaitForServerAsync();

        // installing some browser shit
        var installExitCode = Microsoft.Playwright.Program.Main(new[] { "install", "chromium" });
        if (installExitCode != 0)
            throw new InvalidOperationException($"Playwright browser install failed (exit code {installExitCode}).");


        _playwright = await Playwright.CreateAsync();
        Browser = await _playwright.Chromium.LaunchAsync();
    }

    public async Task DisposeAsync()
    {
        if (Browser != null) await Browser.CloseAsync();
        _playwright?.Dispose();

        if (_server is { HasExited: false })
            _server.Kill(entireProcessTree: true);
        _server?.Dispose();

        try { File.Delete(DbPath); } catch { }
    }

    private async Task WaitForServerAsync()
    {
        using var http = new HttpClient();
        var deadline = DateTime.UtcNow.AddSeconds(90);
        while (DateTime.UtcNow < deadline)
        {
            if (_server!.HasExited)
                throw new InvalidOperationException("The Bison app stopped before it was ready. Run 'dotnet build' first.");
            try
            {
                var response = await http.GetAsync(BaseUrl);
                if (response.IsSuccessStatusCode) return;
            }
            catch (HttpRequestException) { }
            await Task.Delay(500);
        }
        throw new TimeoutException("The Bison app did not start within 90 seconds.");
    }

    private static string FindSolutionRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "Bison.sln")))
            dir = dir.Parent;
        return dir?.FullName ?? throw new InvalidOperationException("Could not find Bison.sln.");
    }
}