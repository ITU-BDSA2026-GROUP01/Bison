using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Bison.Razor.Models;
using Bison.Razor.Services;

namespace Bison.Razor.Pages;

public class UserTimelineModel : PageModel
{
    private readonly IObservationService _service;
    public List<ObservationViewModel> Cheeps { get; set; } = new();

    public UserTimelineModel(IObservationService service)
    {
        _service = service;
    }

    public ActionResult OnGet(string author)
    {
        Cheeps = _service.GetObservationsFromAuthor(author);
        return Page();
    }
}