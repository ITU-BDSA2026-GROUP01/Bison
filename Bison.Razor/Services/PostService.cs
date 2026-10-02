using Bison.Razor.Models;

namespace Bison.Razor.Services;

public class PostService : IPostService
{
    private readonly DBFacade _db;

    public PostService(DBFacade db)
    {
        _db = db;
    }

    public List<ObservationDTO> GetObservations(int page = 1)
    {
        return _db.GetAllObservations(page);
    }
}
