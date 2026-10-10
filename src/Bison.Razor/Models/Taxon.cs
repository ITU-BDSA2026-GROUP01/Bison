namespace Bison.Razor.Models;

public class Taxon
{
    public int TaxonId { get; set; }
    public string dwc_TaxonID { get; set; } = "";
    public string? VernacularName { get; set; }

    public int? ParentId { get; set; }
    public Taxon? Parent { get; set; }

    public List<Taxon> Children { get; set; } = new();

    public override bool Equals(object? obj) => obj is Taxon t && t.TaxonId == TaxonId;
    public override int GetHashCode() => TaxonId.GetHashCode();

    public bool IsSubTaxon(Taxon ancestor)
    {
        var current = Parent;
        while (current != null)
        {
            if(current.TaxonId == ancestor.TaxonId)
            {
                return true;
            }
        }
        return false;
    }

    
}