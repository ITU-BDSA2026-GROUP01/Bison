using Bison.Razor.Models;

namespace Bison.Razor.Services;

public interface IObservationService
{
    List<ObservationViewModel> GetObservations();
    List<ObservationViewModel> GetObservationsFromAuthor(string author);
}
