
using System;
using System.Collections;
using System.IO;
using System.Text;
using Microsoft.VisualBasic.FileIO;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

using SimpleDB;
using System.Data;


partial class Program
{

    private static readonly CSVDatabase<SimpleDB.Cheep> DB = new CSVDatabase<Cheep>("bison_observe_cli_db.csv");
    static void Main(string[] args)
    {

        if (args.Length > 0)
        {
            if (args[0] == "observe")
            {
                if (args.Length > 1)
                {
                    Observe(args[1]);
                }
                else
                {
                    Console.WriteLine("No message is provided");
                }

            }
            else if (args[0] == "read")
            {
                Read();
            }
        }
        else
        {
            Read();
        }

    }
    

    static void Read()
    {
        IEnumerable<SimpleDB.Cheep> cheeps = DB.Read();

        foreach (var cheep in cheeps)
        {
            var localTime = DateTimeOffset
                .FromUnixTimeSeconds(cheep.Timestamp)
                .ToLocalTime();

            Console.WriteLine(
                $"{cheep.Author} @ {localTime}: {cheep.Message}");
        }
    }

    static void Observe(string message)
    {
        var cheep = new Cheep(
            Environment.UserName,
            message,
            DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        DB.Store(cheep);
    }
}
