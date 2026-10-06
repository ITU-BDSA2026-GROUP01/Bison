using Bison.Razor.Models;
using Microsoft.Data.Sqlite;

namespace Bison.Razor.Services;

public class PostRepository : IPostRepository
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
    public List<ObservationDTO> GetAllObservations(int page = 1)
    {
        var result = new List<ObservationDTO>();

        using var conn = GetConnection();
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT o.observation_id, o.text, o.pub_date, u.username
            FROM observation o
            JOIN user u ON o.author_id = u.user_id
            ORDER BY o.pub_date DESC
            LIMIT @limit OFFSET @offset;";

        cmd.Parameters.AddWithValue("@limit", PageSize);
        cmd.Parameters.AddWithValue("@offset", (page - 1) * PageSize); // sørger for, at hver ny side viser et nyt udsnit, ikke de samme observationer igen. Btw OFFSET tæller fra 0, ikke fra 1. 


        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new ObservationDTO
            {
                Id = reader.GetInt32(0),
                Message = reader.GetString(1),
                Timestamp = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(2)).ToString("u"),
                Author = reader.GetString(3)
            });
        }

        return result;
    }

    public List<ObservationDTO> GetObservationsByAuthor(string author, int page = 1)
    {
        var result = new List<ObservationDTO>();

        using var conn = GetConnection();
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT o.observation_id, o.text, o.pub_date, u.username
            FROM observation o
            JOIN user u ON o.author_id = u.user_id
            WHERE u.username = @author
            ORDER BY o.pub_date DESC
            LIMIT @limit OFFSET @offset;";

        cmd.Parameters.AddWithValue("@limit", PageSize);
        cmd.Parameters.AddWithValue("@offset", (page - 1) * PageSize); 
        cmd.Parameters.AddWithValue("@author", author);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new ObservationDTO
            {
                Id = reader.GetInt32(0),
                Message = reader.GetString(1),
                Timestamp = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(2)).ToString("u"),
                Author = reader.GetString(3)
            });
        }
        return result;
    }
}