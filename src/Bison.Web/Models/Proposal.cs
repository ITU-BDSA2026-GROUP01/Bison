namespace Bison.Razor.Models;

public class Proposal : Post
{
    public int ObservationId { get; set; }
    public Observation Observation { get; set; } = null!;

    public int TaxonId { get; set; }
    public Taxon Taxon { get; set; } = null!;
}