using System;
using System.Collections.Generic;

namespace Bison.CLI;

public static class UserInterface
{
    public static void PrintObservations(IEnumerable<Cheep> cheeps)
    {
        foreach (var cheep in cheeps)
        {
            var localTime = DateTimeOffset
                .FromUnixTimeSeconds(cheep.Timestamp)
                .ToLocalTime();

            Console.WriteLine($"({cheep.Id}) {cheep.Author} @ {localTime} [{cheep.Location}]: {cheep.Message}");
        }
    }

    public static void PrintComments(long observationId, IEnumerable<Comment> comments)
    {
        Console.WriteLine($"Comments for Observation {observationId}:");
        foreach (var comment in comments)
        {
            var localTime = DateTimeOffset
                .FromUnixTimeSeconds(comment.Timestamp)
                .ToLocalTime();

            Console.WriteLine($"{comment.Author} @ {localTime}: {comment.Message}");
        }
    }

    public static void PrintProposals(long observationId, IEnumerable<Proposal> proposals)
    {
        Console.WriteLine($"Proposals for Observation {observationId}:");
        foreach (var proposal in proposals)
        {
            var localTime = DateTimeOffset
                .FromUnixTimeSeconds(proposal.Timestamp)
                .ToLocalTime();
            Console.WriteLine($"{proposal.Author} @ {localTime}: Proposed Taxon Id {proposal.TaxonId}");
        }
    }
}
