using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Bison.Razor.Models;
using Bison.Razor.Services;
using System.Data.Common;

namespace Bison.Razor.Pages;

public class ObservationModel : PageModel
{
    private readonly DBFacade _db;

    public ObservationModel(DBFacade db)
    {
        _db = db;
    }

    public ObservationViewModel? Observation { get; set;}
    public List<ObservationViewModel> Observations { get; set;} = new();
    public List<CommentViewModel> Comments {get; set; } = new();
    public List<ProposalViewModel> Proposals {get; set;} = new();

    public IActionResult OnGet(int? id)
    {
        if (id.HasValue)
        {
            Observation = _db.GetObservationsById(id.Value);

            if (Observation == null)
            {
                return NotFound();
            }
            Comments = _db.GetCommentsForObservation(id.Value);
            Proposals = _db.GetProposalsForObservation(id.Value);

            return Page();
        }
        Observations = _db.GetAllObservations();
        return Page();
    }
}