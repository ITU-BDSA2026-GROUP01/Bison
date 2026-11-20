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
                Timestamp = observation.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss"),
                Author = observation.Author?.Name ?? "",
                PicturePath = observation.PicturePath,
                PictureLicense = observation.PictureLicense
            })
            .ToList();
    }


    public ObservationDTO CreateObservation(string authorName, string authorEmail, string text, int taxonId, string? picturePath = null, string? pictureLicense = null)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Observation text cannot be empty", nameof(text));
        return _repository.CreateObservation(authorName, authorEmail, text.Trim(), taxonId, picturePath, pictureLicense);
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
        Timestamp = entity.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss"),
        PicturePath = entity.PicturePath,
        PictureLicense = entity.PictureLicense

    };
}

    public List<CommentDTO> GetComments(int observationId)
        => _repository.GetCommentsForObservation(observationId);

    public List<ProposalDTO> GetProposals(int observationId)
        => _repository.GetProposalsForObservation(observationId);


    public CommentDTO CreateComment (string authorName, string authorEmail, string text, int observationId)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException ("Hey your comment be looking like the dragon warriors scroll (empty....)", nameof(text));
        return _repository.CreateComment(authorName, authorEmail, text.Trim(), observationId);
    }

    public ProposalDTO CreateProposal (string authorName, string authorEmail, int observationId, int taxonId)
        => _repository.CreateProposal(authorName, authorEmail, observationId, taxonId);

}


