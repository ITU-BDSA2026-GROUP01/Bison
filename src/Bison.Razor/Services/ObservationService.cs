using Bison.Razor.Models;

namespace Bison.Razor.Services;

public class ObservationService : IObservationService
{
    private readonly DBFacade _db;

    public ObservationService(DBFacade db)
    {
        _db = db;
    }

    public List<ObservationDTO> GetObservations(int page = 1)
    {
        return _db.GetAllObservations(page);
    }

    public List<ObservationDTO> GetObservationsFromAuthor(string author, int page = 1)
    {
        return _db.GetObservationsByAuthor(author, page);
    private readonly IPostRepository _repository;

    public ObservationService(IPostRepository repository)
    {
        _repository = repository;
    }

    public List<ObservationViewModel> GetObservations(int page = 1)
    {
        return _repository.GetAllObservations(page);
    }

    public List<ObservationViewModel> GetObservationsFromAuthor(string author, int page = 1)
    {
        return _repository.GetObservationsByAuthor(author, page);
    }
}
