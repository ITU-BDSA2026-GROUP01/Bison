using Bison.CLI;

public static class Taxonomy
{
    public static Dictionary<string, Taxon> Lookup { get; private set; } = new();

    static Taxonomy()
    {
        // Auto-load taxonomy.csv when the class is first used
        Lookup = TaxonLoader.Load("../../data/taxonomy.csv");
    }

    public static bool Exists(string taxonId) =>
        Lookup.ContainsKey(taxonId);
}
