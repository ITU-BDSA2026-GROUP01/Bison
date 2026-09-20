using SimpleDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bison.CLI
{
    public static class Proposals
    // Re-resolves the singleton (and re-binds the real db path) on every use,
    // so tests that point the singleton at a temp file don't leak into the app.
    private static CSVDatabase<Proposal> ProposalDB =>
        CSVDatabase<Proposal>.GetInstance(DbPaths.Resolve("bison_proposal_cli_db.csv"));

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
