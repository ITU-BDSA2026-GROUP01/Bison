using Bison.Razor.Pages;
using BisonTest;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebserverTest;

/// <summary>
/// Tests the ObservationModel page (single-observation detail + list-when-no-id)
/// against a hermetic SQLite DB.
/// The real seed (Data/dump.sql) puts one comment and one proposal (both by
/// "Rats") on obs1 (Eduard's "A heron"); obs2 (Peter's) has neither.
/// </summary>
public class ObservationModelTests : IDisposable
{
    private readonly SQLiteDatabaseTestHelper _db;
    private readonly ObservationModel _model;
    private int _obs1, _obs2;

    public ObservationModelTests()
    {
        _db = new SQLiteDatabaseTestHelper();
        (_, _, _obs1, _obs2) = _db.SeedStandard();
        _model = new ObservationModel(_db.CreateFacade());
    }

    public void Dispose() => _db.Dispose();

    // ── OnGet with id → detail view ───────────────────────────────────

    [Fact]
    public void OnGet_WithId_ReturnsPageResult()
    {
        var result = _model.OnGet(id: _obs1);

        Assert.IsType<PageResult>(result);
    }

    [Fact]
    public void OnGet_WithId_PopulatesObservation()
    {
        _model.OnGet(id: _obs1);

        Assert.NotNull(_model.Observation);
        Assert.Equal(_obs1, _model.Observation!.Id);
        Assert.Equal("Eduard", _model.Observation.Author);
        Assert.Equal("A heron", _model.Observation.Message);
    }

    [Fact]
    public void OnGet_WithId_PopulatesSeededComment()
    {
        _model.OnGet(id: _obs1);

        Assert.Single(_model.Comments);
        Assert.Equal(_obs1, _model.Comments[0].ObservationId);
        Assert.Equal("Rats", _model.Comments[0].Author);
        Assert.Equal("wow amazing", _model.Comments[0].Message);
    }

    [Fact]
    public void OnGet_WithId_PopulatesSeededProposal()
    {
        _model.OnGet(id: _obs1);

        Assert.Single(_model.Proposals);
        Assert.Equal(_obs1, _model.Proposals[0].ObservationId);
        Assert.Equal("Rats", _model.Proposals[0].Author);
        Assert.StartsWith("MSTSNM:Arter:", _model.Proposals[0].TaxonId);
    }

    [Fact]
    public void OnGet_WithId_ObservationWithoutCommentsOrProposals_ReturnsEmptyLists()
    {
        _model.OnGet(id: _obs2);

        Assert.NotNull(_model.Observation);
        Assert.Empty(_model.Comments);
        Assert.Empty(_model.Proposals);
    }

    [Fact]
    public void OnGet_WithUnknownId_ReturnsNotFound()
    {
        var result = _model.OnGet(id: 999999);

        var nfr = Assert.IsType<NotFoundResult>(result);
        Assert.Equal(404, nfr.StatusCode);
    }

    [Fact]
    public void OnGet_WithId_MultipleCommentsAndProposals_AllReturned()
    {
        _db.InsertComment(_obs1, "Eduard", "second comment", 1690899200);
        _db.InsertProposal(_obs1, "Eduard", "9179", 1690899300);

        _model.OnGet(id: _obs1);

        Assert.Equal(2, _model.Comments.Count);
        Assert.Equal(2, _model.Proposals.Count);
    }

    // ── OnGet without id → list view ──────────────────────────────────

    [Fact]
    public void OnGet_WithoutId_ReturnsPageResult()
    {
        var result = _model.OnGet(id: null);

        Assert.IsType<PageResult>(result);
    }

    [Fact]
    public void OnGet_WithoutId_PopulatesObservationsList()
    {
        _model.OnGet(id: null);

        Assert.Equal(2, _model.Observations.Count);
        Assert.Equal(_obs2, _model.Observations[0].Id);   // newest first
    }

    [Fact]
    public void OnGet_WithoutId_RespectsPagination()
    {
        // 2 seeded; add 32 → 34 total.
        int big = _db.InsertUser("BigGuy", "big@x.dk");
        for (int i = 0; i < 32; i++)
            _db.InsertObservation(big, $"bulk {i}", 1685000000 + i);

        _model.OnGet(id: null, page: 1);
        Assert.Equal(32, _model.Observations.Count);

        _model.OnGet(id: null, page: 2);
        Assert.Equal(2, _model.Observations.Count);
    }

    [Fact]
    public void OnGet_WithoutId_PageBeyondData_ObservationsIsEmpty()
    {
        _model.OnGet(id: null, page: 99);

        Assert.Empty(_model.Observations);
    }
}
