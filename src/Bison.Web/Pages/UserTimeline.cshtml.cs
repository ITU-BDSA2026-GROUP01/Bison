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

    [BindProperty] public string? Text {get; set; }


    [BindProperty] public int? NewTaxonId { get; set; }

    [BindProperty] public IFormFile? Picture {get; set; }
    
    
    public UserTimelineModel(IObservationService service)
    {
        _service = service;
    }

    public ActionResult OnGet (string author, [FromQuery] int page = 1)
    {
        LoadPage(author, page);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string author, [FromServices] IPictureStorage pictures)
    {
        if (User?.Identity?.IsAuthenticated != true)
        return Unauthorized();
        if (!IsOwn(author))
        return StatusCode(403);

         // empty text or no taxon is rejected
        if (string.IsNullOrWhiteSpace(Text))
            ModelState.AddModelError(nameof(Text), "Write something first!!!!!!!!!!!!!");
        if (!NewTaxonId.HasValue)
            ModelState.AddModelError(nameof(NewTaxonId), "Pls pls pls choose a taxon");

        if (!ModelState.IsValid)
        {
            LoadPage(author, 1);
            return Page();

        
        }


        string? picturePath = null;
        if ( Picture is {Length: > 0}) // only if it has chosen one file
        {

            // Checks the filetype and the size and saves in wwwroot/uploads.
            // gives a path or fucks it 
            var (path, error) = await pictures.SaveAsync(Picture);
            if (error !=null)
            {
                // show the side again with the error message
                ModelState.AddModelError(nameof(Picture), error);
                LoadPage(author, 1);
                return Page();
            }
            picturePath = path;

        }


        var userName = User.Identity.Name!;
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? userName;
        var name = User.FindFirst("FullName")?.Value ?? userName;


        // save via server so like service -> repo -> database
        _service.CreateObservation(name, email, Text!, NewTaxonId!.Value, picturePath);

        // Refresh dont resubmit the form. 
        return RedirectToPage(new {author });

    }

    private void LoadPage(string author, int page)
    {
        IsOwnTimeline = IsOwn(author);
        Cheeps = _service.GetObservationsFromAuthor(author, page);
        Taxa = _service.GetAllTaxa()
        .Where(t => !string.IsNullOrEmpty(t.VernacularName))
        .OrderBy(t => t.VernacularName)
        .ToList();
    }

        private bool IsOwn(string author) => User?.Identity?.IsAuthenticated == true &&
        (string.Equals(author, User.Identity.Name, StringComparison.OrdinalIgnoreCase) || 
        string.Equals(author, User.FindFirst("FullName")?.Value, StringComparison.OrdinalIgnoreCase));
}

