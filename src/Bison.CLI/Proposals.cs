using System;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading.Tasks;

namespace Bison.CLI
{
    public static class Proposals
    {
        // ------------------------------------------------------------
        // 1. REAL HTTP VERSION (used by CLI)
        // ------------------------------------------------------------
        public static async Task AddProposal(long observationId, string taxonId)
        {
            using var service = new HttpService();
            await AddProposal(observationId, taxonId, service);
        }

        // ------------------------------------------------------------
        // 2. TESTABLE VERSION (used by FakeHttpService)
        // ------------------------------------------------------------
        public static async Task AddProposal(long observationId, string taxonId, ITHttpService service)
        {
            // Validate observation ID via web service
            var observations = await service.GetObservationsAsync();
            if (!observations.Any(o => o.Id == observationId))
            {
                Console.WriteLine($"Observation with ID {observationId} does not exist.");
                return;
            }

            // Validate taxon ID using taxonomy lookup
            if (!Taxonomy.Exists(taxonId))
            {
                Console.WriteLine($"Taxon ID {taxonId} is invalid.");
                return;
            }

            var proposal = new Proposal(
                observationId,
                Environment.UserName,
                taxonId,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            );

            await service.PostProposalAsync(proposal);
        }

        // ------------------------------------------------------------
        // SHOW PROPOSALS
        // ------------------------------------------------------------
        public static async Task ShowProposals(long observationId)
        {
            using var service = new HttpService();

            var observations = await service.GetObservationsAsync();
            if (!observations.Any(o => o.Id == observationId))
            {
                Console.WriteLine("Observation not found.");
                return;
            }

            var proposals = await service.GetProposalsAsync(observationId);

            if (proposals.Count == 0)
            {
                Console.WriteLine($"No proposals found for observation {observationId}.");
                return;
            }

            UserInterface.PrintProposals(observationId, proposals);
        }
    }
}
