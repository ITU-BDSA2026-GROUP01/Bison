using Bison.Razor.Models;

namespace Bison.Razor.Services;

public interface IPostRepository
{
    List<ObservationDTO> GetAllObservations(int page = 1);
    List<ObservationDTO> GetObservationsByAuthor(string author, int page = 1);
}