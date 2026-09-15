namespace test;

using System;
using Xunit;
using Xunit.Abstractions;
using Bison.CLI;
using SimpleDB;

public class UnitTests
{
    private readonly ITestOutputHelper output;
    public UnitTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void StoringInvalidRecord()
    {
        var path = Path.Combine(Path.GetTempPath(), $"bison_test_{Guid.NewGuid():N}.csv");
        try
        {
            // Given: a database file that already has its header row.
            // Header must match the Cheep record's column names (Id, Author, Message, Timestamp).
            File.WriteAllText(path, "Id,Author,Message,Timestamp\n");
            var database = CSVDatabase<Bison.CLI.Cheep>.GetInstance(path);

            // When: an empty record and an invalid (null-bearing) record are stored.
            // (If Store rejects either, the test fails with that exception.)
            database.Store(new Bison.CLI.Cheep());
            database.Store(new Bison.CLI.Cheep(0, null!, null!, 0));

            // Then: both are persisted and read back as empty (non-null) strings
            // with a zero timestamp — CsvHelper serialises null and "" identically.
            var actual = database.Read().ToList();

            Console.WriteLine(actual);
            Assert.Equal(2, actual.Count);

            var empty = actual[0];
            Assert.Equal(string.Empty, empty.Author);
            Assert.Equal(string.Empty, empty.Message);
            Assert.Equal(0, empty.Timestamp);

            var invalid = actual[1];
            Assert.Equal(string.Empty, invalid.Author);
            Assert.Equal(string.Empty, invalid.Message);
            Assert.Equal(0, invalid.Timestamp);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public void UnixTimestampTest()
    {
        // Given: a cheep with a fixed, known unix timestamp.
        // 1690891760 == 2023-08-01 12:09:20 UTC (hand-verified with: date -u -d @1690891760).
        var ts = 1690891760L;
        var cheep = new Bison.CLI.Cheep(1, "alice", "Hello", ts);

        var expectedLocal = DateTimeOffset
            .FromUnixTimeSeconds(ts)
            .ToLocalTime();

        var originalOut = Console.Out;
        using var captured = new StringWriter();
        Console.SetOut(captured);

        try
        {
            UserInterface.PrintObservations(new[] { cheep });
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        var output = captured.ToString();

        Assert.Contains(expectedLocal.ToString(), output);
    }

    [Fact]
    public async System.Threading.Tasks.Task CommentInvalidIdTest()
    {
        var observeDb = Bison.CLI.DbPaths.Resolve("bison_observe_cli_db.csv");
        var commentDb = Bison.CLI.DbPaths.Resolve("bison_comment_cli_db.csv");
        var observeBackup = Path.GetTempFileName();
        var commentBackup = Path.GetTempFileName();
        File.Copy(observeDb, observeBackup, true);
        File.Copy(commentDb, commentBackup, true);

        try
        {
            var args = new[] { "comment", "0", "test message" };

            var originalOut = Console.Out;
            var originalErr = Console.Error;
            using var captured = new StringWriter();
            Console.SetOut(captured);
            Console.SetError(captured);

            int exitCode;
            try
            {
                exitCode = await Bison.CLI.Program.Main(args);
            }
            finally
            {
                Console.SetOut(originalOut);
                Console.SetError(originalErr);
            }

            var output = captured.ToString();

            Assert.Contains("does not exist", output);


            originalOut = Console.Out;
            originalErr = Console.Error;
            using var captured1 = new StringWriter();
            Console.SetOut(captured1);
            Console.SetError(captured1);

            args = ["comment", "1", "test message1"];
            try
            {
                exitCode = await Bison.CLI.Program.Main(args);
            }
            finally
            {
                Console.SetOut(originalOut);
                Console.SetError(originalErr);
            }

            output = captured1.ToString();

            // Observation 0 is not in the test's local DB -> the CLI reports that.
            Assert.DoesNotContain("does not exist", output);
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
