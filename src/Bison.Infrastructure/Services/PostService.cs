using Bison.Core;
using Bison.Repositories;
namespace Bison.Services;

public class PostService : IPostService
{
    private const int PageSize = 32;

    private readonly IPostRepository _repository;
    private readonly IObservationTaxonFilterService _filter;

    public PostService(
        IPostRepository repository,
        IObservationTaxonFilterService? filter = null)
    {
        _repository = repository;
        _filter = filter ?? new ObservationTaxonFilterService();
    }

    public List<ObservationDTO> GetObservations(int page = 1)
    {
        return _repository.GetAllObservations(page);
    }

    public List<ObservationDTO> GetObservationsByTaxon(int taxonId, int page = 1)
    {
        var root = _repository.GetAllTaxa()
            .FirstOrDefault(taxon => taxon.TaxonId == taxonId);
        if (root is null)
            return new List<ObservationDTO>();

        var observations = _repository.GetAllObservationsWithTaxons();
        return _filter.FilterByTaxon(root, observations)
            .Skip((Math.Max(page, 1) - 1) * PageSize)
            .Take(PageSize)
            .Select(observation => new ObservationDTO
            {
                Id = observation.PostId,
                Message = observation.Text,
                Timestamp = observation.TimeStamp.ToString("u"),
                Author = observation.Author?.Name ?? ""
            })
            .ToList();
    }

    public List<Taxon> GetAllTaxa()
    {
        return _repository.GetAllTaxa();
    }

    public ObservationDTO? GetObservation(int id)
{
    var entity = _repository.GetObservationWithAttachments(id);
    if (entity == null) return null;

    return new ObservationDTO
    {
        Id = entity.PostId,
        Author = entity.Author.Name,
        Message = entity.Text,
        Timestamp = entity.TimeStamp.ToString("u")
    };
}

public List<CommentDTO> GetComments(int observationId)
    => _repository.GetCommentsForObservation(observationId);

public List<ProposalDTO> GetProposals(int observationId)
    => _repository.GetProposalsForObservation(observationId);
}


