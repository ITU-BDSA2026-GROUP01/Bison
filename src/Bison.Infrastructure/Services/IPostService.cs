using Bison.Core;

namespace Bison.Services;

public interface IPostService
{
    List<ObservationDTO> GetObservations(int page = 1);
}
