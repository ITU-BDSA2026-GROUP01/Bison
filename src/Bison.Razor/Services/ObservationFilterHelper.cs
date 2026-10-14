using Bison.Razor.Models;

namespace Bison.Razor.Services;

public static class ObservationFilterHelper
{
    public static List<Observation> FilterByTaxon(Taxon root, IEnumerable<Observation> observations)
    {
        var input = Dafny.Sequence<Observation>.FromArray(observations.ToArray());
        var result = FilterWrapper.__default.FilterWrapped(root, input);
        return result.CloneAsArray().ToList();
    }
}