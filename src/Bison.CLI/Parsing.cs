using System.CommandLine;

namespace Bison.CLI;

static class Parsing
{
	public static RootCommand BuildRootCommand(Action<string, string> observe, Action<string?> read, Action<long, string> comment, Action<long> discussion)
	{
		var messageArgument = new Argument<string>(
			name: "message",
			description: "The message to record as an observation");

		var locationArgument = new Argument<string> (
			name: "location",
			description: "The location where the observation was made");

        var observeCommand = new Command("observe", "Record a new observation")
		{
			messageArgument,
			locationArgument
		};
		observeCommand.SetHandler((message, location) => observe(message, location), messageArgument, locationArgument);


		var locationOption = new Option<string?> (
			name: "--location",
			description: "Filter observations by location"
		);
		

		var readCommand = new Command("read", "List recorded observations")
		{
			locationOption
		};

		readCommand.SetHandler((location) => read(location), locationOption);



		var observationIdArgument = new Argument<long>(
			name: "observationId",
			description: "The ID of the observation to comment on");
		var taxonIdArgument = new Argument<string>(
			name: "taxonId",
			description: "The taxon ID being proposed for the observation");
		var commentMessageArgument = new Argument<string>(
			name: "message",
			description: "The message to record as a comment");

        


        var commentCommand = new Command("comment", "Add a comment to an observation")
		{
			observationIdArgument,
			commentMessageArgument
		};
		commentCommand.SetHandler((observationId, message) => comment(observationId, message), observationIdArgument, commentMessageArgument);

        var proposalCommand = new Command("proposal", "Add a taxon proposal to an observation")
		{
			observationIdArgument,
			taxonIdArgument
		};

        proposalCommand.SetHandler(
            (observationId, taxonId) => Proposals.AddProposal(observationId, taxonId),
            observationIdArgument,
            taxonIdArgument
        );

        var proposalsCommand = new Command("proposals", "List all proposals for an observation")
		{
			 observationIdArgument
		};

        proposalsCommand.SetHandler(
            (observationId) => Proposals.ShowProposals(observationId),
            observationIdArgument
        );



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
        rootCommand.AddCommand(proposalCommand);
        rootCommand.AddCommand(proposalsCommand);
        // No subcommand given -> default to Read, matching old behavior
        rootCommand.SetHandler(() => read(null));

		return rootCommand;
	}
}