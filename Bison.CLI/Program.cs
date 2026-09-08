using SimpleDB;
using System.CommandLine;

namespace Bison.CLI;

partial class Program
{
    private static readonly CSVDatabase<Cheep> DB = new("bison_observe_cli_db.csv");

    static async Task<int> Main(string[] args)
    {
        var rootCommand = Parsing.BuildRootCommand(Observe, Read);
        return await rootCommand.InvokeAsync(args);
    }

    static void Read()
    {
        UserInterface.PrintObservations(DB.Read());
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