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

    public ObservationDTO? GetObservationsById(int id)
    {
        using var conn = GetConnection();
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT o.observation_id, o.text, o.pub_date, u.username
            FROM observation o
            JOIN user u ON o.author_id = u.user_id
            WHERE o.observation_id = @id;";

    cmd.Parameters.AddWithValue("@id", id);

    using var reader = cmd.ExecuteReader();
    if(!reader.Read())
        {
            return null;
        }
        return new ObservationDTO
        {
            Id = reader.GetInt32(0),
            Message = reader.GetString(1),
            Timestamp = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(2)).ToString("u"),
            Author = reader.GetString(3)
        };
    }

    //Ui model for comments connected to observations, containing author, message and timestamp
    public List<CommentDTO> GetCommentsForObservation(int observationId)
    {
        var result = new List<CommentDTO>();

        using var conn = GetConnection();
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT comment_id, observation_id, author, text, pub_date
            FROM comment
            WHERE observation_id = @observationId
            ORDER BY pub_date DESC;";

        cmd.Parameters.AddWithValue("@observationId", observationId);

        using var reader = cmd.ExecuteReader();
        
        while (reader.Read())
        {
            result.Add(new CommentDTO
            {
                Id = reader.GetInt32(0),
                ObservationId = reader.GetInt32(1),
                Author = reader.GetString(2),
                Message = reader.GetString(3),
                Timestamp = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(4)).ToString("u"),
                
            });
        }

        return result;
    }

    //Ui model for taxon proposals connected to observations, containing author, taxon id and timestamp
    public List<ProposalDTO> GetProposalsForObservation(int observationId)
{
    var result = new List<ProposalDTO>();

    using var conn = GetConnection();
    conn.Open();

    var cmd = conn.CreateCommand();
    cmd.CommandText = @"
        SELECT proposal_id, observation_id, author, taxon_id, pub_date
        FROM proposal
        WHERE observation_id = @observationId
        ORDER BY pub_date DESC;";

    cmd.Parameters.AddWithValue("@observationId", observationId);

    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        result.Add(new ProposalDTO
        {
            Id = reader.GetInt32(0),
            ObservationId = reader.GetInt32(1),
            Author = reader.GetString(2),
            TaxonId = reader.GetString(3),
            Timestamp = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(4)).ToString("u")
        });
    }

    return result;
}
}