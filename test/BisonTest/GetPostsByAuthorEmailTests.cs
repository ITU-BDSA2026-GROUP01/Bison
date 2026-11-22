using Bison.Core;
using Bison.Repositories;

namespace BisonTest;


public class GetPostsByAuthorEmailTests : IDisposable
{
    private readonly SQLiteDatabaseTestHelper _db = new();
    private readonly PostRepository _repo;

    public GetPostsByAuthorEmailTests()
    {
        _repo = _db.CreatePostRepository();
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public void ReturnsOnlyPostsOfThatEmail_EvenWhenNamesAreEqual()
    {
        // Two different people who happen to have the same full name.
        var ctx = _db.Context;
        var taxonA = ctx.Taxons.First();
        var taxonB = new Taxon { dwc_TaxonID = "test:taxon-b", VernacularName = "Taxon B" };
        var a = new Author { Name = "Sam Same", Email = "sam.a@itu.dk" };
        var b = new Author { Name = "Sam Same", Email = "sam.b@itu.dk" };
        var time = new DateTime(2026, 11, 1, 12, 0, 0, DateTimeKind.Utc);

        var obsA = new Observation { Author = a, Taxon = taxonA, Text = "observation from A", TimeStamp = time };
        var obsB = new Observation { Author = b, Taxon = taxonA, Text = "observation from B", TimeStamp = time };
        ctx.Observations.AddRange(obsA, obsB);
        ctx.Comments.AddRange(
            new Comment { Observation = obsA, Author = a, Text = "comment from A", TimeStamp = time },
            new Comment { Observation = obsA, Author = b, Text = "comment from B", TimeStamp = time });
        ctx.Proposals.AddRange(
            new Proposal { Observation = obsA, Author = a, Taxon = taxonA, Text = "", TimeStamp = time },
            new Proposal { Observation = obsA, Author = b, Taxon = taxonB, Text = "", TimeStamp = time });
        ctx.SaveChanges();

        var resultA = _repo.GetPostsByAuthorEmail("sam.a@itu.dk");
        Assert.Equal("observation from A", Assert.Single(resultA.Observations).Message);
        Assert.Equal("comment from A", Assert.Single(resultA.Comments).Message);
        Assert.Equal("Test taxon", Assert.Single(resultA.Proposals).TaxonName);

        var resultB = _repo.GetPostsByAuthorEmail("sam.b@itu.dk");
        Assert.Equal("observation from B", Assert.Single(resultB.Observations).Message);
        Assert.Equal("comment from B", Assert.Single(resultB.Comments).Message);
        Assert.Equal("Taxon B", Assert.Single(resultB.Proposals).TaxonName);
    }

    [Fact]
    public void UnknownEmail_ReturnsEmptyLists()
    {
        _db.SeedStandard();

        var result = _repo.GetPostsByAuthorEmail("nobody@itu.dk");

        Assert.Empty(result.Observations);
        Assert.Empty(result.Comments);
        Assert.Empty(result.Proposals);
    }

    [Fact]
    public void UserWithOnlyComments_HasNoObservations()
    {
        // SeedStandard: "Rats" wrote one comment and one proposal (email Rats@local.test).
        _db.SeedStandard();

        var rats = _repo.GetPostsByAuthorEmail("Rats@local.test");
        Assert.Empty(rats.Observations);
        Assert.Single(rats.Comments);
        Assert.Single(rats.Proposals);

        // Eduard wrote one observation and nothing else.
        var eduard = _repo.GetPostsByAuthorEmail("edka@itu.dk");
        Assert.Single(eduard.Observations);
        Assert.Empty(eduard.Comments);
        Assert.Empty(eduard.Proposals);
    }

    [Fact]
    public void Observations_AreReturnedNewestFirst_WithoutPaging()
    {
        int author = _db.InsertUser("Eduard", "edka@itu.dk");
        for (int i = 0; i < 40; i++)           // more than one page (32)
            _db.InsertObservation(author, $"bird {i}", 1690000000 + i);

        var result = _repo.GetPostsByAuthorEmail("edka@itu.dk");

        Assert.Equal(40, result.Observations.Count);
        Assert.Equal("bird 39", result.Observations[0].Message);
        Assert.Equal("bird 0", result.Observations[^1].Message);
    }
}