using System.Net;
using SimpleDB;

namespace Bison.CLI;

public static class Comments
{
    private static readonly CSVDatabase<Comment> CommentDB = new(DbPaths.Resolve("bison_comment_cli_db.csv"));

    public static void Comment(long observationId, string message)
    {
        if (!Observations.Exists(observationId))
        {
            Console.WriteLine($"Observation with ID {observationId} does not exist.");
            return;
        }
         
        var comment = new Comment(
            observationId,
            Environment.UserName,
            message,
            DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        );

        CommentDB.Store(comment);
    }

    public static void Discussion(long observationId)
{
        if (!Observations.Exists(observationId))
        {
            Console.WriteLine("Observation not found.");
            return;
        }

        var comments = CommentDB.Read()
            .Where(c => c.ObservationId == observationId)
            .ToList();

        if (comments.Count == 0)
        {
            Console.WriteLine($"No comments found for observation {observationId}.");
            return;
        }

        UserInterface.PrintComments(observationId, comments);
}
}

/*public sealed class CommentMap : ClassMap<Comment>
{
    public CommentMap()
    {
        Map(item => item.ObservationId).Name("ObservationId");
        Map(item => item.Message).Name("Message");
        Map(item => item.Timestamp).Name("Timestamp");
    }
}*/

