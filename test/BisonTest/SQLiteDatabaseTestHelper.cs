using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace BisonTest;

/// <summary>
/// Hermetic SQLite test fixture. Each test gets its own isolated temp .db file
/// (a fresh GUID-named file) created from the project's real schema, then
/// deleted in Dispose. No live server, no shared state, no real data files —
/// matching the project's test rules (server-free + hermetic).
///
/// The schema below mirrors Bison.Razor/Data/schema.sql exactly.
/// </summary>
public sealed class SQLiteDatabaseTestHelper : IDisposable
{
    // Mirrors Bison.Razor/Data/schema.sql (kept in sync with the app's DDL).
    private const string Schema = @"
create table user (
  user_id integer primary key autoincrement,
  username string not null,
  email string not null
);
create table observation (
  observation_id integer primary key autoincrement,
  author_id integer not null,
  text string not null,
  pub_date integer
);
create table comment(
  comment_id integer primary key autoincrement,
  observation_id integer not null,
  author string not null,
  text string not null,
  pub_date integer not null
);
create table proposal(
  proposal_id integer primary key autoincrement,
  observation_id integer not null,
  author string not null,
  taxon_id string not null,
  pub_date integer not null
);";

    /// <summary>Absolute path to the isolated temp database file.</summary>
    public string DbPath { get; }

    public SQLiteDatabaseTestHelper()
    {
        DbPath = Path.Combine(Path.GetTempPath(), $"bison_test_{Guid.NewGuid():N}.db");
        using var conn = Open();
        Execute(conn, Schema);
    }

    public void Dispose()
    {
        try { if (File.Exists(DbPath)) File.Delete(DbPath); }
        catch { /* best-effort cleanup; never mask the real test failure */ }
    }

    private SqliteConnection Open()
    {
        var conn = new SqliteConnection($"Data Source={DbPath}");
        conn.Open();
        return conn;
    }

    private static void Execute(SqliteConnection conn, string sql)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }

    // ── Typed insert helpers (return the new row's id) ────────────────────

    public int InsertUser(string username, string email)
    {
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO user (username, email) VALUES (@u, @e); SELECT last_insert_rowid();";
        cmd.Parameters.AddWithValue("@u", username);
        cmd.Parameters.AddWithValue("@e", email);
        return Convert.ToInt32(cmd.ExecuteScalar()!);
    }

    public int InsertObservation(int authorId, string text, long pubDate)
    {
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO observation (author_id, text, pub_date) VALUES (@a, @t, @d); SELECT last_insert_rowid();";
        cmd.Parameters.AddWithValue("@a", authorId);
        cmd.Parameters.AddWithValue("@t", text);
        cmd.Parameters.AddWithValue("@d", pubDate);
        return Convert.ToInt32(cmd.ExecuteScalar()!);
    }

    public int InsertComment(int observationId, string author, string text, long pubDate)
    {
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO comment (observation_id, author, text, pub_date) VALUES (@o, @a, @t, @d); SELECT last_insert_rowid();";
        cmd.Parameters.AddWithValue("@o", observationId);
        cmd.Parameters.AddWithValue("@a", author);
        cmd.Parameters.AddWithValue("@t", text);
        cmd.Parameters.AddWithValue("@d", pubDate);
        return Convert.ToInt32(cmd.ExecuteScalar()!);
    }

    public int InsertProposal(int observationId, string author, string taxonId, long pubDate)
    {
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO proposal (observation_id, author, taxon_id, pub_date) VALUES (@o, @a, @t, @d); SELECT last_insert_rowid();";
        cmd.Parameters.AddWithValue("@o", observationId);
        cmd.Parameters.AddWithValue("@a", author);
        cmd.Parameters.AddWithValue("@t", taxonId);
        cmd.Parameters.AddWithValue("@d", pubDate);
        return Convert.ToInt32(cmd.ExecuteScalar()!);
    }

    // ── Convenience: the standard seed (mirrors Bison.Razor/Data/dump.sql) ──

    /// <summary>Inserts the two users + two observations + one comment + one proposal
    /// that mirror the app's real Data/dump.sql. Returns the generated ids.</summary>
    public (int edu, int pet, int obs1, int obs2) SeedStandard()
    {
        int edu  = InsertUser("Eduard", "edka@itu.dk");
        int pet  = InsertUser("Peter",  "debloh@itu.dk");
        int obs1 = InsertObservation(edu, "A heron",    1690892208);
        int obs2 = InsertObservation(pet, "A big bird", 1690895308);
        InsertComment (obs1, "Rats", "wow amazing", 1690892208);
        InsertProposal(obs1, "Rats", "MSTSNM:Arter:a15367e4-f785-ea11-aa77-501ac539d1ea", 1690895308);
        return (edu, pet, obs1, obs2);
    }

    // ── Facade / service factories (all pointed at this helper's temp DB) ──

    private Microsoft.Extensions.Configuration.IConfiguration CreateConfig()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["BISONDBPATH"] = DbPath })
            .Build();
    }

    public Bison.Razor.Services.DBFacade CreateFacade()
        => new(CreateConfig());

    public Bison.Razor.Services.PostRepository CreatePostRepository()
        => new(CreateConfig());

    public Bison.Razor.Services.ObservationService CreateObservationService()
        => new(CreatePostRepository());

    public Bison.Razor.Services.PostService CreatePostService()
        => new(CreatePostRepository());
}
