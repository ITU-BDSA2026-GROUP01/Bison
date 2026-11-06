using Bison.Razor.Models;
using Bison.Razor.Services;

namespace BisonTest;

/// <summary>
/// Data-layer tests for <see cref="PostRepository"/> — the EF-Core read
/// surface of the new website. Each test runs against an isolated temp
/// database (see <see cref="SQLiteDatabaseTestHelper"/>), so no live server
/// is needed.
/// </summary>
public class PostRepositoryTests : IDisposable
{
    private readonly SQLiteDatabaseTestHelper _db;
    private readonly PostRepository _repo;
    private readonly int _edu;   // Eduard's author id (seeded)
    private readonly int _obs1;  // Eduard's "A heron"
    private readonly int _obs2;  // Peter's "A big bird"

    public PostRepositoryTests()
    {
        _db = new SQLiteDatabaseTestHelper();
        var seed = _db.SeedStandard();
        _edu = seed.edu;
        _obs1 = seed.obs1;
        _obs2 = seed.obs2;
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

        Assert.Equal("Eduard", obs.Author);
        Assert.Equal("A heron", obs.Message);
        // Stored as local DateTime; "u" format gives the datetime portion.
        Assert.StartsWith("2023-08-01 12:16:48", obs.Timestamp);
    }

    [Fact]
    public void GetAllObservations_PagesAtPageSize32()
    {
        using var db = new SQLiteDatabaseTestHelper();
        int author = db.InsertUser("Eduard", "edka@itu.dk");
        for (int i = 0; i < 33; i++)
            db.InsertObservation(author, $"bird {i}", 1690000000 + i);

        var repo = db.CreatePostRepository();
        var page1 = repo.GetAllObservations(page: 1);
        var page2 = repo.GetAllObservations(page: 2);

        Assert.Equal(32, page1.Count);   // PageSize
        Assert.Single(page2);           // the one that overflowed
        // Page 1 = the 32 newest (bird 32 is newest); page 2 = the oldest.
        Assert.Equal("bird 32", page1[0].Message);
        Assert.Equal("bird 0", page2[0].Message);
    }

    // ── GetObservationsByAuthor ─────────────────────────────────────────────

    [Fact]
    public void GetObservationsByAuthor_ReturnsOnlyThatAuthorsObservations()
    {
        _db.InsertObservation(_edu, "A small bird", 1691000000); // a 3rd Eduard row

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
        // SeedStandard already created one Eduard observation; add 32 more to
        // reach 33 (one overflow onto page 2).
        for (int i = 0; i < 32; i++)
            _db.InsertObservation(_edu, $"bird {i}", 1690000000 + i);

        var page1 = _repo.GetObservationsByAuthor("Eduard", page: 1);
        var page2 = _repo.GetObservationsByAuthor("Eduard", page: 2);

        Assert.Equal(32, page1.Count);
        Assert.Single(page2);
    }

    // ── GetObservationWithAttachments ───────────────────────────────────────

    [Fact]
    public void GetObservationWithAttachments_ReturnsTheMatchingObservation()
    {
        var obs = _repo.GetObservationWithAttachments(_obs1);

        Assert.NotNull(obs);
        Assert.Equal(_obs1, obs!.PostId);
        Assert.Equal("Eduard", obs.Author.Name);
        Assert.Equal("A heron", obs.Text);
        Assert.NotNull(obs.Taxon);
    }

    [Fact]
    public void GetObservationWithAttachments_NonExistentId_ReturnsNull()
    {
        Assert.Null(_repo.GetObservationWithAttachments(9999));
    }

    [Fact]
    public void GetObservationWithAttachments_IncludesSeededCommentsAndProposals()
    {
        var obs = _repo.GetObservationWithAttachments(_obs1)!;

        Assert.Single(obs.Comments);
        Assert.Equal("Rats", obs.Comments[0].Author.Name);
        Assert.Equal("wow amazing", obs.Comments[0].Text);
        Assert.Single(obs.Proposals);
        Assert.Equal("Rats", obs.Proposals[0].Author.Name);
        Assert.StartsWith("MSTSNM:Arter:", obs.Proposals[0].Taxon.dwc_TaxonID);
    }

    [Fact]
    public void GetObservationWithAttachments_ObservationWithoutAttachments_HasEmptyLists()
    {
        var obs = _repo.GetObservationWithAttachments(_obs2)!;

        Assert.Empty(obs.Comments);
        Assert.Empty(obs.Proposals);
    }

    // ── GetCommentsForObservation ───────────────────────────────────────────

    [Fact]
    public void GetCommentsForObservation_ReturnsCommentsOrderedByPubDateDesc()
    {
        int c1 = _db.InsertComment(_obs1, "Rats", "first", 1690890000);
        int c2 = _db.InsertComment(_obs1, "Gull", "second", 1690895308);

        var result = _repo.GetCommentsForObservation(_obs1);

        Assert.Equal(3, result.Count); // SeedStandard's comment + these two
        // DESC: newest (c2) first.
        Assert.Equal(c2, result[0].Id);
        Assert.All(result, c => Assert.Equal(_obs1, c.ObservationId));
    }

    [Fact]
    public void GetCommentsForObservation_ObservationWithoutComments_ReturnsEmpty()
    {
        Assert.Empty(_repo.GetCommentsForObservation(_obs2));
    }

    [Fact]
    public void GetCommentsForObservation_MapsAllFields()
    {
        int c = _db.InsertComment(_obs1, "Rats", "hello", 1690892208);

        var comment = _repo.GetCommentsForObservation(_obs1).First(x => x.Id == c);

        Assert.Equal("Rats", comment.Author);
        Assert.Equal("hello", comment.Message);
        Assert.StartsWith("2023-08-01 12:16:48", comment.Timestamp);
    }

    // ── GetProposalsForObservation ──────────────────────────────────────────

    [Fact]
    public void GetProposalsForObservation_ReturnsProposalsForThatObservation()
    {
        int p = _db.InsertProposal(_obs1, "Rats", "MSTSNM:Arter:abc-123", 1690895308);

        var result = _repo.GetProposalsForObservation(_obs1);

        // SeedStandard already added one proposal to obs1.
        var proposal = result.First(x => x.Id == p);
        Assert.Equal(_obs1, proposal.ObservationId);
        Assert.Equal("Rats", proposal.Author);
        Assert.Equal("MSTSNM:Arter:abc-123", proposal.TaxonId);
    }

    [Fact]
    public void GetProposalsForObservation_ObservationWithoutProposals_ReturnsEmpty()
    {
        Assert.Empty(_repo.GetProposalsForObservation(_obs2));
    }
}
