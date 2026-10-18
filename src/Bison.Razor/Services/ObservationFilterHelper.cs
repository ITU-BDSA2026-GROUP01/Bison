using Bison.Razor.Models;

namespace Bison.Razor.Services;

public interface IObservationTaxonFilterService
{
    List<Observation> FilterByTaxon(Taxon root, IEnumerable<Observation> observations);
}

public static class ObservationFilterHelper
{
    public static List<Observation> FilterByTaxon(Taxon root, IEnumerable<Observation> observations)
    {
        if (root == null)
            return observations.ToList();

        return observations
            .Where(o => o.Taxon != null && (o.Taxon == root || o.Taxon.IsSubTaxon(root)))
            .ToList();
    }
}

public class ObservationTaxonFilterService : IObservationTaxonFilterService
{
    public List<Observation> FilterByTaxon(Taxon root, IEnumerable<Observation> observations)
    {
        if (root == null)
            return observations.ToList();

        return ObservationFilterHelper.FilterByTaxon(root, observations);
    }
}