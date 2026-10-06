using Bison.Razor.Pages;
using Bison.Razor.Services;
using BisonTest;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BisonE2ETest;

/// <summary>
/// End-to-end read pipeline tests: full schema + seed data → DBFacade →
/// services → page models. Verifies the whole read path works together
/// without a live web server, against a hermetic SQLite DB.
/// </summary>
public class E2EReadPipelineTests : IDisposable
{
    private readonly SQLiteDatabaseTestHelper _db;
    private readonly DBFacade _facade;
    private readonly ObservationService _obsSvc;
    private readonly PostService _postSvc;
    private int _obs1, _obs2;

    public E2EReadPipelineTests()
    {
        _db = new SQLiteDatabaseTestHelper();
        (_, _, _obs1, _obs2) = _db.SeedStandard();
        _facade = _db.CreateFacade();
        var repo = _db.CreatePostRepository();
        _obsSvc = new ObservationService(repo);
        _postSvc = new PostService(repo);
    }

    public void Dispose() => _db.Dispose();

    // ── Pipeline 1: Public feed ───────────────────────────────────────

    [Fact]
    public void PublicFeed_ServiceToPageModel_DeliversAllSeeded()
    {
        var fromService = _postSvc.GetObservations();
        Assert.Equal(2, fromService.Count);

        var page = new PublicModel(_postSvc);
        var result = page.OnGet();
        Assert.IsType<PageResult>(result);
        Assert.Equal(2, page.Cheeps.Count);
        Assert.Equal(_obs2, page.Cheeps[0].Id);
    }

    [Fact]
    public void PublicFeed_DataMatchesAcrossLayers()
    {
        var service = _postSvc.GetObservations();
        var page = new PublicModel(_postSvc);
        page.OnGet();

        for (int i = 0; i < service.Count; i++)
        {
            Assert.Equal(service[i].Id, page.Cheeps[i].Id);
            Assert.Equal(service[i].Author, page.Cheeps[i].Author);
            Assert.Equal(service[i].Message, page.Cheeps[i].Message);
        }
    }

    // ── Pipeline 2: User timeline ─────────────────────────────────────

    [Fact]
    public void UserTimeline_ServiceToPageModel_FiltersByAuthor()
    {
        var fromService = _obsSvc.GetObservationsFromAuthor("Eduard");
        Assert.Single(fromService);

        var page = new UserTimelineModel(_obsSvc);
        page.OnGet("Eduard");
        Assert.Single(page.Cheeps);
        Assert.Equal("Eduard", page.Cheeps[0].Author);
    }

    // ── Pipeline 3: Single observation detail ─────────────────────────

    [Fact]
    public void ObservationDetail_FacadeToPageModel_DeliversSeededCommentAndProposal()
    {
        var page = new ObservationModel(_facade, _db.CreatePostRepository());
        var result = page.OnGet(id: _obs1);

        Assert.IsType<PageResult>(result);
        Assert.Equal(_obs1, page.Observation!.Id);
        Assert.Single(page.Comments);
        Assert.Equal("Rats", page.Comments[0].Author);
        Assert.Single(page.Proposals);
        Assert.StartsWith("MSTSNM:Arter:", page.Proposals[0].TaxonId);
    }

    [Fact]
    public void ObservationDetail_ObservationWithoutAttachments_HasEmptyLists()
    {
        var page = new ObservationModel(_facade, _db.CreatePostRepository());
        page.OnGet(id: _obs2);

        Assert.NotNull(page.Observation);
        Assert.Empty(page.Comments);
        Assert.Empty(page.Proposals);
    }

    [Fact]
    public void ObservationDetail_UnknownId_PipelineReturnsNotFound()
    {
        var page = new ObservationModel(_facade, _db.CreatePostRepository());
        var result = page.OnGet(id: 424242);

        var nfr = Assert.IsType<NotFoundResult>(result);
        Assert.Equal(404, nfr.StatusCode);
        Assert.Null(page.Observation);
    }

    [Fact]
    public void ObservationDetail_ListModeWithoutId_ReturnsAll()
    {
        var page = new ObservationModel(_facade, _db.CreatePostRepository());
        var result = page.OnGet(id: null);

        Assert.IsType<PageResult>(result);
        Assert.Equal(2, page.Observations.Count);
    }

    // ── Pipeline 4: Full round-trip with fresh data ───────────────────

    [Fact]
    public void FullPipeline_NewObservation_FlowsThroughAllLayers()
    {
        int newAuthor = _db.InsertUser("Fresh", "fresh@e2e.dk");
        int newObs = _db.InsertObservation(newAuthor, "fresh e2e bird", 1690999999);
        int newComment = _db.InsertComment(newObs, "Fresh", "fresh comment", 1690999998);
        int newProposal = _db.InsertProposal(newObs, "Fresh", "1234", 1690999997);

        // 1. DBFacade sees it.
        var fromFacade = _facade.GetObservationsById(newObs);
        Assert.NotNull(fromFacade);
        Assert.Equal("fresh e2e bird", fromFacade!.Message);
        Assert.Equal("Fresh", fromFacade.Author);

        // 2. PostService (feed) sees it — it's the newest.
        var feed = _postSvc.GetObservations();
        Assert.Equal(newObs, feed[0].Id);

        // 3. ObservationService (author timeline) sees it under "Fresh".
        var timeline = _obsSvc.GetObservationsFromAuthor("Fresh");
        Assert.Single(timeline);
        Assert.Equal(newObs, timeline[0].Id);

        // 4. ObservationModel detail page sees it + its comment + proposal.
        var detail = new ObservationModel(_facade, _db.CreatePostRepository());
        detail.OnGet(id: newObs);
        Assert.Equal(newObs, detail.Observation!.Id);
        Assert.Single(detail.Comments);
        Assert.Equal(newComment, detail.Comments[0].Id);
        Assert.Single(detail.Proposals);
        Assert.Equal(newProposal, detail.Proposals[0].Id);

        // 5. PublicModel feed shows it at the top.
        var publicPage = new PublicModel(_postSvc);
        publicPage.OnGet();
        Assert.Equal(newObs, publicPage.Cheeps[0].Id);

        // 6. UserTimelineModel for "Fresh" shows it.
        var userPage = new UserTimelineModel(_obsSvc);
        userPage.OnGet("Fresh");
        Assert.Single(userPage.Cheeps);
        Assert.Equal(newObs, userPage.Cheeps[0].Id);
    }

    [Fact]
    public void TimestampsAreFormattedConsistently()
    {
        // SeedStandard obs1 pub_date = 1690892208 → "2023-08-01 12:16:48Z"
        var obs = _facade.GetObservationsById(_obs1);
        Assert.Equal("2023-08-01 12:16:48Z", obs!.Timestamp);
    }
}
