using System.CommandLine;

namespace Bison.CLI;

static class Parsing
{
	public static RootCommand BuildRootCommand(Action<string> observe, Action read, Action<long, string> comment, Action<long> discussion)
	{
		var messageArgument = new Argument<string>(
			name: "message",
			description: "The message to record as an observation");

		var observeCommand = new Command("observe", "Record a new observation")
		{
			messageArgument
		};
		observeCommand.SetHandler((message) => observe(message), messageArgument);

		var readCommand = new Command("read", "List recorded observations");
		readCommand.SetHandler(() => read());

		var observationIdArgument = new Argument<long>(
			name: "observationId",
			description: "The ID of the observation to comment on");

		var commentMessageArgument = new Argument<string>(
			name: "message",
			description: "The message to record as a comment");

		var commentCommand = new Command("comment", "Add a comment to an observation")
		{
			observationIdArgument,
			commentMessageArgument
		};
		commentCommand.SetHandler((observationId, message) => comment(observationId, message), observationIdArgument, commentMessageArgument);

		var discussionCommand = new Command("discussion", "List all observations and their comments")
		{
			observationIdArgument
		};
		discussionCommand.SetHandler((observationId) => discussion(observationId), observationIdArgument);


		var rootCommand = new RootCommand("Bison CLI");
		rootCommand.AddCommand(observeCommand);
		rootCommand.AddCommand(readCommand);
		rootCommand.AddCommand(commentCommand);
		rootCommand.AddCommand(discussionCommand);
		// No subcommand given -> default to Read, matching old behavior
		rootCommand.SetHandler(() => read());

		return rootCommand;
	}
}