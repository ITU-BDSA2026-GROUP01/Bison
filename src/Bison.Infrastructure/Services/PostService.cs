using Bison.Core;

namespace Bison.Services;

public class PostService : IPostService
{
    private readonly IPostRepository _repository;

    public PostService(IPostRepository repository)
    {
        _repository = repository;
    }

    public List<ObservationDTO> GetObservations(int page = 1)
    {
        return _repository.GetAllObservations(page);
    }
    
}
