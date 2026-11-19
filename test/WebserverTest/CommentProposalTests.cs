using Bison.Core;
using Bison.Infrastructure.Data;
using Bison.Repositories;
using Bison.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace WebserverTest;

// Unit tests of comments and proposals. Its works like this we have a service -> repository -> in-memory SQLite database.
public class CommentProposalTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly BisonDBContext _context;
    private readonly PostService _service;
    private readonly int _observationId;
    private readonly int _taxonId;

    public CommentProposalTests()
    {
        // the connection must stay open, otherwise the in-memory database disappears
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<BisonDBContext>().UseSqlite(_connection).Options;
        _context = new BisonDBContext(options);
        _context.Database.EnsureCreated();

        var taxon = new Taxon { dwc_TaxonID = "test-heron", VernacularName = "Grey heron" };
        _context.Taxons.Add(taxon);
        _context.SaveChanges();
        _taxonId = taxon.TaxonId;

        _service = new PostService(new PostRepository(_context));
        _observationId = _service.CreateObservation("Eduard", "eduard@test.dk", "A heron by the lake", _taxonId).Id;
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public void CreateComment_IsStoredAndCanBeFetched()
    {
        _service.CreateComment("Nathan", "nathan@test.dk", "Nice picture!", _observationId);

        var comments = _service.GetComments(_observationId);

        var comment = Assert.Single(comments);
        Assert.Equal("Nice picture!", comment.Message);
        Assert.Equal("Nathan", comment.Author);
        Assert.Equal(_observationId, comment.ObservationId);
    }

    [Fact]
    public void CreateComment_TrimsText()
    {
        _service.CreateComment("Nathan", "nathan@test.dk", "  hello  ", _observationId);

        Assert.Equal("hello", _service.GetComments(_observationId).Single().Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateComment_EmptyText_Throws(string text)
    {
        Assert.Throws<ArgumentException>(() =>
            _service.CreateComment("Nathan", "nathan@test.dk", text, _observationId));
    }

    [Fact]
    public void CreateComment_UnknownObservation_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            _service.CreateComment("Nathan", "nathan@test.dk", "hello", 99999));
    }

    [Fact]
    public void CreateComment_ReusesExistingAuthor()
    {
        _service.CreateComment("Eduard", "eduard@test.dk", "my own comment", _observationId);

        // Eduard already exists from the observation, so there is still only one
        Assert.Equal(1, _context.Authors.Count(a => a.Email == "eduard@test.dk"));
    }

    [Fact]
    public void CreateProposal_IsStoredWithTaxonName()
    {
        _service.CreateProposal("Nathan", "nathan@test.dk", _observationId, _taxonId);

        var proposal = Assert.Single(_service.GetProposals(_observationId));
        Assert.Equal("Grey heron", proposal.TaxonName);
        Assert.Equal("test-heron", proposal.TaxonId);
        Assert.Equal("Nathan", proposal.Author);
    }

    [Fact]
    public void CreateProposal_UnknownTaxon_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            _service.CreateProposal("Nathan", "nathan@test.dk", _observationId, 99999));
    }

    [Fact]
    public void CreateProposal_UnknownObservation_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            _service.CreateProposal("Nathan", "nathan@test.dk", 99999, _taxonId));
    }

    [Fact]
    public void CommentsAndProposals_BelongToTheirOwnObservation()
    {
        var other = _service.CreateObservation("Eduard", "eduard@test.dk", "Another bird", _taxonId).Id;
        _service.CreateComment("Nathan", "nathan@test.dk", "only on first", _observationId);

        Assert.Empty(_service.GetComments(other));
    }
}