using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Bison.Razor.Models;
using Bison.Razor.Services;
using Microsoft.EntityFrameworkCore;

namespace Bison.Razor.Pages;

public class PublicModel : PageModel
{
    private readonly IPostService _service;
    private readonly BisonDbContext _db;
    public List<ObservationDTO> Cheeps { get; set; } = new();

    public PublicModel(IPostService service, BisonDbContext db)
    {
        _service = service;
        _db = db;
    }

    public ActionResult OnGet([FromQuery] int page = 1)
    {
        if (taxonId.HasValue)
        Cheeps = _service.GetObservations(page);
        return Page();
    }
}