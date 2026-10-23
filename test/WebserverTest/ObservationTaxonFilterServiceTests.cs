using Bison.Razor.Models;
using Bison.Razor.Services;

namespace WebserverTest;


public class ObservationTaxonFilterServiceTests
{
    // The service under test. It has no dependencies, so we can just instantiate it directly.
    private readonly ObservationTaxonFilterService _svc = new();

    // Sample taxon tree for testing:
    private readonly Taxon _life, _animal, _bird, _heron, _mammal, _rat, _plant;

    public ObservationTaxonFilterServiceTests()
    {
        // Build a simple taxon tree for testing:
        _life = MakeTaxon(1, null);
        _animal = MakeTaxon(2, _life);
        _bird = MakeTaxon(3, _animal);
        _heron = MakeTaxon(4, _bird);
        _mammal = MakeTaxon(5, _animal);
        _rat = MakeTaxon(6, _mammal);
        _plant = MakeTaxon(7, _life);
    }

    // Helper to create a Taxon with a given id and optional parent. The ParentId is set accordingly.
    private static Taxon MakeTaxon(int id, Taxon? parent) =>
        new() { TaxonId = id, ParentId = parent?.TaxonId, Parent = parent };

    // Helper to create an Observation with a given id and taxon.
    private static Observation Obs(int id, Taxon taxon) =>
        new() { PostId = id, Taxon = taxon, TaxonId = taxon.TaxonId };

    // Helper to extract the PostIds from a list of Observations, for easier assertions.
    private static List<int> Ids(IEnumerable<Observation> obs) => obs.Select(o => o.PostId).ToList();

    // Sample observations for testing. The ids are arbitrary, but the taxon relationships are important.
    private List<Observation> Sample() => new()
    {
        Obs(1, _heron),
        Obs(2, _rat),
        Obs(3, _plant),
        Obs(4, _bird),
        Obs(5, _heron),
    };

    // Test cases for the FilterByTaxon method, covering various scenarios.
    [Fact]
    public void FilterByTaxon_ExactTaxon_ReturnsOnlyThatTaxon()
    {
        var result = _svc.FilterByTaxon(_plant, Sample());

        Assert.Equal(new[] { 3 }, Ids(result));
    }

    // Test that filtering by a taxon includes all observations of its descendants.
    [Fact]
    public void FilterByTaxon_Ancestor_IncludesAllDescendants()
    {
        var result = _svc.FilterByTaxon(_bird, Sample());

        // heron observations (1, 5) are sub-taxa of bird, obs 4 is bird itself
        Assert.Equal(new[] { 1, 4, 5 }, Ids(result));
    }

    // Test that filtering by a taxon includes observations of its immediate children.
    [Fact]
    public void FilterByTaxon_DeepAncestor_WalksWholeParentChain()
    {
        // rat -> mammal -> animal: two levels up
        var result = _svc.FilterByTaxon(_animal, Sample());

        Assert.Equal(new[] { 1, 2, 4, 5 }, Ids(result));
    }

    // Test that filtering by a taxon does not include observations of sibling taxa.
    [Fact]
    public void FilterByTaxon_Root_ReturnsEverything()
    {
        var result = _svc.FilterByTaxon(_life, Sample());

        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, Ids(result));
    }

    // Test that filtering by a taxon does not include observations of its ancestors.
    [Fact]
    public void FilterByTaxon_Leaf_DoesNotMatchItsAncestors()
    {
        // Filtering by heron must not return the plain "bird" observation (4).
        var result = _svc.FilterByTaxon(_heron, Sample());

        Assert.Equal(new[] { 1, 5 }, Ids(result));
    }

    // Test that filtering by a taxon does not include observations of its siblings.
    [Fact]
    public void FilterByTaxon_SiblingBranch_ReturnsNothing()
    {
        
        var result = _svc.FilterByTaxon(_mammal, new List<Observation> { Obs(1, _heron), Obs(2, _plant) }); // mammal is a sibling of bird, so it should not match heron or plant

        // The result should be empty because neither observation is a mammal or a descendant of mammal.
        Assert.Empty(result);
    }

    // Test that filtering by a taxon returns an empty list when the input list is empty.
    [Fact]
    public void FilterByTaxon_NullRoot_ReturnsAllObservations()
    {
        
        var result = _svc.FilterByTaxon(null, Sample()); // null root means "no filtering"

        // All observations should be returned when no taxon is specified.
        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, Ids(result));
    }

    // Test that filtering by a taxon returns an empty list when the input list is empty.
    [Fact]
    public void FilterByTaxon_EmptyInput_ReturnsEmpty()
    {
        // Filtering by any taxon on an empty list of observations should return an empty list.
        Assert.Empty(_svc.FilterByTaxon(_bird, new List<Observation>()));
    }

    // Test that filtering by a taxon does not throw an exception when an observation has no taxon assigned.
    [Fact]
    public void FilterByTaxon_ObservationWithoutTaxon_IsSkippedWithoutThrowing()
    {
        // Create an observation with no taxon assigned (Taxon is null).
        var noTaxon = new Observation { PostId = 99, Taxon = null! };
        // Create an observation with a taxon assigned (heron).
        var input = new List<Observation> { noTaxon, Obs(1, _heron) };

        // Filter by bird, which should include heron observations (1) and skip the observation with no taxon.
        var result = _svc.FilterByTaxon(_bird, input);

        // The result should only include the heron observation (1) and not throw an exception for the observation with no taxon.
        Assert.Equal(new[] { 1 }, Ids(result));
    }

    // Test that filtering by a taxon preserves the order of the input list.
    [Fact]
    public void FilterByTaxon_PreservesInputOrder()
    {
        var input = new List<Observation> { Obs(30, _heron), Obs(10, _bird), Obs(20, _heron) };

        // Filter by bird, which should include heron observations (30, 20) and bird itself (10)
        var result = _svc.FilterByTaxon(_bird, input);

        // The order of the returned observations should match the order in the input list, not sorted by PostId or any other criteria.
        Assert.Equal(new[] { 30, 10, 20 }, Ids(result));
    }

    // Test that filtering by a taxon returns the same observation instances as in the input list, not new instances.
    [Fact]
    public void FilterByTaxon_ReturnsSameObservationInstances()
    {
        
        var input = Sample();

        // Filter by bird, which should include heron observations (1, 5) and bird itself (4)
        var result = _svc.FilterByTaxon(_bird, input);

        // Check that the returned observations are the same instances as in the input list
        Assert.All(result, o => Assert.Contains(o, input));
    }
}
