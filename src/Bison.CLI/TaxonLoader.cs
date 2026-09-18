using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace Bison.CLI;

public class TaxonMap : ClassMap<Taxon>
{

    public TaxonMap()
    { // The TaxonMap class defines the mapping between the properties of the Taxon class and the corresponding column names in a CSV file.
        Map(t => t.TaxonId).Name("dwc:taxonID");
        Map(t => t.ParentNameUsageId).Name("dwc:parentNameUsageID");
        Map(t => t.AcceptedNameUsageId).Name("dwc:acceptedNameUsageID");
        Map(t => t.TaxonomicStatus).Name("dwc:taxonomicStatus");
        Map(t => t.TaxonRank).Name("dwc:taxonRank");
        Map(t => t.ScientificName).Name("dwc:scientificName");
        Map(t => t.ScientificNameAuthorship).Name("dwc:scientificNameAuthorship");
        Map(t => t.Language).Name("dcterms:language");
        Map(t => t.VernacularName).Name("dwc:vernacularName");
        Map(t => t.Merged).Name("clb:merged");
    }
}

public static class TaxonLoader
{   // The TaxonLoader class provides a static method to load taxonomic data from a CSV file and establish parent-child relationships between taxa based on their identifiers.
    public static Dictionary<string, Taxon> Load(string path)
    {   // The Load method reads taxonomic data from a CSV file specified by the path parameter, creates Taxon objects, and establishes parent-child relationships based on the ParentNameUsageId property.
        using var reader = new StreamReader(path);
        // Create a CsvReader to read the CSV data from the specified file path, using the invariant culture for consistent parsing of data.
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        // Register the TaxonMap class to define the mapping between the Taxon class properties and the corresponding CSV column names.
        csv.Context.RegisterClassMap<TaxonMap>();
        // Read the records from the CSV file and convert them into a list of Taxon objects.
        var taxa = csv.GetRecords<Taxon>().ToList();
        // Create a lookup dictionary for taxa by their TaxonId, filtering out any taxa with empty or whitespace TaxonId values.
        var lookup = taxa
            .Where(t => !string.IsNullOrWhiteSpace(t.TaxonId))
            .GroupBy(t => t.TaxonId)
            .ToDictionary(g => g.Key, g => g.First());
        // Establish parent-child relationships between taxa by setting the Parent property of each taxon based on the ParentNameUsageId property, if it exists in the lookup dictionary.
        foreach (var taxon in lookup.Values)
        {     // If the taxon has a non-empty ParentNameUsageId and that ID exists in the lookup dictionary, set the Parent property of the taxon to the corresponding parent Taxon object.
            if (!string.IsNullOrWhiteSpace(taxon.ParentNameUsageId) &&
                lookup.ContainsKey(taxon.ParentNameUsageId))
            {
                taxon.Parent = lookup[taxon.ParentNameUsageId];
            }
        }

        return lookup;
    }
}