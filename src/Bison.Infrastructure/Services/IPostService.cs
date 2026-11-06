using Bison.Core;

namespace Bison.Services;

public interface IPostService
{
    List<ObservationDTO> GetObservations(int page = 1);
    List<ObservationDTO> GetObservationsByTaxon(int taxonId, int page = 1);
    List<Taxon> GetAllTaxa();
}
