namespace Bison.Razor.Models;

public class Taxon
{
    public int Id { get; set; }
    public string DwcTaxonId { get; set; } = "";
    public string DanishVernacularName { get; set; } = "";

    public int? ParentId { get; set; }
    public Taxon? Parent { get; set; }

    public List<Taxon> Children { get; set; } = new();
}