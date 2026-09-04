using System;
using System.Collections.Generic;

namespace Bison.CLI;

public class UserInterface
{
    public static void DisplayCheeps(List<Program.Cheep> cheeps)
    {
        foreach (var cheep in cheeps)
        {
            var localTime = DateTimeOffset
                .FromUnixTimeSeconds(cheep.Timestamp)
                .ToLocalTime();

            Console.WriteLine($"Author: {cheep.Author}");
            Console.WriteLine($"Timestamp: {localTime}");
            Console.WriteLine($"Message: {cheep.Message}");
            Console.WriteLine(new string('-', 40));
        }
    }
}
