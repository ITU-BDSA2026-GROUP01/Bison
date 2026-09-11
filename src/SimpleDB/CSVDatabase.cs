
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace SimpleDB;

public sealed class CSVDatabase<T> : IDatabaseRepository<T>
{
    private static CSVDatabase<T>? _instance; //uanset hvor mange instancer vi laver af CSVDatabase<T>, vil de alle refere tilbage til den STATIC CSVDatabase
    private readonly string bison_observe_cli_dbpath = "";

    private CSVDatabase(string path)
    {
        bison_observe_cli_dbpath = path;
    }

    public static CSVDatabase<T> getInstance()
    {
        if (_instance == null)
        {
            _instance = new CSVDatabase<T>(bison_observe_cli_dbpath);
        }
        return _instance; 
    }

    public IEnumerable<T> Read(int? limit = null)
    {
        using var reader = new StreamReader(bison_observe_cli_dbpath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        csv.Context.RegisterClassMap<CheepMap>();

        return csv.GetRecords<T>().ToList();
    }

    public void Store(T record)
    {
        using var stream = new FileStream(
        bison_observe_cli_dbpath,
        FileMode.Append,
        FileAccess.Write,
        FileShare.Read);

        using var writer = new StreamWriter(stream);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        csv.WriteRecord(record);
        csv.NextRecord();

    }
}

public sealed class CheepMap : ClassMap<Cheep>
{
    public CheepMap()
    {
        Map(item => item.Author).Name("Author");
        Map(item => item.Message).Name("Observation");
        Map(item => item.Timestamp).Name("Timestamp");
    }
}

public record Cheep(string Author, string Message, long Timestamp)
{
    public Cheep() : this(string.Empty, string.Empty, 0) { }
}

