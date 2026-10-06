using Bison.Razor.Models;

namespace Bison.Razor.Services;

public interface IPostService
{
    List<ObservationDTO> GetObservations(int page = 1);
}
