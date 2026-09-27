using Bison.Razor.Models;

namespace Bison.Razor.Services;

public class ObservationService : IObservationService
{
    private readonly DBFacade _db;

    public ObservationService(DBFacade db)
    {
        _db = db;
    }

    public List<ObservationViewModel> GetObservations(int page = 1)
    {
        return _db.GetAllObservations();
    }

    public List<ObservationViewModel> GetObservationsFromAuthor(string author, int page = 1)
    {
        return _db.GetObservationsByAuthor(author);
    }
}
