using Bison.Razor.Pages;
using Bison.Razor.Services;
using BisonTest;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebserverTest;

/// <summary>
/// Tests the UserTimelineModel page (per-author observation feed)
/// against a hermetic SQLite DB.
/// Real seed: Eduard owns obs1 ("A heron"), Peter owns obs2 ("A big bird").
/// </summary>
public class UserTimelineModelTests : IDisposable
{
    private readonly SQLiteDatabaseTestHelper _db;
    private readonly UserTimelineModel _model;
    private int _obs1, _obs2;

    public UserTimelineModelTests()
    {
        _db = new SQLiteDatabaseTestHelper();
        (_, _, _obs1, _obs2) = _db.SeedStandard();
        _model = new UserTimelineModel(new ObservationService(_db.CreateFacade()));
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public void OnGet_ReturnsPageResult()
    {
        var result = _model.OnGet("Eduard");

        Assert.IsType<PageResult>(result);
    }

    [Fact]
    public void OnGet_Eduard_ReturnsOnlyHisObservation()
    {
        _model.OnGet("Eduard");

        Assert.Single(_model.Cheeps);
        Assert.Equal(_obs1, _model.Cheeps[0].Id);
        Assert.Equal("Eduard", _model.Cheeps[0].Author);
    }

    [Fact]
    public void OnGet_Peter_ReturnsOnlyHisObservation()
    {
        _model.OnGet("Peter");

        Assert.Single(_model.Cheeps);
        Assert.Equal(_obs2, _model.Cheeps[0].Id);
        Assert.Equal("Peter", _model.Cheeps[0].Author);
    }

    [Fact]
    public void OnGet_UnknownAuthor_CheepsIsEmpty()
    {
        _model.OnGet("NobodyHere");

        Assert.Empty(_model.Cheeps);
    }

    [Fact]
    public void OnGet_WithPage_RespectsPagination()
    {
        // Author with 33 observations → page 1 holds 32, page 2 holds 1.
        int big = _db.InsertUser("BigAuthor", "b@x.dk");
        for (int i = 0; i < 33; i++)
            _db.InsertObservation(big, $"bulk {i}", 1683000000 + i);

        _model.OnGet("BigAuthor", page: 1);
        Assert.Equal(32, _model.Cheeps.Count);

        _model.OnGet("BigAuthor", page: 2);
        Assert.Single(_model.Cheeps);
    }

    [Fact]
    public void OnGet_PageBeyondData_CheepsIsEmpty()
    {
        _model.OnGet("Eduard", page: 99);

        Assert.Empty(_model.Cheeps);
    }

    [Fact]
    public void OnGet_CheepsHaveAuthorMessageAndTimestamp()
    {
        _model.OnGet("Eduard");

        Assert.All(_model.Cheeps, o =>
        {
            Assert.Equal("Eduard", o.Author);
            Assert.False(string.IsNullOrEmpty(o.Message));
            Assert.False(string.IsNullOrEmpty(o.Timestamp));
        });
    }
}
