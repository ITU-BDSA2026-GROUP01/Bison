using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Bison.Razor.Models;
using Bison.Razor.Services;

namespace Bison.Razor.Pages;

public class ObservationsModel : PageModel
{
    private readonly DBFacade _db;

    public ObservationsModel(DBFacade db)
    {
        _db = db;
    }

    public ObservationViewModel? Observation { get; set;}
    public List<ObservationViewModel> Observations { get; set;} = new();

    public IActionResult OnGet(int? id)
    {
        if (id.HasValue)
        {
            Observation = _db.GetObservationsById(id.Value);

            if (Observation == null)
            {
                return NotFound();
            }
            return Page();
        }
        Observations = _db.GetAllObservations();
        return Page();
    }
}