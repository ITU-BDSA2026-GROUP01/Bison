using Microsoft.Data.Sqlite;
using Bison.Razor.Models;

namespace Bison.Razor.Services;

public class DBFacade
{
    private readonly string _dbPath;

    public DBFacade(IConfiguration config)
    {
        var envPath = config["BISONDBPATH"];
        _dbPath = envPath ?? Path.Combine(Path.GetTempPath(), "bison.db");
    }

    private SqliteConnection GetConnection()
    {
        return new SqliteConnection($"Data Source={_dbPath}");
    }

    public List<ObservationViewModel> GetAllObservations()
    {
        var result = new List<ObservationViewModel>();

        using var conn = GetConnection();
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT o.observation_id, o.text, o.pub_date, u.username
            FROM observation o
            JOIN user u ON o.author_id = u.user_id
            ORDER BY o.pub_date DESC;
        ";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new ObservationViewModel
            {
                Id = reader.GetInt32(0),
                Message = reader.GetString(1),
                Timestamp = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(2)).ToString("u"),
                Author = reader.GetString(3)
            });
        }

        return result;
    }

    public List<ObservationViewModel> GetObservationsByAuthor(string author)
    {
        var result = new List<ObservationViewModel>();

        using var conn = GetConnection();
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT o.observation_id, o.text, o.pub_date, u.username
            FROM observation o
            JOIN user u ON o.author_id = u.user_id
            WHERE u.username = @author
            ORDER BY o.pub_date DESC;
        ";

        cmd.Parameters.AddWithValue("@author", author);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new ObservationViewModel
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