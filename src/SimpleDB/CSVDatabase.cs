
using CsvHelper;
using System.Globalization;

namespace SimpleDB;

public sealed class CSVDatabase<T> : IDatabaseRepository<T>
{
    private static CSVDatabase<T>? _instance; // singleton instance
   
    private string _dbPath = "";

    private CSVDatabase(string path)
    {
        _dbPath = path;
    }

    public static CSVDatabase<T> GetInstance(string path)
    {
        if (_instance == null)
        {
            _instance = new CSVDatabase<T>(path);
        }

        // Always (re)bind the current database file. The instance stays unique
        // per T (that's the Singleton part), but each caller decides which file
        // it operates on. If the path were only set on the first call, the first
        // caller's file would win forever and every later GetInstance(otherPath)
        // would silently read/write the wrong file.
        _instance._dbPath = path;
        return _instance;
    }

    public IEnumerable<T> Read(int? limit = null)
    {
        using var reader = new StreamReader(_dbPath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        return csv.GetRecords<T>().ToList();
    }

    public void Store(T record)
    {
        using var stream = new FileStream(
        _dbPath,
        FileMode.Append,
        FileAccess.Write,
        FileShare.Read);

        using var writer = new StreamWriter(stream);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        csv.WriteRecord(record);
        csv.NextRecord();

    }


    // Reset the singleton instance (for testing purposes)
    public static void Reset()
    {
        _instance = null;
    }

}

