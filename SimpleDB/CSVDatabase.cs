using CsvHelper;
using System.Globalization;

namespace SimpleDB;

public sealed class CSVDatabase<T> : IDatabaseRepository<T> 
{

    private readonly string bison_observe_cli_dbpath;

    public CSVDatabase (string path) 
    {
        bison_observe_cli_db = path;

    }

    public void Store (T record) 
    {

        
    }
}

