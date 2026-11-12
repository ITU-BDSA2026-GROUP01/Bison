using Bison.Core;

namespace Bison.Services;

public interface IPostService
{
    List<ObservationDTO> GetObservations(int page = 1);
    List<ObservationDTO> GetObservationsByTaxon(int taxonId, int page = 1);
    List<Taxon> GetAllTaxa();
    ObservationDTO? GetObservation(int id);
    List<CommentDTO> GetComments(int observationId);
    List<ProposalDTO> GetProposals(int observationId);


    ObservationDTO CreateObervation (string authorName, string authorEmail, string text, int taxonId);
}
