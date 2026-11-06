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

    //list of all taxa for the filter dropdown
    public List<Taxon> Taxa { get; set; } = new();

    public int? SelectedTaxonId { get; set; }

    // Whether the current page is filtered by a taxon. This is used to
    // conditionally show the "clear filter" link in the UI.
    public bool IsFiltered => SelectedTaxonId.HasValue;

    public PublicModel(IPostService service, BisonDBContext db, IObservationTaxonFilterService filter)
    {
        _service = service;
        _db = db;
        _filter = filter;
    }

    public ActionResult OnGet([FromQuery] int page = 1, [FromQuery] int? taxonId = null)
    {
        // Load all taxa for the filter dropdown.
        var allTaxa = _db.Taxons.ToList();

        // Only show taxa with a vernacular name, and sort by that name.
        Taxa = allTaxa
            .Where(t=> !String.IsNullOrEmpty(t.VernacularName))
            .OrderBy(t => t.VernacularName)
            .ToList();
            SelectedTaxonId = taxonId;

        if (!taxonId.HasValue)
        {
            Cheeps = _service.GetObservations(page);
            return Page();
        }

       var root = allTaxa.FirstOrDefault(t => t.TaxonId == taxonId.Value);
        if (root is null)
        {
            // Unknown taxon: nothing can match, so the page stays blank.
            return Page();
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