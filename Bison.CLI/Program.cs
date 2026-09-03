
using System;
using System.Collections;
using System.IO;
using System.Text;
using Microsoft.VisualBasic.FileIO;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;


class Program {
    public record Cheap(string Author, string Message, long Timestamp)
    {
        public Cheap() : this(string.Empty, string.Empty, 0) { }
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
    public sealed class CheapMap : ClassMap<Cheap>
{
    public CheapMap()
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

    csv.Context.RegisterClassMap<CheapMap>();

    foreach (var cheap in csv.GetRecords<Cheap>())
    {
        var localTime = DateTimeOffset
            .FromUnixTimeSeconds(cheap.Timestamp)
            .ToLocalTime();

        Console.WriteLine(
            $"{cheap.Author} @ {localTime}: {cheap.Message}");
    }
}

   static void Observe(string message)
{
    var file = "bison_observe_cli_db.csv";

    var cheap = new Cheap(
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

    csv.WriteRecord(cheap);
    csv.NextRecord();
}
}
