using Bison.Razor.Models;

namespace Bison.Razor.Services;

public interface IPostRepository
{
    List<ObservationViewModel> GetAllObservations(int page = 1);
    List<ObservationViewModel> GetObservationsByAuthor(string author, int page = 1);
}