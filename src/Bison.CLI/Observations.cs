using SimpleDB;
namespace Bison.CLI;

public static class Observations
{
     private static readonly CSVDatabase<Cheep> observeDB = new(DbPaths.Resolve("bison_observe_cli_db.csv"));

     public static void Read()
    {
        UserInterface.PrintObservations(observeDB.Read());
    }

    public static void Observe(string message)
    {
        var cheeps = observeDB.Read().ToList();
        var nextId = cheeps.Count == 0 ? 1 : cheeps.Max(c => c.Id) + 1;

        var cheep = new Cheep(
            nextId,
            Environment.UserName,
            message,
            DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            );

        observeDB.Store(cheep);
    }
     
     public static bool Exists(long id)
    {
      return observeDB.Read().Any(c => c.Id == id);  
    } 
}