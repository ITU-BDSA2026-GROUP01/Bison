namespace Bison.CLI;

public static class Comments
{
    public static async Task Comment(long observationId, string message)
    {
        using var service = new HttpService();

        await Comment(observationId, message, service);
    }

    public static async Task Comment(
        long observationId,
        string message,
        ITHttpService service)
    {
        var observations = await service.GetCheepsAsync();

        if (!observations.Any(c => c.Id == observationId))
        {
            Console.WriteLine($"Observation with ID {observationId} does not exist.");
            return;
        }

        var comment = new Comment(
            observationId,
            Environment.UserName,
            message,
            DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        await service.SendCommentAsync(comment);
    }

    public static async Task Discussion(long observationId)
    {
        using var service = new HttpService();

        await Discussion(observationId, service);
    }

    public static async Task Discussion(
        long observationId,
        ITHttpService service)
    {
        var observations = await service.GetCheepsAsync();

        if (!observations.Any(c => c.Id == observationId))
        {
            Console.WriteLine("Observation not found.");
            return;
        }

        var comments = await service.GetCommentsAsync(observationId);

        if (comments.Count == 0)
        {
            Console.WriteLine($"No comments found for observation {observationId}.");
            return;
        }

        UserInterface.PrintComments(observationId, comments);
    }
}