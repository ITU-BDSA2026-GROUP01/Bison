using SimpleDB;
using System.CommandLine;

namespace Bison.CLI;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var rootCommand = Parsing.BuildRootCommand(Observations.Observe, Observations.Read, Comments.Comment, Comments.Discussion);
        return await rootCommand.InvokeAsync(args);
    }
}