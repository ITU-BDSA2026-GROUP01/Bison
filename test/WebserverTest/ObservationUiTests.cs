using System.Net;
using System.Text.RegularExpressions;
using Bison.Core;
using Bison.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace WebserverTest;

public class ObservationUiTests : IClassFixture<BisonFactory>
{
    private const string PublicUrl = "/";
    private const string UserEmail = "eduard@test.dk";

    private readonly BisonFactory _factory;

    public ObservationUiTests(BisonFactory factory)
    {
        _factory = factory;
    }

    private HttpClient NewClient(bool loggedIn)
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        if (loggedIn)
            client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderName, UserEmail);
        return client;
    }

    private int EnsureTaxon()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BisonDBContext>();
        var taxon = db.Taxons.FirstOrDefault(t => t.dwc_TaxonID == "ui-test-heron");
        if (taxon == null)
        {
            taxon = new Taxon { dwc_TaxonID = "ui-test-heron", VernacularName = "UI test heron" };
            db.Taxons.Add(taxon);
            db.SaveChanges();
        }
        return taxon.TaxonId;
    }

    private static string GetToken(string html) =>
        Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value;


    private async Task<HttpResponseMessage> PostObservationAsync(HttpClient client, string text, int? taxonId)
    {
        var html = await client.GetStringAsync(PublicUrl);

        var form = new List<KeyValuePair<string, string>>
        {
            new("Text", text),
            new("__RequestVerificationToken", GetToken(html))
        };
        if (taxonId.HasValue)
            form.Add(new("NewTaxonId", taxonId.Value.ToString()));

        return await client.PostAsync(PublicUrl, new FormUrlEncodedContent(form));
    }

    [Fact]
    public async Task ObservationBox_IsHidden_WhenLoggedOut()
    {
        var html = await NewClient(loggedIn: false).GetStringAsync(PublicUrl);

        Assert.DoesNotContain("name=\"NewTaxonId\"", html);
    }

    [Fact]
    public async Task ObservationBox_IsShown_WhenLoggedIn()
    {
        var html = await NewClient(loggedIn: true).GetStringAsync(PublicUrl);

        Assert.Contains("name=\"NewTaxonId\"", html);
    }

    [Fact]
    public async Task Post_WhenLoggedOut_IsNotAccepted()
    {
        var taxonId = EnsureTaxon();
        var html = await NewClient(loggedIn: true).GetStringAsync(PublicUrl);

        var response = await NewClient(loggedIn: false).PostAsync(PublicUrl, new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Text", "logged out text"),
            new KeyValuePair<string, string>("NewTaxonId", taxonId.ToString()),
            new KeyValuePair<string, string>("__RequestVerificationToken", GetToken(html))
        }));

        Assert.NotEqual(HttpStatusCode.Redirect, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BisonDBContext>();
        Assert.False(db.Observations.Any(o => o.Text == "logged out text"));
    }

    [Fact]
    public async Task Post_EmptyText_ShowsErrorMessage()
    {
        var taxonId = EnsureTaxon();

        var response = await PostObservationAsync(NewClient(loggedIn: true), "   ", taxonId);
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Write something first", html);
    }

    [Fact]
    public async Task Post_WithoutTaxon_ShowsErrorMessage()
    {
        var response = await PostObservationAsync(NewClient(loggedIn: true), "A heron by the lake", null);
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("choose a taxon", html);
    }

    [Fact]
    public async Task Post_Valid_RedirectsAndStoresObservationForAuthor()
    {
        var taxonId = EnsureTaxon();
        var text = "UI test observation " + Guid.NewGuid();

        var response = await PostObservationAsync(NewClient(loggedIn: true), text, taxonId);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BisonDBContext>();
        var saved = db.Observations.Include(o => o.Author).Single(o => o.Text == text);
        Assert.Equal(UserEmail, saved.Author.Email);
        Assert.Equal(taxonId, saved.TaxonId);
    }
}