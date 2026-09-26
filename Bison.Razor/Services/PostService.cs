using Bison.Razor.Models;

namespace Bison.Razor.Services;

public class PostService : IPostService
{
    private readonly DBFacade _db;

    public PostService(DBFacade db)
    {
        _db = db;
    }

    public List<ObservationViewModel> GetObservations()
    {
        return _db.GetAllObservations();
    }
}
