using SimpleDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bison.CLI
{
    public static class Proposals
    {
        // Re-resolves the singleton (and re-binds the real db path) on every use,
        // so tests that point the singleton at a temp file don't leak into the app.
        private static CSVDatabase<Proposal> ProposalDB =>
            CSVDatabase<Proposal>.GetInstance(DbPaths.Resolve("bison_proposal_cli_db.csv"));

        public static void AddProposal(long observationId, string taxonId)
        {
            if (!Observations.Exists(observationId))
            {
                Console.WriteLine($"Observation with ID {observationId} does not exist.");
                return;
            }

            // Validate taxon ID using the taxonomy lookup
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

            ProposalDB.Store(proposal);
        }

        public static void ShowProposals(long observationId)
        {
            if (!Observations.Exists(observationId))
            {
                Console.WriteLine("Observation not found.");
                return;
            }

            var proposals = ProposalDB.Read()
                .Where(p => p.ObservationId == observationId)
                .ToList();

            if (proposals.Count == 0)
            {
                Console.WriteLine($"No proposals found for observation {observationId}.");
                return;
            }

            UserInterface.PrintProposals(observationId, proposals);
        }
    }

}