using Bison.Razor.Models;

namespace Bison.Razor.Services;

public interface IObservationService
{
    List<ObservationViewModel> GetObservations(int page = 1);
    List<ObservationViewModel> GetObservationsFromAuthor(string author, int page = 1);
}
