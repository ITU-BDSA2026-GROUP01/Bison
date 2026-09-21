namespace Bison.CLI;

public static class Comments
{
    public static async Task Comment(long observationId, string message)
    {
        if (!await Observations.Exists(observationId))
        {
            Console.WriteLine($"Observation with ID {observationId} does not exist.");
            return;
        }

        using var service = new HttpService();
        var comment = new Comment(
            observationId,
            Environment.UserName,
            message,
            DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        await service.SendCommentAsync(comment);
    }

    public static async Task Discussion(long observationId)
    {
        if (!await Observations.Exists(observationId))
        {
            Console.WriteLine("Observation not found.");
            return;
        }

        using var service = new HttpService();
        var comments = await service.GetCommentsAsync(observationId);

        if (comments.Count == 0)
        {
            Console.WriteLine($"No comments found for observation {observationId}.");
            return;
        }

        UserInterface.PrintComments(observationId, comments);
    }
}
