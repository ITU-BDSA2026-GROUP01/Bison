using Bison.Core;
using Bison.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Bison.Repositories;

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
                Id = o.PostId,
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
            .Where(o => o.Author.Name == author || o.Author.Email == auther)
            .OrderByDescending(o => o.TimeStamp)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .Select(o => new ObservationDTO
            {
                Id = o.PostId,
                Message = o.Text,
                Timestamp = o.TimeStamp.ToString("u"),
                Author = o.Author.Name
            })
            .ToList();
    }


    public ObservationDTO CreateObervation (string authorName, string autherEmail, string text, int taxonId)
    {
        if (!_contextTaxon.Any(t=> t.TaxonId == taxonid))
            throw new ArgumentException ("Unknow taxon.", nameof(taxonId));

            var auther = _context.Author.FirstOrDefault(a => a.Email == authorEmail)
                    ??_context..Author.FirstOrDefault(a => a.Email == authorEmail)
            if (author == null)
            {
                author = new Author { Name = authorName, Email = authorEmail};
                _context.Authers.Add(auther);
            }

            var observation = new Observation
            {
                Text = text,
                TimeStamp = DateTime.UtcNow,
                Author = author,
                TaxonId = taxonId
            };
            _context.Observations.Add(observation);
            _context.SaveChanges();

            return new ObservationDTO
            {
                Id = observation.PostId,
                Messages = observation.Text,
                TimeStamp = observation.TimeStamp.ToString("u"),
                Author = author.Name
            };
    }

    public Observation? GetObservationWithAttachments(int id)
    {
        return _context.Observations
            .Include(o => o.Author)
            .Include(o => o.Taxon)
            .Include(o => o.Comments)
                .ThenInclude(c => c.Author)
            .Include(o => o.Proposals)
                .ThenInclude(p => p.Author)
            .Include(o => o.Proposals)
                .ThenInclude(p => p.Taxon)
            .FirstOrDefault(o => o.PostId == id);
    }

    public List<CommentDTO> GetCommentsForObservation(int observationId)
    {
        return _context.Comments
            .Include(c => c.Author)
            .Where(c => c.ObservationId == observationId)
            .OrderByDescending(c => c.TimeStamp)
            .Select(c => new CommentDTO
            {
                Id = c.PostId,
                ObservationId = c.ObservationId,
                Author = c.Author.Name,
                Message = c.Text,
                Timestamp = c.TimeStamp.ToString("u")
            })
            .ToList();
    }

    public List<ProposalDTO> GetProposalsForObservation(int observationId)
    {
        return _context.Proposals
            .Include(p => p.Author)
            .Include(p => p.Taxon)
            .Where(p => p.ObservationId == observationId)
            .OrderByDescending(p => p.TimeStamp)
            .ToList()
            .Select(p => new ProposalDTO
            {
                Id = p.PostId,
                ObservationId = p.ObservationId,
                Author = p.Author.Name,
                TaxonId = p.Taxon?.dwc_TaxonID ?? "",
                Timestamp = p.TimeStamp.ToString("u")
            })
            .ToList();
    }

    public List<Observation> GetAllObservationsWithTaxons()
    {
        // Load the whole taxonomy first so EF links every Parent chain; IsSubTaxon
        // walks Parent all the way up, which a single Include/ThenInclude can't guarantee.
        _context.Taxons.Load();

        return _context.Observations
            .Include(o => o.Author)
            .Include(o => o.Taxon)
            .OrderByDescending(o => o.TimeStamp)
            .ToList();
    }
    public List<Taxon> GetAllTaxa()
{
    return _context.Taxons.ToList();
}
}
