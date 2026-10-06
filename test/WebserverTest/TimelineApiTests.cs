using Bison.Razor;
using BisonTest;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace WebserverTest;

/// <summary>
/// HTTP API tests: send real GET requests to the public timeline (/) and
/// private timeline (/{author}) endpoints via WebApplicationFactory, and
/// assert the rendered HTML body contains the expected cheep data.
///
/// Canonical seed (mirrors Data/dump.sql):
///   Eduard → "A heron"
///   Peter  → "A big bird"
/// </summary>
public class TimelineApiTests : IDisposable
{
    private readonly SQLiteDatabaseTestHelper _db;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public TimelineApiTests()
    {
        _db = new SQLiteDatabaseTestHelper();
        _db.SeedStandard();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((_, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["BISONDBPATH"] = _db.DbPath
                    });
                });
            });

        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
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
