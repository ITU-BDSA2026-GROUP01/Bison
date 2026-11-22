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
                Timestamp = o.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
                Author = o.Author.Name,
                PicturePath = o.PicturePath,
                PictureLicense = o.PictureLicense
            })
            .ToList();
    }

    public List<ObservationDTO> GetObservationsByAuthor(string author, int page = 1)
    {
        return _context.Observations
            .Include(o => o.Author)
            .Where(o => o.Author.Name == author || o.Author.Email == author)
            .OrderByDescending(o => o.TimeStamp)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .Select(o => new ObservationDTO
            {
                Id = o.PostId,
                Message = o.Text,
                Timestamp = o.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
                Author = o.Author.Name,
                PicturePath = o.PicturePath,
                PictureLicense = o.PictureLicense
            })
            .ToList();
    }


    public ObservationDTO CreateObservation (string authorName, string authorEmail, string text, int taxonId, string? picturePath = null, string? pictureLicense = null)
    {
        if (!_context.Taxons.Any(t=> t.TaxonId == taxonId))
            throw new ArgumentException ("Unknown taxon.", nameof(taxonId));

            var author = _context.Authors.FirstOrDefault(a => a.Email == authorEmail)
                    ??_context.Authors.FirstOrDefault(a => a.Name == authorName);
            if (author == null)
            {
                author = new Author { Name = authorName, Email = authorEmail};
                _context.Authors.Add(author);
            }

            var observation = new Observation
            {
                Text = text,
                TimeStamp = DateTime.UtcNow,
                Author = author,
                TaxonId = taxonId,
                PicturePath = picturePath,
                PictureLicense = pictureLicense
            };
            _context.Observations.Add(observation);
            _context.SaveChanges();

            return new ObservationDTO
            {
                Id = observation.PostId,
                Message = observation.Text,
                Timestamp = observation.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
                Author = author.Name,
                PicturePath = observation.PicturePath,
                PictureLicense = observation.PictureLicense
            };
    }



    private Author GetCreateOrAuthor(string authorName, string authorEmail)
    {
        var author= _context.Authors.FirstOrDefault (a => a.Email == authorEmail)
            ?? _context.Authors.FirstOrDefault (a => a.Name == authorName);

        if (author == null)
        {
            author = new Author {Name = authorName, Email = authorEmail};
            _context.Authors.Add(author);
        } return author;
    }

    public CommentDTO CreateComment (string authorName, string authorEmail, string text, int observationId)
    {
        if (!_context.Observations.Any(o => o.PostId == observationId))
            throw new ArgumentException("hmmm i don't know this observation", nameof(observationId));


        var author = GetCreateOrAuthor (authorName, authorEmail); 
        var comment = new Comment
            {
                Text = text,
                TimeStamp = DateTime.UtcNow,
                Author = author,
                ObservationId = observationId
            };
            _context.Comments.Add(comment);
            _context.SaveChanges();

            return new CommentDTO
            {
                Id = comment.PostId,
                ObservationId = observationId,
                Author = author.Name,
                Message = comment.Text,
                Timestamp = comment.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture)
            };

    }

public ProposalDTO CreateProposal(string authorName, string authorEmail, int observationId, int taxonId)
{
    if (!_context.Observations.Any(o => o.PostId == observationId))
        throw new ArgumentException("hmmm i don't know this observation", nameof(observationId));
    var taxon = _context.Taxons.FirstOrDefault (t => t.TaxonId == taxonId)
        ?? throw new ArgumentException("hmmm i don't know this taxon", nameof(taxonId));
    
    var author = GetCreateOrAuthor(authorName, authorEmail);
    var proposal = new Proposal
    {

        Text = "",
        TimeStamp = DateTime.UtcNow,
        Author = author,
        ObservationId = observationId,
        TaxonId = taxonId
    };
    _context.Proposals.Add(proposal);
    _context.SaveChanges();

    return new ProposalDTO
    {
        Id = proposal.PostId,
        ObservationId = observationId,
        Author = author.Name,
        TaxonId = taxon.dwc_TaxonID,
        TaxonName = taxon.VernacularName ?? taxon.dwc_TaxonID,
        Timestamp = proposal.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture)

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
                Timestamp = c.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture)
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
                TaxonName = p.Taxon?.VernacularName ?? p.Taxon?.dwc_TaxonID ?? "",
                Timestamp = p.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture)
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


public UserPostsDTO GetPostsByAuthorEmail(string email) 
{
    const string format = "yyyy-MM-dd HH:mm:ss";
    var culture = System.Globalization.CultureInfo.InvariantCulture;

    var observations = _context.Observations
        .Include(o => o.Author)
        .Where(o => o.Author.Email == email)
        .OrderByDescending(o => o.TimeStamp)
        .ToList()
        .Select(o => new ObservationDTO
        {
            Id = o.PostId,
            Message = o.Text,
            Timestamp = o.TimeStamp.ToString(format, culture),
            Author = o.Author.Name,
            PicturePath = o.PicturePath,
            PictureLicense = o.PictureLicense
        })
        .ToList();

    var comments = _context.Comments
        .Include(c => c.Author)
        .Where(c => c.Author.Email == email)
        .OrderByDescending(c => c.TimeStamp)
        .ToList()
        .Select(c => new CommentDTO
        {
            Id = c.PostId,
            ObservationId = c.ObservationId,
            Author = c.Author.Name,
            Message = c.Text,
            Timestamp = c.TimeStamp.ToString(format, culture)
        })
        .ToList();

    var proposals = _context.Proposals
        .Include(p => p.Author)
        .Include(p => p.Taxon)
        .Where(p => p.Author.Email == email)
        .OrderByDescending(p => p.TimeStamp)
        .ToList()
        .Select(p => new ProposalDTO
        {
            Id = p.PostId,
            ObservationId = p.ObservationId,
            Author = p.Author.Name,
            TaxonId = p.Taxon?.dwc_TaxonID ?? "",
            TaxonName = p.Taxon?.VernacularName ?? p.Taxon?.dwc_TaxonID ?? "",
            Timestamp = p.TimeStamp.ToString(format, culture)
        })
        .ToList();

    return new UserPostsDTO
    {
        Observations = observations,
        Comments = comments,
        Proposals = proposals
    };
}

}
