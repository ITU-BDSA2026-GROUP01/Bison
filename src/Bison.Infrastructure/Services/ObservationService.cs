using Bison.Core;
using Bison.Repositories;
namespace Bison.Services;

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


     public ObservationDTO CreateObservation(string authorName, string authorEmail, string text, int taxonId)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Observation text cannot be empty", nameof(text));
        return _repository.CreateObservation(authorName, authorEmail, text.Trim(), taxonId);
    }


    public List<Taxon> GetAllTaxa()
    {
        return _repository.GetAllTaxa();
    }

    public List<Observation> GetByTaxon(Taxon root)
    {
        var allObservations = _repository.GetAllObservationsWithTaxons();
        return _filter.FilterByTaxon(root, allObservations);
    }
}
