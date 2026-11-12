using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Bison.Core;
using Bison.Services;

namespace Bison.Web.Pages;

public class UserTimelineModel : PageModel
{
    private readonly IObservationService _service;


    public List<ObservationDTO> Cheeps { get; set; } = new();
    public List <Taxon> Taxa { get ; set; } = new();
    public bool IsOwnTimeline { get; private set; }

    [BindPropetry] public string? Text {get; set; }


    [BindPropetry] public int? NewTaxonId { get; set; }
    
    
    public UserTimelineModel(IObservationService service)
    {
        _service = service;
    }

    public ActionResult OnGet (string author, [FromQuery] int page = 1)
    {
        LoadPage(author, page);
        return Page();
    }

    public IActionResult OnPost(string author)
    {
        if (User.Identity?.IsAuthenticated != true)
        return Unauthorized();
        if (!IsOwn(author))
        return StatusCode(403);

         // empty text or no taxon is rejected
        if (string.IsNullorWhiteSpace(Text))
            ModelState.AddModelEroor(nameof(Text), "Write something first!!!!!!!!!!!!!")
        if (!NewTaxonId.HasValue)
            ModelState.AddModelEroor(nameof(NewTaxonId), "Pls pls pls choose a taxon");

        if (!ModelState.IsValid)
        {
            LoadPage(author, 1);
            return Page();

        }


        var userName = User.Identity.Name!;
        var email = User.FindFirst(System.Security.ClaimTypes.Email)?.Value ?? userName;
        var name = User.FindFirst("FullName")?.Value ?? userName;


        // save via server so like service -> repo -> database
        _service.CreateObervation(name, emial, Text!, NewTaxonId!.Value);

        // Refresh dont resubmit the form. 
        return RedirectToPage(new {author });

        private void LoadPage(string author, int page)
        {
            IsOwnTimeline = IsOwn(author);
            Cheeps = _service.GetObservationsFromAuthor(author, page);
            Taxa = _service.GetAllTaxa()
            .Where(t => !string.IsNullOrEmpty(t.VernacularName))
            .OrderBy(t => t.VernacularName)
            .ToList();
        }

        private bool IsOwn(string author) => User.Identity?.IsAuthenticated == True &&
        (string.Equals(author, User.Identity.Name, StringComparision.OrdinalIngoreCase) || 
        string.Equals(author, User.FindFirst("FullName")?.Value, StringComparision.OrdinalIngoreCase));
    }

}