using Bison.Razor.Services;
using BisonTest;

namespace CsvTest;


public class ObservationServiceTests : IDisposable
{
    private readonly SQLiteDatabaseTestHelper _db;
    private readonly ObservationService _svc;
    private int _pet, _obs1, _obs2;

    public ObservationServiceTests()
    {
        _db = new SQLiteDatabaseTestHelper();
        (_, _pet, _obs1, _obs2) = _db.SeedStandard();
        _svc = new ObservationService(_db.CreatePostRepository());
    }

    public void Dispose() => _db.Dispose();

    // ── GetObservationsFromAuthor ─────────────────────────────

    [Fact]
    public void GetObservationsFromAuthor_Eduard_ReturnsOne()
    {
        var result = _svc.GetObservationsFromAuthor("Eduard");

        Assert.Single(result);
        Assert.Equal(_obs1, result[0].Id);
        Assert.Equal("Eduard", result[0].Author);
    }

    [Fact]
    public void GetObservationsFromAuthor_Peter_ReturnsOne()
    {
        var result = _svc.GetObservationsFromAuthor("Peter");

        Assert.Single(result);
        Assert.Equal(_obs2, result[0].Id);
        Assert.Equal("Peter", result[0].Author);
    }

    [Fact]
    public void GetObservationsFromAuthor_UnknownAuthor_ReturnsEmpty()
    {
        Assert.Empty(_svc.GetObservationsFromAuthor("NobodyHere"));
    }

    [Fact]
    public void GetObservationsFromAuthor_Pagination()
    {
        int big = _db.InsertUser("BigAuthor", "b@x.dk");
        for (int i = 0; i < 33; i++)
            _db.InsertObservation(big, $"more {i}", 1688000000 + i);

        var page1 = _svc.GetObservationsFromAuthor("BigAuthor", 1);
        var page2 = _svc.GetObservationsFromAuthor("BigAuthor", 2);

        Assert.Equal(32, page1.Count);
        Assert.Single(page2);
    }
}
