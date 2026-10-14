using Bison.Razor.Models;

namespace Bison.Razor.Services;

public interface IObservationTaxonFilterService
{
    List<Observation> FilterByTaxon(Taxon root, IEnumerable<Observation> observations);
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