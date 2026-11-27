using Bison.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;

namespace BisonE2ETest;

public class SecurityE2ETests : IClassFixture<BisonAppFixture>
{
    private readonly BisonAppFixture _app;

    public SecurityE2ETests(BisonAppFixture app)
    {
        _app = app;
    }

    // Same helpers as in ObservationE2ETests
    private async Task<IPage> NewPageAsync()
    {
        var context = await _app.Browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = BisonAppFixture.BaseUrl
        });
        context.SetDefaultTimeout(60_000);
        return await context.NewPageAsync();
    }

    private static async Task LoginAsync(IPage page, string email, string password)
    {
        await page.GotoAsync("/Identity/Account/Login");
        await page.FillAsync("#Input_Email", email);
        await page.FillAsync("#Input_Password", password);
        await page.ClickAsync("#login-submit");
        await Assertions.Expect(page.Locator("select[name=NewTaxonId]"))
            .ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = 60_000 });
    }

    private static async Task ShareObservationAsync(IPage page, string text)
    {
        await page.GotoAsync("/");
        await page.FillAsync("input[name=Text]", text);
        await page.SelectOptionAsync("select[name=NewTaxonId]", new SelectOptionValue { Index = 1 });
        await page.ClickAsync("input[type=submit][value=Share]");
        await Assertions.Expect(page.GetByText(text)).ToBeVisibleAsync();
    }

    [Theory]
    [InlineData("<script>alert('xss')</script>")]
    [InlineData("<img src=x onerror=alert('xss')>")]
    [InlineData("<svg onload=alert('xss')>")]
    public async Task Observation_with_html_payload_is_shown_as_text_and_never_executed(string payload)
    {
        var page = await NewPageAsync();
        var dialogFired = false;
        page.Dialog += async (_, dialog) => { dialogFired = true; await dialog.DismissAsync(); };

        await LoginAsync(page, "edka@itu.dk", "123456");
        var text = payload + " " + Guid.NewGuid();   // unique so we find exactly our post

        await ShareObservationAsync(page, text);
        await page.ReloadAsync();                    // render it again from the database

        // 1. Visible as literal text
        var shown = page.GetByText(text);
        await Assertions.Expect(shown).ToBeVisibleAsync();
        await Assertions.Expect(shown).ToContainTextAsync(payload);

        // 2. Encoded in the DOM: the angle brackets are entities, not real tags
        var html = await shown.First.InnerHTMLAsync();
        Assert.Contains("&lt;", html);

        // 3. No injected element exists
        var injected = page.Locator("img[onerror], svg[onload], script:has-text(\"alert('xss')\")");
        Assert.Equal(0, await injected.CountAsync());

        // 4. Nothing executed (onerror fires asynchronously, so give it a moment)
        await page.WaitForTimeoutAsync(500);
        Assert.False(dialogFired, "A JavaScript dialog opened, so the payload was executed.");
    }

    [Theory]
    [InlineData("' OR 1=1 --")]
    [InlineData("'); DROP TABLE Posts; --")]
    public async Task Observation_with_sql_payload_is_stored_as_literal_text(string payload)
    {
        var page = await NewPageAsync();
        await LoginAsync(page, "edka@itu.dk", "123456");
        var text = payload + " " + Guid.NewGuid();

        var options = new DbContextOptionsBuilder<BisonDBContext>()
            .UseSqlite($"Data Source={_app.DbPath}").Options;

        int before;
        using (var db = new BisonDBContext(options))
            before = db.Observations.Count();

        await ShareObservationAsync(page, text);

        using (var db = new BisonDBContext(options))
        {
            // exactly one new row, stored with the payload intact, and the table survived
            Assert.Equal(before + 1, db.Observations.Count());
            Assert.Equal(text, db.Observations.Single(o => o.Text == text).Text);
        }

        await page.GotoAsync("/");
        await Assertions.Expect(page.GetByText(text)).ToBeVisibleAsync();
    }
}