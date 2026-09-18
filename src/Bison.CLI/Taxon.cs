namespace Bison.CLI;

public class Taxon
{
    // The properties of the Taxon class represent various attributes of a taxonomic entity, such as its unique identifier, 
    // parent and accepted name usage identifiers, taxonomic status, rank, scientific name, authorship, language, vernacular name, 
    // and whether it has been merged with another taxon. The Parent property allows for hierarchical relationships between taxa to be represented.
    public string TaxonId { get; set; } = "";
    public string? ParentNameUsageId { get; set; }
    public string? AcceptedNameUsageId { get; set; }
    public string? TaxonomicStatus { get; set; }
    public string? TaxonRank { get; set; }
    public string? ScientificName { get; set; }
    public string? ScientificNameAuthorship { get; set; }
    public string? Language { get; set; }
    public string? VernacularName { get; set; }
    public string? Merged { get; set; }

    public Taxon? Parent { get; set; }
}