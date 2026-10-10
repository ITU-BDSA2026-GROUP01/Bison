using Bison.Razor.Models;

namespace Bison.Razor.Services;

public class ObservationService : IObservationService
{
    private readonly IPostRepository _repository;

    public ObservationService(IPostRepository repository)
    {
        _repository = repository;
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
        return ObservationFilterHelper.FilterByTaxon(root, allObservations);
    }
}
