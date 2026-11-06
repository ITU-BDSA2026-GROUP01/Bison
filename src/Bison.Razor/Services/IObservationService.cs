using Bison.Razor.Models;

namespace Bison.Razor.Services;

public interface IObservationService
{
    List<ObservationDTO> GetObservations(int page = 1);
    List<ObservationDTO> GetObservationsFromAuthor(string author, int page = 1);
    List<Observation> GetByTaxon(Taxon root);
}
