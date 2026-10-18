using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Bison.Razor.Data;
using Bison.Razor.Models;
using Bison.Razor.Services;
using Microsoft.EntityFrameworkCore;

namespace Bison.Razor.Pages;

public class PublicModel : PageModel
{
    private const int PageSize = 32;

    private readonly IPostService _service;
    private readonly BisonDBContext _db;
    private readonly IObservationTaxonFilterService _filter;

    public List<ObservationDTO> Cheeps { get; set; } = new();

    public PublicModel(IPostService service, BisonDBContext db, IObservationTaxonFilterService? filter = null)
    {
        _service = service;
        _db = db;
        _filter = filter ?? new ObservationTaxonFilterService();
    }

    public ActionResult OnGet([FromQuery] int page = 1, [FromQuery] int? taxonId = null)
    {
        if (!taxonId.HasValue)
        {
            Cheeps = _service.GetObservations(page);
            return Page();
        }

        // Load the whole taxonomy so every Parent chain is linked (IsSubTaxon walks up the tree).
        _db.Taxons.Load();
        var root = _db.Taxons.Find(taxonId.Value);
        if (root is null)
        {
            return NotFound();
        }

        var newestFirst = _db.Observations
            .Include(o => o.Author)
            .Include(o => o.Taxon)
            .OrderByDescending(o => o.TimeStamp)
            .ToList();

        // Filter first, paginate afterwards, otherwise a page can come back
        // short (or empty) just because the matches live on a later page.
        Cheeps = _filter.FilterByTaxon(root, newestFirst)
            .Skip((Math.Max(page, 1) - 1) * PageSize)
            .Take(PageSize)
            .Select(o => new ObservationDTO
            {
                Id = o.PostId,
                Message = o.Text,
                Timestamp = o.TimeStamp.ToString("u"),
                Author = o.Author?.Name ?? ""
            })
            .ToList();

        return Page();
    }
}