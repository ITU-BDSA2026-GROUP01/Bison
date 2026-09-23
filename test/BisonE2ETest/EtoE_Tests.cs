namespace test;

using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Bison.CLI;
using CsvHelper;
using SimpleDB;

public class EtoE_Tests
{
    [Fact]
    public async Task ObserveThenReadFuzz()
    {
        var observeDb = ServerData.Observations;
        var backup = Path.GetTempFileName();
        File.Copy(observeDb, backup, true);

        var rng = new Random();
        var expectedMessages = new List<string>();

        try
        {
            const int iterations = 200;

            for (var i = 0; i < iterations; i++)
            {
                var message = RandomMessage(rng);
                var location = RandomLocation(rng);

                await Bison.CLI.Program.Main(["observe", message, location]);
                expectedMessages.Add(message);
            }

            using var service = new HttpService();
            var observations = await service.GetCheepsAsync();

            foreach (var expectedMessage in expectedMessages)
            {
                Assert.Contains(observations, c => c.Message == expectedMessage);
            }

            Assert.True(observations.Count >= expectedMessages.Count,
                "The service did not persist all fuzzed observations.");
        }
        finally
        {
            File.Copy(backup, observeDb, true);
            File.Delete(backup);
        }
    }

    [Fact]
    public async Task CommentThenDiscussionFuzz()
    {
        // This test writes to both shared test DBs (an observation and a
        // comment), so back both up and restore them in finally.
        var observeDb = ServerData.Observations;
        var commentDb = ServerData.Comments;
        var observeBackup = Path.GetTempFileName();
        var commentBackup = Path.GetTempFileName();
        File.Copy(observeDb, observeBackup, true);
        File.Copy(commentDb, commentBackup, true);

        var rng = new Random();
        var expectedCommentsByObservation = new Dictionary<long, List<string>>();
        var message = $"E2E discussion {Guid.NewGuid():N}";

        try
        {
            await Bison.CLI.Program.Main(["observe", message, "DR Byen"]);

            using var service = new HttpService();
            var observationId = (await service.GetCheepsAsync())
                .Where(c => c.Message == message)
                .Select(c => c.Id)
                .Single();

            expectedCommentsByObservation[observationId] = new List<string>();

            const int iterations = 200;
            for (var i = 0; i < iterations; i++)
            {
                var commentText = RandomComment(rng);
                await Bison.CLI.Program.Main(["comment", observationId.ToString(), commentText]);
                expectedCommentsByObservation[observationId].Add(commentText);
            }

            var actualComments = await service.GetCommentsAsync(observationId);
            foreach (var expectedComment in expectedCommentsByObservation[observationId])
            {
                Assert.Contains(actualComments, c => c.ObservationId == observationId && c.Message == expectedComment);
            }

            Assert.Equal(expectedCommentsByObservation[observationId].Count, actualComments.Count);
        }
        finally
        {
            File.Copy(observeBackup, observeDb, true);
            File.Copy(commentBackup, commentDb, true);
            File.Delete(observeBackup);
            File.Delete(commentBackup);
        }
    }


    [Fact]
    public async Task ProposalThenDiscussionFuzz()
    {
        var observeDb = ServerData.Observations;
        var proposalDb = ServerData.Proposals;
        var observeBackup = Path.GetTempFileName();
        var proposalBackup = Path.GetTempFileName();
        File.Copy(observeDb, observeBackup, true);
        File.Copy(proposalDb, proposalBackup, true);

        var rng = new Random();
        var expectedProposalsByObservation = new Dictionary<long, List<string>>();
        var message = $"E2E proposal {Guid.NewGuid():N}";

        try
        {
            await Bison.CLI.Program.Main(["observe", message, "DR Byen"]);

            using var service = new HttpService();
            var observationId = (await service.GetCheepsAsync())
                .Where(c => c.Message == message)
                .Select(c => c.Id)
                .Single();

            expectedProposalsByObservation[observationId] = new List<string>();

            var validTaxa = new []
            {
                "MSTSNM:Arter:c28811f4-f785-ea11-aa77-501ac539d1ea",
                "MSTSNM:Arter:3e4e67e4-f785-ea11-aa77-501ac539d1ea",
                "MSTSNM:Arter:495067e4-f785-ea11-aa77-501ac539d1ea",
            };

             const int iterations = 200;

        for (var i = 0; i < iterations; i++)
            {
                var taxonId = RandomProposal(rng);

                try
                {
                    await Bison.CLI.Program.Main(["proposal", observationId.ToString(), taxonId]);
                    expectedProposalsByObservation[observationId].Add(taxonId);
                }
                catch
                {
                    //invalid taxon IDs
                }
            }

    }
    finally
        {
        File.Copy(observeBackup, observeDb, true);
        File.Copy(proposalBackup, proposalDb, true);
        File.Delete(observeBackup);
        File.Delete(proposalBackup);
        }
    }
    private static string RandomMessage(Random rng)
    {
        var choices = new[]
        {
            "",
            "A",
            "Random",
            " ",
            "!@#$%^&*()_+",
            "øæå test",
            "emoji 🥀🥀",
            "quote \"hello\"",
            "very long message " + new string('X', 500),
            $"Random message {Guid.NewGuid():N}",
            $"Random-message-{rng.Next(1, 1000)}",
            $"Observation {DateTime.UtcNow.Ticks}",
            new string('Z', rng.Next(1, 200)),
            "Mixed Case TEXT 123",
            "  padded  message  ",
            "aaaaaaaaaaaaaaaaaaa",
            "aaaaaa aaaaaa",
        };

        return choices[rng.Next(choices.Length)];
    }

    private static string RandomLocation(Random rng)
    {
        var locations = new[]
        {
            "DR Byen",
            "Copenhagen",
            "New York",
            "Tokyo",
            "London",
            "Berlin",
            "Paris",
            "Sydney",
            "São Paulo",
            "Moscow",
            "Nørrebro",
            "Vesterbro",
            "Aarhus",
            "Oslo"
        };
        return locations[rng.Next(locations.Length)];
    }

    private static string RandomComment(Random rng)
    {
        var comments = new[]
        {
            "",
            "A",
            "Random",
            "Interesting observation",
            $"Comment {Guid.NewGuid():N}",
            $"Random-{rng.Next()}",
            "øæå test",
            "!@#$%^&*()_+",
            "emoji 🥀🥀",
            "very long comment test " + new string('Y', 500),
            "Mixed Case COMMENT Test 123",
            "aaaaaaaaaaaaaaaaaaa",
            "aaaaaa aaaaaa",
        };
        return comments[rng.Next(comments.Length)];
    }

    private static string RandomProposal(Random rng)
    {
          var validTaxa = new[]
    {
        "MSTSNM:Arter:3e4e67e4-f785-ea11-aa77-501ac539d1ea",
        "MSTSNM:Arter:495067e4-f785-ea11-aa77-501ac539d1ea",
        "MSTSNM:Arter:a15367e4-f785-ea11-aa77-501ac539d1ea",
    };

    if(rng.Next(100)<95)
        {
            return validTaxa[rng.Next(validTaxa.Length)];
        }

        return $"INVALID-{Guid.NewGuid()}";
    }
}
