using Bison.Razor.Pages;
using Bison.Razor.Services;
using BisonTest;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebserverTest;


public class PublicModelTests : IDisposable
{
    // The test database helper, which sets up an isolated SQLite database for each test.
    private readonly SQLiteDatabaseTestHelper _db;
    // The model under test, which uses the PostService and the test database context.
    private readonly PublicModel _model;
    // The IDs of the two seeded observations, for easier assertions.
    private int _obs1, _obs2;

    // Constructor sets up the test database and seeds it with standard data.
    public PublicModelTests()
    {
        // Create a new isolated test database and seed it with standard data.
        _db = new SQLiteDatabaseTestHelper();
        // SeedStandard returns a tuple of (userId, taxonId, obs1Id, obs2Id). We only care about the observation IDs here.
        (_, _, _obs1, _obs2) = _db.SeedStandard();
        // Create the PublicModel instance, which is the page model we are testing. It uses the PostService and the test database context.
        _model = new PublicModel(new PostService(_db.CreatePostRepository()), _db.Context);
    }

    // Dispose method cleans up the test database after each test.
    public void Dispose() => _db.Dispose();


    [Fact]
    // Test that the OnGet method returns a PageResult, indicating that the page was rendered successfully.
    public void OnGet_ReturnsPageResult()
    {
        
        var result = _model.OnGet(); 
        // Assert that the result is of type PageResult, which means the page was rendered successfully.
        Assert.IsType<PageResult>(result);
    }

    [Fact]
    // Test that the OnGet method populates the Cheeps property with all seeded observations, in newest-first order.
    public void OnGet_PopulatesCheepsWithAllSeeded_NewestFirst()
    {
        _model.OnGet();

        Assert.Equal(2, _model.Cheeps.Count);
        Assert.Equal(_obs2, _model.Cheeps[0].Id);
        Assert.Equal(_obs1, _model.Cheeps[1].Id);
    }

    [Fact]
    // Test that each Cheep has a non-empty Author, Message, and Timestamp after OnGet is called.
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
    // Test that the OnGet method respects pagination when a page parameter is provided.
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
    // Test that requesting a page beyond the available data returns an empty Cheeps list.
    public void OnGet_PageBeyondData_CheepsIsEmpty()
    {
        _model.OnGet(page: 99);

        Assert.Empty(_model.Cheeps);
    }

        // Helper method to seed taxa for testing. Returns the IDs of the bird, heron, and mammal taxa.
        private (int bird, int heron, int mammal) SeedTaxa()
    {
        // Insert taxa into the test database. The heron is a child of bird, and mammal is a separate branch.
        int bird = _db.InsertTaxon("t:bird", "Bird");
        int heron = _db.InsertTaxon("t:heron", "Heron", bird);
        int mammal = _db.InsertTaxon("t:mammal", "Mammal");
        return (bird, heron, mammal);
    }

    [Fact]
    // Test that filtering by a taxon returns only observations of that taxon and its sub-taxa.
    public void OnGet_WithTaxonId_ReturnsObservationsOfTaxonAndSubTaxa()
    {
        // Seed taxa and insert observations for testing. The heron is a sub-taxon of bird, so both should be returned when filtering by bird.
        var (bird, heron, mammal) = SeedTaxa();
        int author = _db.InsertUser("Birder", "b@x.dk");
        int oBird = _db.InsertObservationWithTaxon(author, bird, "some bird", 1700000001);
        int oHeron = _db.InsertObservationWithTaxon(author, heron, "a heron", 1700000002);
        _db.InsertObservationWithTaxon(author, mammal, "a mammal", 1700000003);

        // Call OnGet with the bird taxon ID, which should return both the bird and heron observations, but not the mammal observation.
        var result = _model.OnGet(taxonId: bird);

        // Assert that the result is a PageResult and that the Cheeps list contains only the bird and heron observations, in newest-first order.
        Assert.IsType<PageResult>(result);
        // The Cheeps list should contain the heron and bird observations, in newest-first order.
        Assert.Equal(new[] { oHeron, oBird }, _model.Cheeps.Select(c => c.Id).ToArray()); // newest first
    }

    [Fact]
    // Test that filtering by a taxon fills in the Author name for each Cheep.
    public void OnGet_WithTaxonId_FillsInAuthorName()
    {
        // Seed taxa and insert an observation for testing. The heron is a sub-taxon of bird, so it should be returned when filtering by bird.
        var (bird, _, _) = SeedTaxa();
        int author = _db.InsertUser("Birder", "b@x.dk");
        _db.InsertObservationWithTaxon(author, bird, "some bird", 1700000001);

        // Call OnGet with the bird taxon ID, which should return the observation and fill in the Author name.
        _model.OnGet(taxonId: bird);

        // Assert that the Cheeps list contains one observation and that its Author is "Birder".
        Assert.Equal("Birder", Assert.Single(_model.Cheeps).Author);
    }

    [Fact]
    // Test that filtering by a taxon respects pagination after filtering.
    public void OnGet_WithTaxonId_PaginatesAfterFiltering()
    {
        // Seed taxa and insert 40 heron observations and 40 mammal observations for testing. The heron is a sub-taxon of bird, so filtering by bird should return only the heron observations.
        var (bird, heron, mammal) = SeedTaxa();
        int author = _db.InsertUser("Bulk", "bulk@x.dk");
        for (int i = 0; i < 40; i++)
            _db.InsertObservationWithTaxon(author, heron, $"heron {i}", 1700000000 + i);
        // Newer than every heron, so a "page first, filter later" implementation
        // would return an empty first page here.
        for (int i = 0; i < 40; i++)
            _db.InsertObservationWithTaxon(author, mammal, $"mammal {i}", 1800000000 + i);

        // Call OnGet with the bird taxon ID, which should return only the heron observations, paginated.
        _model.OnGet(page: 1, taxonId: bird);
        Assert.Equal(32, _model.Cheeps.Count);

        // Call OnGet with the bird taxon ID and page 2, which should return the remaining heron observations.
        _model.OnGet(page: 2, taxonId: bird);
        Assert.Equal(8, _model.Cheeps.Count);
    }

    [Fact]
    // Test that filtering by a taxon returns an empty Cheeps list when there are no matching observations.
    public void OnGet_WithTaxonIdWithoutMatches_CheepsIsEmpty()
    {
        // Seed taxa and insert a mammal observation for testing. The bird taxon has no matching observations, so filtering by bird should return an empty Cheeps list.
        var (bird, _, mammal) = SeedTaxa();
        int author = _db.InsertUser("Birder", "b@x.dk");
        _db.InsertObservationWithTaxon(author, mammal, "a mammal", 1700000001);

        // Call OnGet with the bird taxon ID, which should return an empty Cheeps list since there are no matching observations.
        var result = _model.OnGet(taxonId: bird);

        // Assert that the result is a PageResult and that the Cheeps list is empty.
        Assert.IsType<PageResult>(result);
        Assert.Empty(_model.Cheeps);
    }

    [Fact]
    // Test that filtering by an unknown taxon ID returns a NotFoundResult.
    public void OnGet_WithUnknownTaxonId_ReturnsNotFound()
    { 
        // Call OnGet with a taxon ID that does not exist in the database, which should return a NotFoundResult.
        var result = _model.OnGet(taxonId: 987654);

        // Assert that the result is of type NotFoundResult, indicating that the requested taxon was not found.
        Assert.IsType<NotFoundResult>(result);
    }
}
