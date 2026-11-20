using Bison.Infrastructure.Data;
using Deque.AxeCore.Commons;
using Deque.AxeCore.Playwright;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;

namespace BisonE2ETest;

public class ObservationE2ETests : IClassFixture<BisonAppFixture>
{
    private readonly BisonAppFixture _app;

    public ObservationE2ETests(BisonAppFixture app)
    {
        _app = app;
    }

    private async Task<IPage> NewPageAsync()
    {
        var context = await _app.Browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = BisonAppFixture.BaseUrl
        });
        return await context.NewPageAsync();
    }

    private static async Task LoginAsync(IPage page, string email, string password)
    {
        await page.GotoAsync("/Identity/Account/Login");
        await page.FillAsync("#Input_Email", email);
        await page.FillAsync("#Input_Password", password);
        await page.ClickAsync("#login-submit");
        await page.WaitForURLAsync(url => !url.Contains("/Identity/Account/Login"));
    }

    private static async Task ShareObservationAsync(IPage page, string text)
    {
        await page.GotoAsync("/");
        await page.FillAsync("input[name=Text]", text);
        await page.SelectOptionAsync("select[name=NewTaxonId]", new SelectOptionValue { Index = 1 });
        await page.ClickAsync("input[type=submit][value=Share]");
        await Assertions.Expect(page.GetByText(text)).ToBeVisibleAsync();
    }

    private static async Task AssertNoAxeViolationsAsync(IPage page)
    {
        var results = await page.RunAxe(new AxeRunOptions
        {
            RunOnly = new RunOnlyOptions
            {
                Type = "tag",
                Values = new List<string> { "wcag2a", "wcag2aa" }
            }
        });

                var summary = string.Join("\n", results.Violations.Select(v =>
            $"- {v.Id} ({v.Impact}): {v.Help} [{v.Nodes.Count()} element(s)]\n" +
            string.Join("\n", v.Nodes.Take(5).Select(n =>
                "    " + (n.Html.Length > 120 ? n.Html[..120] : n.Html)))));
        Assert.True(!results.Violations.Any(), "Axe found accessibility problems:\n" + summary);
    }

    [Fact]
    public async Task ObservationBox_IsHidden_WhenLoggedOut()
    {
        var page = await NewPageAsync();

        await page.GotoAsync("/");

        await Assertions.Expect(page.Locator("select[name=NewTaxonId]")).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task LoggedInUser_CanCreateObservation_AndSeeItInThePublicList()
    {
        var page = await NewPageAsync();
        await LoginAsync(page, "edka@itu.dk", "123456");
        var text = "E2E heron " + Guid.NewGuid();

        await ShareObservationAsync(page, text);

        await page.ReloadAsync();
        await Assertions.Expect(page.GetByText(text)).ToBeVisibleAsync();
    }

    [Fact]
    public async Task CreatedObservation_IsStoredInDatabase_ForTheAuthor()
    {
        var page = await NewPageAsync();
        await LoginAsync(page, "edka@itu.dk", "123456");
        var text = "E2E stored " + Guid.NewGuid();

        await ShareObservationAsync(page, text);

        var options = new DbContextOptionsBuilder<BisonDBContext>()
            .UseSqlite($"Data Source={_app.DbPath}").Options;
        using (var db = new BisonDBContext(options))
        {
            var saved = db.Observations.Include(o => o.Author).Single(o => o.Text == text);
            Assert.Equal("Eduard", saved.Author.Name);
        }

        await page.GotoAsync("/Eduard");
        await Assertions.Expect(page.GetByText(text)).ToBeVisibleAsync();

        await page.GotoAsync("/Debasmita");
        await Assertions.Expect(page.GetByText(text)).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task PublicPage_LoggedOut_HasNoAccessibilityViolations()
    {
        var page = await NewPageAsync();
        await page.GotoAsync("/");

        await AssertNoAxeViolationsAsync(page);
    }

    [Fact]
    public async Task LoginPage_HasNoAccessibilityViolations()
    {
        var page = await NewPageAsync();
        await page.GotoAsync("/Identity/Account/Login");

        await AssertNoAxeViolationsAsync(page);
    }

    [Fact]
    public async Task PublicPage_WithObservationBox_HasNoAccessibilityViolations()
    {
        var page = await NewPageAsync();
        await LoginAsync(page, "edka@itu.dk", "123456");
        await page.GotoAsync("/");

        await AssertNoAxeViolationsAsync(page);
    }
}