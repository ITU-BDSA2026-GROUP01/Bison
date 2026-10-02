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

    public ObservationDTO? Observation { get; set;}
    public List<ObservationDTO> Observations { get; set;} = new();
    public List<CommentDTO> Comments {get; set; } = new();
    public List<ProposalDTO> Proposals {get; set;} = new();

    public IActionResult OnGet(int? id, [FromQuery] int page = 1)
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
        Observations = _db.GetAllObservations(page);
        return Page();
    }
}