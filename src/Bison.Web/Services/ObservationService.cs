using Bison.Razor.Models;

namespace Bison.Razor.Services;

public class ObservationService : IObservationService
{
    private readonly IPostRepository _repository;
    private readonly IObservationTaxonFilterService _filter;

    public ObservationService(IPostRepository repository, IObservationTaxonFilterService? filter = null)
    {
        _repository = repository;
        _filter = filter ?? new ObservationTaxonFilterService();
    }

    public List<ObservationDTO> GetObservations(int page = 1)
    {
        return _repository.GetAllObservations(page);
    }

    public List<ObservationDTO> GetObservationsFromAuthor(string author, int page = 1)
    {
        return _repository.GetObservationsByAuthor(author, page);
    }

    public List<Observation> GetByTaxon(Taxon root)
    {
        var allObservations = _repository.GetAllObservationsWithTaxons();
        return _filter.FilterByTaxon(root, allObservations);
    }
}
