using Bison.Razor.Models;

namespace Bison.Razor.Services;

public class PostService : IPostService
{
    private readonly IPostRepository _repository;

    public PostService(IPostRepository repository)
    {
        _repository = repository;
    }

    public List<ObservationViewModel> GetObservations(int page = 1)
    {
        return _repository.GetAllObservations(page);
    }
}
