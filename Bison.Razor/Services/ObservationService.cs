using Bison.Razor.Models;

namespace Bison.Razor.Services;

public class ObservationService : IObservationService
{
    private readonly DBFacade _db;

    public ObservationService(DBFacade db)
    {
        _db = db;
    }

    public List<ObservationViewModel> GetObservations()
    {
        return _db.GetAllObservations();
    }

    public List<ObservationViewModel> GetObservationsFromAuthor(string author)
    {
        return _db.GetObservationsByAuthor(author);
    }
}
