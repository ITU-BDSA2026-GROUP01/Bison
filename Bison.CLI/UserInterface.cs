using System;
using System.Collections.Generic;

namespace Bison.CLI;

public static class UserInterface
{
    public static void PrintObservations(IEnumerable<SimpleDB.Cheep> cheeps)
    {
        foreach (var cheep in cheeps)
        {
            var localTime = DateTimeOffset
                .FromUnixTimeSeconds(cheep.Timestamp)
                .ToLocalTime();

            Console.WriteLine($"{cheep.Author} @ {localTime}: {cheep.Message}");
        }
    }
    
}
