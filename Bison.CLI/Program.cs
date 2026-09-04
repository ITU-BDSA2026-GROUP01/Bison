
using System;
using System.Collections;
using System.IO;
using System.Text;
using Microsoft.VisualBasic.FileIO;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;


class Program {
    public record Cheep(string Author, string Message, long Timestamp)
    {
        public Cheep() : this(string.Empty, string.Empty, 0) { }
    }

    static void Main(string[] args)
    {
        if (args.Length > 0) {
            if (args[0] == "observe") {
                if (args.Length > 1) {
                    Observe(args[1]);
                } else {
                    Console.WriteLine("No message is provided");
                }
                
            } else if (args[0] == "read") {
                Read();
            }
        } else {
            Read();
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

    static void Read()
{
    var file = "bison_observe_cli_db.csv";

    using var reader = new StreamReader(file);
    using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

    csv.Context.RegisterClassMap<CheepMap>();

    foreach (var cheep in csv.GetRecords<Cheep>())
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
    var file = "bison_observe_cli_db.csv";

    var cheep = new Cheep(
        Environment.UserName,
        message,
        DateTimeOffset.UtcNow.ToUnixTimeSeconds());

    using var stream = new FileStream(
        file,
        FileMode.Append,
        FileAccess.Write,
        FileShare.Read);

    using var writer = new StreamWriter(stream);
    using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

    csv.WriteRecord(cheep);
    csv.NextRecord();
    }
}
