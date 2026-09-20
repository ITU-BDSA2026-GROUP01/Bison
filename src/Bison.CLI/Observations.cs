using SimpleDB;
namespace Bison.CLI;

public static class Observations
{
    private static CSVDatabase<Cheep> observeDB =>
        CSVDatabase<Cheep>.GetInstance(DbPaths.Resolve("bison_observe_cli_db.csv"));
    public static void Read(string? location)
    {
            var observation = location is null
            ? observeDB.Read()
            :observeDB.Read().Where(c=> c.Location == location);
        
        UserInterface.PrintObservations(observation);
    }

    public static void Observe(string message, string location)
    {
        var cheeps = observeDB.Read().ToList();
        var nextId = cheeps.Count == 0 ? 1 : cheeps.Max(c => c.Id) + 1;

        var cheep = new Cheep(
            nextId,
            Environment.UserName,
            message,
            DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            location
            );

        observeDB.Store(cheep);
    }

    public static bool Exists(long id)
    {
        return observeDB.Read().Any(c => c.Id == id);
    }

    // This method is useful for testing purposes, to reset the state of the Observations class.
    public static void Reset()
    {
        // Clear the in-memory cache if you have one
        // If not, this can be empty
    }

    // This method is useful for testing purposes, to add an observation without going through the CLI. Wrapper Method. 
    public static void AddObservation(string message, string location)
    {
        Observe(message, location);
    }


}