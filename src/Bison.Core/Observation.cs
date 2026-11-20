namespace Bison.Core;

public class Observation : Post
{
    public int TaxonId { get; set; }
    public Taxon Taxon { get; set; } = null!;

    public List<Comment> Comments { get; set; } = new();
    public List<Proposal> Proposals { get; set; } = new();
    public string? PicturePath { get; set; }
    public string? PictureLicense { get; set; }
    public Taxon GetTaxon() => Taxon;
}