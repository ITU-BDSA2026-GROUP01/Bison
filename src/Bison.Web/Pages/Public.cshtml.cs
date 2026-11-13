using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Bison.Core;
using Bison.Services;
using System.Security.Claims;

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


    [BindProperty] public string? Text {get; set; }


    [BindProperty] public int? NewTaxonId { get; set; }

    public PublicModel(IPostService service)
    {
        _service = service;
    }

    public ActionResult OnGet([FromQuery] int page = 1, [FromQuery] int? taxonId = null)
    {
        LoadPage(page, taxonId);
        return Page();
    }


    public IActionResult OnPost()
    {
        // only real homies (those who are logged in) are allowed to create observations)
        if (User.Identity?.IsAuthenticated !=true)
            return Unauthorized();

        // empty text or no taxon is rejected
        if (string.IsNullOrWhiteSpace(Text))
            ModelState.AddModelError(nameof(Text), "Write something first!!!!!!!!!!!!!");
        if (!NewTaxonId.HasValue)
            ModelState.AddModelError(nameof(NewTaxonId), "Pls pls pls choose a taxon");
        if (!ModelState.IsValid)
        {
            // Show the page again with error messages. Bad bad.
            LoadPage(1, null);
            return Page();

        }
        // Get authors information from the logged user
        var userName = User.Identity.Name!;
        var email = User.FindFirst(ClaimTypes.Email)?.Value ?? userName;
        var name = User.FindFirst("FullName")?.Value ?? userName;


        // save via server so like service -> repo -> database
        _service.CreateObservation(name, email, Text!, NewTaxonId!.Value);

        // Refresh dont resubmit the form. 
        return RedirectToPage();
    }   

    private void LoadPage(int page, int? taxonId)
    {
        // Only show taxa with a vernacular name, and sort by that name.
        Taxa = _service.GetAllTaxa()
            .Where(t => !string.IsNullOrEmpty(t.VernacularName))
            .OrderBy(t => t.VernacularName)
            .ToList();
        SelectedTaxonId = taxonId;
        // Filtering happens before pagination in the service so matching rows
        // on later pages are not skipped.
        Cheeps = taxonId.HasValue
        ?_service.GetObservationsByTaxon(taxonId.Value, page)
        : _service.GetObservations(page);

    }


}