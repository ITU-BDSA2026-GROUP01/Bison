using System.CommandLine;

namespace Bison.CLI;

static class Parsing
{
	public static RootCommand BuildRootCommand(Action<string> observe, Action read)
	{
		var messageArgument = new Argument<string>(
			name: "message",
			description: "The message to record as a cheep");

		var observeCommand = new Command("observe", "Record a new cheep")
		{
			messageArgument
		};
		observeCommand.SetHandler((message) => observe(message), messageArgument);

		var readCommand = new Command("read", "List recorded cheeps");
		readCommand.SetHandler(() => read());

		var rootCommand = new RootCommand("Bison CLI");
		rootCommand.AddCommand(observeCommand);
		rootCommand.AddCommand(readCommand);

		// No subcommand given -> default to Read, matching old behavior
		rootCommand.SetHandler(() => read());

		return rootCommand;
	}
}