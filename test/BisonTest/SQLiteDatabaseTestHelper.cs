using Bison.Razor.Data;
using Bison.Razor.Models;
using Microsoft.EntityFrameworkCore;

namespace BisonTest;

/// <summary>
/// Hermetic EF-Core test fixture. Each test gets its own isolated temp .db
/// file (fresh GUID-named) whose schema is created by EF Core on first use,
/// then deleted in <see cref="Dispose"/>. No live server, no shared state,
/// no real data files — matching the project's test rules (server-free +
/// hermetic).
///
/// All insert helpers add entities through <see cref="BisonDBContext"/> so
/// the repository under test sees exactly what the app would see.
/// </summary>
public sealed class SQLiteDatabaseTestHelper : IDisposable
{
    /// <summary>Absolute path to the isolated temp database file.</summary>
    public string DbPath { get; }

    private readonly BisonDBContext _ctx;

    public SQLiteDatabaseTestHelper()
    {
        DbPath = Path.Combine(Path.GetTempPath(), $"bison_test_{Guid.NewGuid():N}.db");
        _ctx = NewContext();
        _ctx.Database.EnsureCreated();

        // The Observation entity requires a non-null Taxon. Seed a single
        // default taxon so InsertObservation always has one to link to.
        using var ctx = NewContext();
        ctx.Taxons.Add(new Taxon
        {
            dwc_TaxonID = "MSTSNM:Arter:default",
            VernacularName = "Test taxon"
        });
        ctx.SaveChanges();
    }

    public void Dispose()
    {
        try { _ctx.Dispose(); } catch { /* best-effort */ }
        try { if (File.Exists(DbPath)) File.Delete(DbPath); }
        catch { /* never mask the real test failure */ }
    }

    private BisonDBContext NewContext()
    {
        var options = new DbContextOptionsBuilder<BisonDBContext>()
            .UseSqlite($"Data Source={DbPath}")
            .Options;
        return new BisonDBContext(options);
    }

    // ── EF insert helpers (return the new row's id) ──────────────────────

    public int InsertUser(string username, string email)
    {
        using var ctx = NewContext();
        var author = new Author { Name = username, Email = email };
        ctx.Authors.Add(author);
        ctx.SaveChanges();
        return author.AuthorId;
    }

    public int InsertObservation(int authorId, string text, long pubDate)
    {
        using var ctx = NewContext();
        var author = ctx.Authors.Find(authorId)
            ?? throw new InvalidOperationException($"Author {authorId} not found");
        var taxon = ctx.Taxons.OrderBy(t => t.TaxonId).First();
        var obs = new Observation
        {
            Author = author,
            Taxon = taxon,
            Text = text,
            TimeStamp = DateTimeOffset.FromUnixTimeSeconds(pubDate).UtcDateTime
        };
        ctx.Observations.Add(obs);
        ctx.SaveChanges();
        return obs.PostId;
    }

    public int InsertComment(int observationId, string author, string text, long pubDate)
    {
        using var ctx = NewContext();
        var observation = ctx.Observations.Find(observationId)
            ?? throw new InvalidOperationException($"Observation {observationId} not found");
        var authorEntity = ctx.Authors.FirstOrDefault(a => a.Name == author);
        if (authorEntity == null)
        {
            authorEntity = new Author { Name = author, Email = $"{author}@local.test" };
            ctx.Authors.Add(authorEntity);
        }
        var comment = new Comment
        {
            Observation = observation,
            Author = authorEntity,
            Text = text,
            TimeStamp = DateTimeOffset.FromUnixTimeSeconds(pubDate).UtcDateTime
        };
        ctx.Comments.Add(comment);
        ctx.SaveChanges();
        return comment.PostId;
    }

    public int InsertProposal(int observationId, string author, string taxonId, long pubDate)
    {
        using var ctx = NewContext();
        var observation = ctx.Observations.Find(observationId)
            ?? throw new InvalidOperationException($"Observation {observationId} not found");
        var authorEntity = ctx.Authors.FirstOrDefault(a => a.Name == author);
        if (authorEntity == null)
        {
            authorEntity = new Author { Name = author, Email = $"{author}@local.test" };
            ctx.Authors.Add(authorEntity);
        }
        // Ensure a taxon with this dwc_TaxonID exists (the DTO exposes the
        // string, not the int PK).
        var taxon = ctx.Taxons.FirstOrDefault(t => t.dwc_TaxonID == taxonId);
        if (taxon == null)
        {
            taxon = new Taxon { dwc_TaxonID = taxonId, VernacularName = null };
            ctx.Taxons.Add(taxon);
        }
        var proposal = new Proposal
        {
            Observation = observation,
            Author = authorEntity,
            Taxon = taxon,
            Text = "",
            TimeStamp = DateTimeOffset.FromUnixTimeSeconds(pubDate).UtcDateTime
        };
        ctx.Proposals.Add(proposal);
        ctx.SaveChanges();
        return proposal.PostId;
    }

    // ── Convenience: the standard seed ────────────────────────────────────

    /// <summary>Inserts the two users + two observations + one comment + one
    /// proposal that the test suite has always expected (Eduard "A heron",
    /// Peter "A big bird", a "Rats" comment and proposal on obs1).</summary>
    public (int edu, int pet, int obs1, int obs2) SeedStandard()
    {
        int edu  = InsertUser("Eduard", "edka@itu.dk");
        int pet  = InsertUser("Peter",  "debloh@itu.dk");
        int obs1 = InsertObservation(edu, "A heron",    1690892208);
        int obs2 = InsertObservation(pet, "A big bird", 1690895308);
        InsertComment (obs1, "Rats", "wow amazing", 1690892208);
        InsertProposal(obs1, "Rats", "MSTSNM:Arter:a15367e4-f785-ea11-aa77-501ac539d1ea", 1690895308);
        return (edu, pet, obs1, obs2);
    }

    // ── Factories (all pointed at this helper's temp DB) ─────────────────

    public BisonDBContext CreateContext() => _ctx;

    public Bison.Razor.Services.PostRepository CreatePostRepository()
        => new(CreateContext());

    public Bison.Razor.Services.ObservationService CreateObservationService()
        => new(CreatePostRepository());

    public Bison.Razor.Services.PostService CreatePostService()
        => new(CreatePostRepository());
}
