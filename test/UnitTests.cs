namespace test;

using System;
using Xunit;
using Xunit.Abstractions;

public class UnitTests
{
    private readonly ITestOutputHelper output;
    public UnitTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public async System.Threading.Tasks.Task commentTest()
    {
        var args = new[] { "comment", "0", "\"test message\"" };

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

        // Observation 0 is not in the test's local DB -> the CLI reports that.
        Assert.Contains("does not exist", output);
    }
}
