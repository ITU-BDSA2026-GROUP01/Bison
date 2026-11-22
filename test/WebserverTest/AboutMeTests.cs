using System.Net;
using Bison.Web;
using BisonTest;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace WebserverTest;

public class AboutMeTests : IDisposable
{
    private readonly SQLiteDatabaseTestHelper _db;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly string? _oldBisonDbPath;

    public AboutMeTests()
    {
        _db = new SQLiteDatabaseTestHelper();
        _db.SeedStandard();

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
                        ["Logging:LogLevel:Microsoft.EntityFrameworkCore"] = "Warning"
                    });
                });
            });

        // Don't follow redirects, so we can inspect where an anonymous user is sent.
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
        Environment.SetEnvironmentVariable("BISONDBPATH", _oldBisonDbPath);
        Environment.SetEnvironmentVariable("BISON_SEED", null);
        _db.Dispose();
    }

    [Fact]
    public async Task Anonymous_AboutMe_RedirectsToLogin()
    {
        var response = await _client.GetAsync("/aboutme");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Identity/Account/Login", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Anonymous_PublicPage_DoesNotShowAboutMeLink()
    {
        var response = await _client.GetAsync("/");
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain("about me", html);
    }
}