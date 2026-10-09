using Bison.Razor;
using BisonTest;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using System.Runtime.InteropServices;

namespace WebserverTest;

/// <summary>
/// HTTP API tests: send real GET requests to the public timeline (/) and
/// private timeline (/{author}) endpoints via WebApplicationFactory, and
/// assert the rendered HTML body contains the expected cheep data.
///
/// The app is pointed at a hermetic temp database (via the BISONDBPATH
/// environment variable — which Program.cs honours) so it reads exactly the
/// standard seed the suite has always expected:
///   Eduard → "A heron"
///   Peter  → "A big bird"
/// BISON_SEED=false disables the app's 400+ post seed so it doesn't clobber
/// the test fixture.
/// </summary>
public class TimelineApiTests : IDisposable
{
    private readonly SQLiteDatabaseTestHelper _db;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private string? _oldBisonDbPath;

    public TimelineApiTests()
    {
        _db = new SQLiteDatabaseTestHelper();
        _db.SeedStandard();

        // Point the app at our hermetic temp DB and disable its 400+ post seed
        // so the test fixture is what the app reads.
        _oldBisonDbPath = Environment.GetEnvironmentVariable("BISONDBPATH");
        Environment.SetEnvironmentVariable("BISONDBPATH", _db.DbPath);
        Environment.SetEnvironmentVariable("BISON_SEED", "false");

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((_, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["BISONDBPATH"] = _db.DbPath,
                        ["BISON_SEED"] = "false",
                        // Silence EF Core's per-statement SQL logging (it logs
                        // at Information level, which the app's Default=Information
                        // lets through) so test output stays readable.
                        ["Logging:LogLevel:Microsoft.EntityFrameworkCore"] = "Warning"
                    });
                });
            });

        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
        // Restore the environment so we don't leak test state into other tests.
        Environment.SetEnvironmentVariable("BISONDBPATH", _oldBisonDbPath);
        Environment.SetEnvironmentVariable("BISON_SEED", null);
        _db.Dispose();
    }

    [Fact]
    public async Task PublicTimeline_ContainsPeterCheep()
    {
        var response = await _client.GetAsync("/");
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("Peter", html);
        Assert.Contains("A big bird", html);
    }

    [Fact]
    public async Task PublicTimeline_ContainsEduardCheep()
    {
        var response = await _client.GetAsync("/");
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("Eduard", html);
        Assert.Contains("A heron", html);
    }

    [Fact]
    public async Task PrivateTimeline_Eduard_ContainsHeronCheep()
    {
        var response = await _client.GetAsync("/Eduard");
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("Eduard", html);
        Assert.Contains("A heron", html);
        // Peter's cheep must NOT appear on Eduard's private timeline.
        Assert.DoesNotContain("A big bird", html);
    }

    [Fact]
    public async Task PrivateTimeline_Peter_ContainsBigBirdCheep()
    {
        var response = await _client.GetAsync("/Peter");
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("Peter", html);
        Assert.Contains("A big bird", html);
        // Eduard's cheep must NOT appear on Peter's private timeline.
        Assert.DoesNotContain("A heron", html);
    }

    [Fact]
    public async Task PrivateTimeline_UnknownAuthor_ShowsEmptyMessage()
    {
        var response = await _client.GetAsync("/NobodyHere");
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("There are no cheeps so far.", html);
    }
}
