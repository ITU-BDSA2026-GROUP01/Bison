using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Bison.Razor.Models;
using Bison.Razor.Services;

namespace Bison.Razor.Pages;

public class ObservationModel : PageModel
{
    private readonly DBFacade _db;
    private readonly IPostRepository _postRepository;

    public ObservationModel(DBFacade db, IPostRepository postRepository)
    {
        _db = db;
        _postRepository = postRepository;
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
        Observations = _postRepository.GetAllObservations(page);
        return Page();
    }
}
