using Bison.Razor.Models;

namespace Bison.Razor.Services;

public interface IObservationTaxonFilterService
{
    /// <summary>
    /// Returns the observations whose taxon is <paramref name="root"/> or one of its
    /// descendants. A null root means "no filter" and returns everything.
    /// </summary>
    List<Observation> FilterByTaxon(Taxon? root, IEnumerable<Observation> observations);
}

/// <summary>
/// Thin wrapper around the Dafny-verified filter (verification/ObservationFilter.dfy).
/// All matching is done by the generated FilterWrapper.__default.FilterWrapped; this class
/// only converts between .NET collections and Dafny sequences.
/// </summary>
public class ObservationTaxonFilterService : IObservationTaxonFilterService
{
    public List<Observation> FilterByTaxon(Taxon? root, IEnumerable<Observation> observations)
    {
        if (root == null)
            return observations.ToList();

        // The Dafny model has non-null Taxon/Observation types, so observations
        // without a taxon cannot be passed in (they can never match anyway).
        var candidates = observations.Where(o => o.Taxon != null).ToArray();

        var verified = FilterWrapper.__default.FilterWrapped(
            root,
            Dafny.Sequence<Observation>.FromArray(candidates));

        return verified.ToList();
    }
}