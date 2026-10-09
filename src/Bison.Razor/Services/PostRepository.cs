using Bison.Razor.Models;
using Bison.Razor.Data;
using Microsoft.EntityFrameworkCore;

namespace Bison.Razor.Services;

public class PostRepository : IPostRepository
{
    private const int PageSize = 32;

    private readonly BisonDBContext _context;

    public PostRepository(BisonDBContext context)
    {
        _context = context;
    }

    public List<ObservationDTO> GetAllObservations(int page = 1)
    {
        return _context.Observations
            .Include(o => o.Author)
            .OrderByDescending(o => o.TimeStamp)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .Select(o => new ObservationDTO
            {
                Id = o.Id,
                Message = o.Text,
                Timestamp = o.TimeStamp.ToString("u"),
                Author = o.Author.Name
            })
            .ToList();
    }

    public List<ObservationDTO> GetObservationsByAuthor(string author, int page = 1)
    {
        return _context.Observations
            .Include(o => o.Author)
            .Where(o => o.Author.Name == author)
            .OrderByDescending(o => o.TimeStamp)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .Select(o => new ObservationDTO
            {
                Id = o.Id,
                Message = o.Text,
                Timestamp = o.TimeStamp.ToString("u"),
                Author = o.Author.Name
            })
            .ToList();
    }
}   