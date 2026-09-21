namespace Bison.CLI;

public static class Observations
{
    public static async Task Read(string? location)
    {
        using var service = new HttpService();
        var all = await service.GetCheepsAsync();
        var cheeps = location is null
            ? all
            : all.Where(c => c.Location == location);

        UserInterface.PrintObservations(cheeps);
    }

    public static async Task Observe(string message, string location)
    {
        using var service = new HttpService();

        await Observe (message, location, service);
    }

    public static async Task Observe (
        string message,
        string location,
        ITHttpService service)

        {
            var existing = await service.GetCheepsAsync();
            var nextId = existing.Count == 0 ? 1 : existing.Max(c => c.Id) + 1;

            var cheep = new Cheep(
            nextId,
            Environment.UserName,
            message,
            DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            location);

        await service.SendCheepAsync(cheep); 
    }

    public static async Task<bool> Exists(long id)
    {
        using var service = new HttpService();
        var all = await service.GetCheepsAsync();
        return all.Any(c => c.Id == id);
    }
}
