using Bison.Razor.Models;

namespace Bison.Razor.Services;

public interface IPostService
{
    List<ObservationViewModel> GetObservations();
}
