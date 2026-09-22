namespace test;

using System;
using System.IO;
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
            const int iterations = 500;

            for (int i = 0; i < iterations; i++)
            {
                var message = RandomMessage(rng);
                var location = RandomLocation(rng);

                await Bison.CLI.Program.Main(new[] { "observe", message, location });

                expectedMessages.Add(message);
            }

        using var service = new HttpService();

        var observations = await service.GetCheepsAsync();

        foreach (var expectedMessage in expectedMessages)
        {
            Assert.Contains(observations, c => c.Message == expectedMessage);
        }
        }
        finally
        {
            File.Copy(backup, observeDb, true);
            File.Delete(backup);
        }
    }
    private static string RandomMessage(Random rng)
    {
        var choices = new []
        {
           $"Random message {Guid.NewGuid():N}",
           "",
           "A",
           "Random",
           "øæå test",
           "!@#$%^&*()_+",
           new string('X', 100),
           $"Random-message-{rng.Next(1, 1000)}",
           $"Observation {DateTime.UtcNow.Ticks}"
        };
        return choices[rng.Next(choices.Length)];
    }

    private static string RandomLocation(Random rng)
    {
        var locations = new []
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
            "Moscow"
        };
        return locations[rng.Next(locations.Length)];
    }
    
    [Fact]
    public async Task CommentThenDiscussion()
    {
        // This test writes to both shared test DBs (an observation and a
        // comment), so back both up and restore them in finally.
        var observeDb = ServerData.Observations;
        var commentDb = ServerData.Comments;
        var observeBackup = Path.GetTempFileName();
        var commentBackup = Path.GetTempFileName();
        File.Copy(observeDb, observeBackup, true);
        File.Copy(commentDb, commentBackup, true);

        var message = $"E2E discussion {Guid.NewGuid():N}";
        var commentText = $"E2E comment {Guid.NewGuid():N}";

        try
        {
            await Bison.CLI.Program.Main(["observe", message, "DR Byen"]);

            using var service = new HttpService();
            var newId = (await service.GetCheepsAsync())
                .Where(c => c.Message == message)
                .Select(c => c.Id)
                .Single();

            var originalOut = Console.Out;
            using var captured = new StringWriter();
            Console.SetOut(captured);

            string discussionOutput;
            try
            {
                await Bison.CLI.Program.Main(["comment", newId.ToString(), commentText]);
                await Bison.CLI.Program.Main(["discussion", newId.ToString()]);
                discussionOutput = captured.ToString();
            }
            finally
            {
                Console.SetOut(originalOut);
            }

            Assert.Contains(commentText, discussionOutput);
        }
        finally
        {
            File.Copy(observeBackup, observeDb, true);
            File.Copy(commentBackup, commentDb, true);
            File.Delete(observeBackup);
            File.Delete(commentBackup);
        }
    }
}
