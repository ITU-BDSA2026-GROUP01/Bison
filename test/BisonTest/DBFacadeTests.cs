using Bison.Razor.Models;
using Bison.Razor.Services;

namespace BisonTest;

/// <summary>
/// Data-layer tests for <see cref="DBFacade"/> — the SQLite read surface of the
/// new website. Each test runs against an isolated temp database (see
/// <see cref="SQLiteDatabaseTestHelper"/>), so no live server is needed.
/// </summary>
public class DBFacadeTests : IDisposable
{
    private readonly SQLiteDatabaseTestHelper _db;
    private readonly DBFacade _facade;
    private readonly PostRepository _repo;
    private readonly int _obs1; // Eduard's "A heron"
    private readonly int _obs2; // Peter's "A big bird"

    public DBFacadeTests()
    {
        _db = new SQLiteDatabaseTestHelper();
        (_, _, _obs1, _obs2) = _db.SeedStandard();
        _facade = _db.CreateFacade();
        _repo = _db.CreatePostRepository();
    }

    public void Dispose() => _db.Dispose();

    // ── GetAllObservations ──────────────────────────────────────────────────

    [Fact]
    public void GetAllObservations_ReturnsAllOrderedByPubDateDesc()
    {
        var result = _repo.GetAllObservations();

        Assert.Equal(2, result.Count);
        // pub_date DESC: obs2 (1690895308) is newer than obs1 (1690892208).
        Assert.Equal(_obs2, result[0].Id);
        Assert.Equal(_obs1, result[1].Id);

    }

    [Fact]
    public void GetAllObservations_MapsAllFields()
    {
        var obs = _repo.GetAllObservations().First(o => o.Id == _obs1);

        Assert.Equal("Eduard", obs.Author);     // resolved via the user JOIN
        Assert.Equal("A heron", obs.Message);
        Assert.Equal("2023-08-01 12:16:48Z", obs.Timestamp);
    }

    [Fact]
    public void GetAllObservations_PagesAtPageSize32()
    {
        // Fresh DB with exactly 33 observations so page 2 is non-empty.
        using var db = new SQLiteDatabaseTestHelper();
        int author = db.InsertUser("Eduard", "edka@itu.dk");
        for (int i = 0; i < 33; i++)
            db.InsertObservation(author, $"bird {i}", 1690000000 + i);

        var facade = db.CreatePostRepository();
        var page1 = facade.GetAllObservations(page: 1);
        var page2 = facade.GetAllObservations(page: 2);

        Assert.Equal(32, page1.Count);   // PageSize
        Assert.Single(page2);           // the one that overflowed
        // Page 1 = the 32 newest; page 2 = the oldest.
        Assert.Equal(1690000032, DateTimeOffset.Parse(page1[0].Timestamp).ToUnixTimeSeconds());
    }

    // ── GetObservationsByAuthor ─────────────────────────────────────────────

    [Fact]
    public void GetObservationsByAuthor_ReturnsOnlyThatAuthorsObservations()
    {
        int edu = _db.InsertUser("Eduard", "edka@itu.dk");
        _db.InsertObservation(edu, "A small bird", 1691000000); // a 3rd Eduard row

        var result = _repo.GetObservationsByAuthor(author: "Eduard");

        Assert.Equal(2, result.Count);
        Assert.All(result, o => Assert.Equal("Eduard", o.Author));
    }

    [Fact]
    public void GetObservationsByAuthor_UnknownAuthor_ReturnsEmpty()
    {
        var result = _repo.GetObservationsByAuthor(author: "Nobody");

        Assert.Empty(result);
    }

    [Fact]
    public void GetObservationsByAuthor_PagesResults()
    {
        int edu = _db.InsertUser("Eduard", "edka@itu.dk");
        // SeedStandard already created one Eduard observation, so insert 32 more
        // to reach a total of 33 (one overflow onto page 2).
        for (int i = 0; i < 32; i++)
            _db.InsertObservation(edu, $"bird {i}", 1690000000 + i);

        var page1 = _repo.GetObservationsByAuthor("Eduard", page: 1);
        var page2 = _repo.GetObservationsByAuthor("Eduard", page: 2);

        Assert.Equal(32, page1.Count);
        Assert.Single(page2);
    }

    // ── GetObservationsById ─────────────────────────────────────────────────

    [Fact]
    public void GetObservationsById_ReturnsTheMatchingObservation()
    {
        var obs = _facade.GetObservationsById(_obs1);

        Assert.NotNull(obs);
        Assert.Equal(_obs1, obs!.Id);
        Assert.Equal("Eduard", obs.Author);
        Assert.Equal("A heron", obs.Message);
    }

    [Fact]
    public void GetObservationsById_NonExistentId_ReturnsNull()
    {
        var obs = _facade.GetObservationsById(9999);

        Assert.Null(obs);
    }

    // ── GetCommentsForObservation ───────────────────────────────────────────

    [Fact]
    public void GetCommentsForObservation_ReturnsCommentsOrderedByPubDateDesc()
    {
        int c1 = _db.InsertComment(_obs1, "Rats", "first", 1690890000);
        int c2 = _db.InsertComment(_obs1, "Gull", "second", 1690895308);

        var result = _facade.GetCommentsForObservation(_obs1);

        Assert.Equal(3, result.Count); // SeedStandard's comment + these two
        // DESC: newest (c2) first.
        Assert.Equal(c2, result[0].Id);
        Assert.All(result, c => Assert.Equal(_obs1, c.ObservationId));
    }

    [Fact]
    public void GetCommentsForObservation_ObservationWithoutComments_ReturnsEmpty()
    {
        var result = _facade.GetCommentsForObservation(_obs2);

        Assert.Empty(result);
    }

    [Fact]
    public void GetCommentsForObservation_MapsAllFields()
    {
        int c = _db.InsertComment(_obs1, "Rats", "hello", 1690892208);

        var comment = _facade.GetCommentsForObservation(_obs1).First(x => x.Id == c);

        Assert.Equal("Rats", comment.Author);
        Assert.Equal("hello", comment.Message);
        Assert.Equal("2023-08-01 12:16:48Z", comment.Timestamp);
    }

    // ── GetProposalsForObservation ──────────────────────────────────────────

    [Fact]
    public void GetProposalsForObservation_ReturnsProposalsForThatObservation()
    {
        int p = _db.InsertProposal(_obs1, "Rats", "MSTSNM:Arter:abc-123", 1690895308);

        var result = _facade.GetProposalsForObservation(_obs1);

        // SeedStandard already added one proposal to obs1.
        var proposal = result.First(x => x.Id == p);
        Assert.Equal(_obs1, proposal.ObservationId);
        Assert.Equal("Rats", proposal.Author);
        Assert.Equal("MSTSNM:Arter:abc-123", proposal.TaxonId);
    }

    [Fact]
    public void GetProposalsForObservation_ObservationWithoutProposals_ReturnsEmpty()
    {
        var result = _facade.GetProposalsForObservation(_obs2);

        Assert.Empty(result);
    }
}
