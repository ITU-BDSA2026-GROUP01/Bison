using Bison.Razor.Pages;
using Bison.Razor.Services;
using BisonTest;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebserverTest;

/// <summary>
/// Tests the PublicModel page (home/feed page) against a hermetic SQLite DB.
/// PublicModel.OnGet(page) → Cheeps property, returns a PageResult.
/// </summary>
public class PublicModelTests : IDisposable
{
    private readonly SQLiteDatabaseTestHelper _db;
    private readonly PublicModel _model;
    private int _obs1, _obs2;

    public PublicModelTests()
    {
        _db = new SQLiteDatabaseTestHelper();
        (_, _, _obs1, _obs2) = _db.SeedStandard();
        _model = new PublicModel(new PostService(_db.CreatePostRepository()));
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public void OnGet_ReturnsPageResult()
    {
        var result = _model.OnGet();

        Assert.IsType<PageResult>(result);
    }

    [Fact]
    public void OnGet_PopulatesCheepsWithAllSeeded_NewestFirst()
    {
        _model.OnGet();

        Assert.Equal(2, _model.Cheeps.Count);
        Assert.Equal(_obs2, _model.Cheeps[0].Id);
        Assert.Equal(_obs1, _model.Cheeps[1].Id);
    }

    [Fact]
    public void OnGet_EachCheepHasAuthorMessageAndTimestamp()
    {
        _model.OnGet();

        Assert.All(_model.Cheeps, o =>
        {
            Assert.False(string.IsNullOrEmpty(o.Author));
            Assert.False(string.IsNullOrEmpty(o.Message));
            Assert.False(string.IsNullOrEmpty(o.Timestamp));
        });
    }

    [Fact]
    public void OnGet_WithPageParameter_RespectsPagination()
    {
        // 2 seeded; add 32 → 34 total.
        int big = _db.InsertUser("BigGuy", "big@x.dk");
        for (int i = 0; i < 32; i++)
            _db.InsertObservation(big, $"bulk {i}", 1686000000 + i);

        _model.OnGet(page: 1);
        Assert.Equal(32, _model.Cheeps.Count);

        _model.OnGet(page: 2);
        Assert.Equal(2, _model.Cheeps.Count);
    }

    [Fact]
    public void OnGet_PageBeyondData_CheepsIsEmpty()
    {
        _model.OnGet(page: 99);

        Assert.Empty(_model.Cheeps);
    }
}
