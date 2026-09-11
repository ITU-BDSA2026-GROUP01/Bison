using SimpleDB;
using System.CommandLine;

namespace Bison.CLI;

partial class Program
{
    //private static readonly CSVDatabase<Cheep> ObserveDB = new("bison_observe_cli_db.csv");
    
    //private static readonly CSVDatabase<Comment> CommentDB = new("bison_comment_cli_db.csv");    

    static async Task<int> Main(string[] args)
    {
        var rootCommand = Parsing.BuildRootCommand(Observations.Observe, Observations.Read, Comments.Comment, Comments.Discussion);
        return await rootCommand.InvokeAsync(args);
    }

   /* static void Read()
    {
        UserInterface.PrintObservations(ObserveDB.Read());
    }

    static void Observe(string message)
    {
        var cheeps = ObserveDB.Read().ToList();
        var nextId = cheeps.Count == 0 ? 1 : cheeps.Max(c => c.Id) + 1;

        var cheep = new Cheep(
            nextId,
            Environment.UserName,
            message,
            DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            );

        ObserveDB.Store(cheep);
    }*/
}