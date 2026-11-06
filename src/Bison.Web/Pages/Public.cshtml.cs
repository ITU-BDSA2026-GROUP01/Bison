using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Bison.Core;
using Bison.Services;

namespace Bison.Web.Pages;

public class PublicModel : PageModel
{
    private readonly IPostService _service;

    public List<ObservationDTO> Cheeps { get; set; } = new();

    //list of all taxa for the filter dropdown
    public List<Taxon> Taxa { get; set; } = new();

    public int? SelectedTaxonId { get; set; }

    // Whether the current page is filtered by a taxon. This is used to
    // conditionally show the "clear filter" link in the UI.
    public bool IsFiltered => SelectedTaxonId.HasValue;

    public PublicModel(IPostService service)
    {
        _service = service;
    }

    public ActionResult OnGet([FromQuery] int page = 1, [FromQuery] int? taxonId = null)
    {
        // Only show taxa with a vernacular name, and sort by that name.
        Taxa = _service.GetAllTaxa()
            .Where(t => !string.IsNullOrEmpty(t.VernacularName))
            .OrderBy(t => t.VernacularName)
            .ToList();
        SelectedTaxonId = taxonId;

        if (!taxonId.HasValue)
        {
            Cheeps = _service.GetObservations(page);
            return Page();
        }

        // Filtering happens before pagination in the service so later matches
        // are not skipped.
        Cheeps = _service.GetObservationsByTaxon(taxonId.Value, page);
        return Page();
    }
}