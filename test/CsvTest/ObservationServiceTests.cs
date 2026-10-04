using Bison.Razor.Services;
using BisonTest;

namespace CsvTest;

/// <summary>
/// Tests the ObservationService (service layer) against a hermetic SQLite DB.
/// Verifies both public methods delegate to DBFacade and return the right data.
/// </summary>
public class ObservationServiceTests : IDisposable
{
    private readonly SQLiteDatabaseTestHelper _db;
    private readonly ObservationService _svc;
    private int _edu, _pet, _obs1, _obs2;

    public ObservationServiceTests()
    {
        _db = new SQLiteDatabaseTestHelper();
        (_edu, _pet, _obs1, _obs2) = _db.SeedStandard();
        _svc = new ObservationService(_db.CreateFacade());
    }

    public void Dispose() => _db.Dispose();

    // ── GetObservations ───────────────────────────────────────────────

    [Fact]
    public void GetObservations_ReturnsAllSeeded_NewestFirst()
    {
        var result = _svc.GetObservations();

        Assert.Equal(2, result.Count);
        // pub_date DESC: obs2 (1690895308, Peter) is newer than obs1 (1690892208, Eduard).
        Assert.Equal(_obs2, result[0].Id);
        Assert.Equal(_obs1, result[1].Id);
    }

    [Fact]
    public void GetObservations_DefaultPageIs1()
    {
        var result = _svc.GetObservations();

        Assert.Equal(2, result.Count);
        Assert.All(result, o => Assert.InRange(o.Id, 1, int.MaxValue));
    }

    [Fact]
    public void GetObservations_Pagination()
    {
        // 2 seeded; add 32 → 34 total → page 1 holds 32, page 2 holds 2.
        int big = _db.InsertUser("BigGuy", "big@x.dk");
        for (int i = 0; i < 32; i++)
            _db.InsertObservation(big, $"bulk {i}", 1689000000 + i);

        var page1 = _svc.GetObservations(1);
        var page2 = _svc.GetObservations(2);

        Assert.Equal(32, page1.Count);
        Assert.Equal(2, page2.Count);
    }

    [Fact]
    public void GetObservations_EachHasAuthorMessageAndTimestamp()
    {
        var result = _svc.GetObservations();

        Assert.All(result, o =>
        {
            Assert.False(string.IsNullOrEmpty(o.Author));
            Assert.False(string.IsNullOrEmpty(o.Message));
            Assert.False(string.IsNullOrEmpty(o.Timestamp));
        });
    }

    // ── GetObservationsFromAuthor ─────────────────────────────────────

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
