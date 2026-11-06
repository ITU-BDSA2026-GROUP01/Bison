using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Bison.Core;
using Bison.Services;

namespace Bison.Web.Pages;

public class ObservationModel : PageModel
{
    private readonly IPostService _service;

    public ObservationModel(IPostService service)
    {
        _service = service;
    }

    public ObservationDTO? Observation { get; set; }
    public List<ObservationDTO> Observations { get; set; } = new();
    public List<CommentDTO> Comments { get; set; } = new();
    public List<ProposalDTO> Proposals { get; set; } = new();

    public IActionResult OnGet(int? id, [FromQuery] int page = 1)
    {
        if (id.HasValue)
        {
            Observation = _service.GetObservation(id.Value);
            if (Observation == null)
            {
                return NotFound();
            }

            Comments = _service.GetComments(id.Value);
            Proposals = _service.GetProposals(id.Value);
            return Page();
        }

        Observations = _service.GetObservations(page);
        return Page();
    }
}