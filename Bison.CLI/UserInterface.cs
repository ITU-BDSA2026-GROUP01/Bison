using System;
using System.Collections.Generic;

namespace Bison.CLI;

public static class UserInterface
{
    public static void Read()
    {
        var file = "bison_observe_cli_db.csv";

        using var reader = new StreamReader(file);
        using var csv = new CsvHelper.CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture);

        csv.Context.RegisterClassMap<Program.CheepMap>();

        var cheeps = csv.GetRecords<Program.Cheep>();
        DisplayCheeps(cheeps);
    }
    
    public static void DisplayCheeps(IEnumerable<Program.Cheep> cheeps)
    {
        foreach (var cheep in cheeps)
        {
            var localTime = DateTimeOffset
                .FromUnixTimeSeconds(cheep.Timestamp)
                .ToLocalTime();

            Console.WriteLine(
                $"{cheep.Author} @ {localTime}: {cheep.Message}");
        }
    }
}
