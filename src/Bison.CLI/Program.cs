using System.CommandLine;

namespace Bison.CLI;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        Taxonomy.Initialize("../../data/taxonomy.csv"); // Initialize the taxonomy lookup at the start of the program, to avoid null reference issues later on.

        var rootCommand = Parsing.BuildRootCommand(Observations.Observe, Observations.Read, Comments.Comment, Comments.Discussion);
        return await rootCommand.InvokeAsync(args);
    }
}