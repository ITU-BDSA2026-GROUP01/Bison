using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bison.CLI
{
    public record Proposal(long ObservationId, string Author, string TaxonId, long Timestamp)
    {
        public Proposal() : this(0, string.Empty, string.Empty, 0) { }
    }

    public sealed class ProposalMap : ClassMap<Proposal>
    {
        public ProposalMap()
        {
            Map(item => item.ObservationId).Name("ObservationId");
            Map(item => item.Author).Name("Author");
            Map(item => item.TaxonId).Name("TaxonId");
            Map(item => item.Timestamp).Name("Timestamp"); 
        }
    }
}
