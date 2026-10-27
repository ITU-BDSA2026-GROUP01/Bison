using Bison.Razor.Data;
using Bison.Razor.Models;
using Microsoft.EntityFrameworkCore;

namespace BisonTest;



public sealed class SQLiteDatabaseTestHelper : IDisposable
{
    // The path to the temporary SQLite database file. Each test gets its own isolated database.
    public string DbPath { get; }

    // The EF Core database context for the test database. This is created once and reused for the lifetime of the helper.
    private readonly BisonDBContext _ctx;

    // Constructor creates a new temporary SQLite database and ensures it is created. It also seeds a default taxon so that observations can be inserted without violating foreign key constraints.
    public SQLiteDatabaseTestHelper()
    {
        // Create a unique temporary file path for the SQLite database. This ensures that each test run has its own isolated database.
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
    
    public BisonDBContext Context => _ctx;

    private BisonDBContext NewContext()
    {
        var options = new DbContextOptionsBuilder<BisonDBContext>()
            .UseSqlite($"Data Source={DbPath}")
            .Options;
        return new BisonDBContext(options);
    }

    // ── Convenience: insert helpers ───────────────────────────────────────
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


    public int InsertTaxon(string dwcTaxonId, string? vernacularName, int? parentId = null)
    {
        using var ctx = NewContext();
        var taxon = new Taxon { dwc_TaxonID = dwcTaxonId, VernacularName = vernacularName, ParentId = parentId };
        ctx.Taxons.Add(taxon);
        ctx.SaveChanges();
        return taxon.TaxonId;
    }

    public int InsertObservationWithTaxon(int authorId, int taxonId, string text, long pubDate)
    {
        using var ctx = NewContext();
        var author = ctx.Authors.Find(authorId)
            ?? throw new InvalidOperationException($"Author {authorId} not found");
        var taxon = ctx.Taxons.Find(taxonId)
            ?? throw new InvalidOperationException($"Taxon {taxonId} not found");
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
    // Inserts a comment for a given observation, creating the author if they don't exist. Returns the PostId of the new comment.
    public int InsertComment(int observationId, string author, string text, long pubDate)
    {
        // Use a new context to ensure that the comment is added in a fresh transaction. This avoids issues with tracking entities across different contexts.
        using var ctx = NewContext();
        var observation = ctx.Observations.Find(observationId)
            ?? throw new InvalidOperationException($"Observation {observationId} not found");
        var authorEntity = ctx.Authors.FirstOrDefault(a => a.Name == author);
        if (authorEntity == null)
        {
            authorEntity = new Author { Name = author, Email = $"{author}@local.test" };
            ctx.Authors.Add(authorEntity);
        }
        // Create a new Comment entity linking the observation and author. The Text is set to the provided text, and the TimeStamp is set based on the provided pubDate.
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
        // Use a new context to ensure that the proposal is added in a fresh transaction. This avoids issues with tracking entities across different contexts.
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
            // If the taxon doesn't exist, create a new one with the given dwc_TaxonID. The VernacularName is set to null since it's not provided in this context.
            taxon = new Taxon { dwc_TaxonID = taxonId, VernacularName = null };
            ctx.Taxons.Add(taxon);
        }
        // Create a new Proposal entity linking the observation, author, and taxon. The Text is set to an empty string, and the TimeStamp is set based on the provided pubDate.
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

    // Seeds the database with a standard set of users and observations for testing. Returns a tuple of (userId, taxonId, obs1Id, obs2Id) for easier assertions in tests.
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

    // Creates a new BisonDBContext for the test database. This is useful for tests that need a fresh context to avoid issues with entity tracking.
    public BisonDBContext CreateContext() => _ctx;

    // Creates a new PostRepository that uses the test database context. This is useful for tests that need to interact with posts in the database.
    public Bison.Razor.Services.PostRepository CreatePostRepository()
        => new(CreateContext());

    // Creates a new ObservationService that uses the test database context. This is useful for tests that need to interact with observations in the database.
    public Bison.Razor.Services.ObservationService CreateObservationService()
        => new(CreatePostRepository());

    // Creates a new PostService that uses the test database context. This is useful for tests that need to interact with posts in the database.
    public Bison.Razor.Services.PostService CreatePostService()
        => new(CreatePostRepository());
}
