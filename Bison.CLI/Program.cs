
using SimpleDB;

namespace Bison.CLI;

partial class Program
{
    private static readonly CSVDatabase<Cheep> DB = new("bison_observe_cli_db.csv");

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
