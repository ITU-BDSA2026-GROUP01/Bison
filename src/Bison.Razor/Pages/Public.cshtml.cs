using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Bison.Razor.Models;
using Bison.Razor.Services;

namespace Bison.Razor.Pages;

public class PublicModel : PageModel
{
    private readonly IPostService _service;
    public List<ObservationDTO> Cheeps { get; set; } = new();

    public PublicModel(IPostService service)
    {
        _service = service;
    }

    public ActionResult OnGet([FromQuery] int page = 1)
    {
        Cheeps = _service.GetObservations(page);
        return Page();
    }
}