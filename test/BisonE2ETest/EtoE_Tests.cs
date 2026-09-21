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
    public async Task ObserveThenRead()
    {
        var observeDb = ServerData.Observations;
        var backup = Path.GetTempFileName();
        File.Copy(observeDb, backup, true);

        var message = $"E2E round trip {Guid.NewGuid():N}";

        var originalOut = Console.Out;
        using var captured = new StringWriter();
        Console.SetOut(captured);

        string readOutput;
        try
        {
            await Bison.CLI.Program.Main(["observe", message, "DR Byen"]);

            await Bison.CLI.Program.Main(["read"]);
            readOutput = captured.ToString();
        }
        finally
        {
            Console.SetOut(originalOut);
            File.Copy(backup, observeDb, true);
            File.Delete(backup);
        }

        // Then: the new observation is listed with its message.
        Assert.Contains(message, readOutput);
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
