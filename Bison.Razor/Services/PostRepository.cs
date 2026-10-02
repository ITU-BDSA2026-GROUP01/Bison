using Bison.Razor.Models;
using Microsoft.Data.Sqlite;

namespace Bison.Razor.Services;

public class PostRepository
{
    private const int PageSize = 32;
    private readonly string _dbPath;

    public PostRepository(IConfiguration configuration)
    {
        _dbPath = configuration["BISONDBPATH"] ?? Path.Combine(Path.GetTempPath(), "bison.db");
    }
    private SqliteConnection GetConnection()
    {
        return new SqliteConnection($"Data Source={_dbPath}");
    }

    public List<ObservationViewModel> GetAllObservations(int page = 1)
    {
        
    }
}