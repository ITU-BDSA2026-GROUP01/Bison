namespace test;

using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Bison.CLI;
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
            "  padded  message  "
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
            "Mixed Case COMMENT Test 123"
        };
        return comments[rng.Next(comments.Length)];
    }
}
