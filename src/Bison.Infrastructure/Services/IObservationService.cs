using Bison.Core;

namespace Bison.Services;

public interface IObservationService
{
    List<ObservationDTO> GetObservations(int page = 1);
    List<ObservationDTO> GetObservationsFromAuthor(string author, int page = 1);
    List<Observation> GetByTaxon(Taxon root);
    ObservationDTO CreateObservation (string authorName, string authorEmail, string text, int taxonId);
    List<Taxon> GetAllTaxa();
}

