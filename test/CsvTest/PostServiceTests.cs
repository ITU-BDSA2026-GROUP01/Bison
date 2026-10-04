using Bison.Razor.Services;
using BisonTest;

namespace CsvTest;

/// <summary>
/// Tests the PostService (service layer) against a hermetic SQLite DB.
/// PostService exposes a single method: GetObservations(page).
/// </summary>
public class PostServiceTests : IDisposable
{
    private readonly SQLiteDatabaseTestHelper _db;
    private readonly PostService _svc;
    private int _obs1, _obs2;

    public PostServiceTests()
    {
        _db = new SQLiteDatabaseTestHelper();
        (_, _, _obs1, _obs2) = _db.SeedStandard();
        _svc = new PostService(_db.CreateFacade());
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public void GetObservations_ReturnsAllSeeded_NewestFirst()
    {
        var result = _svc.GetObservations();

        Assert.Equal(2, result.Count);
        Assert.Equal(_obs2, result[0].Id);   // newest (Peter)
        Assert.Equal(_obs1, result[1].Id);
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

    [Fact]
    public void GetObservations_Page1_CapsAtPageSize()
    {
        // 2 seeded; add 32 → 34 total → page 1 caps at 32.
        int big = _db.InsertUser("BigGuy", "big@x.dk");
        for (int i = 0; i < 32; i++)
            _db.InsertObservation(big, $"bulk {i}", 1687000000 + i);

        var page1 = _svc.GetObservations(1);

        Assert.Equal(32, page1.Count);
    }

    [Fact]
    public void GetObservations_PageBeyondData_ReturnsEmpty()
    {
        Assert.Empty(_svc.GetObservations(page: 99));
    }
}
