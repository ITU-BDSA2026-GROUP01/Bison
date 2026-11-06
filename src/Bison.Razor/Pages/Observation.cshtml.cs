using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Bison.Razor.Models;
using Bison.Razor.Services;

namespace Bison.Razor.Pages;

public class ObservationModel : PageModel
{
    private readonly IPostRepository _repo;

    public ObservationModel(IPostRepository repo)
    {
        _repo = repo;
    }

    public ObservationDTO? Observation { get; set; }
    public List<ObservationDTO> Observations { get; set; } = new();
    public List<CommentDTO> Comments { get; set; } = new();
    public List<ProposalDTO> Proposals { get; set; } = new();

    public IActionResult OnGet(int? id, [FromQuery] int page = 1)
    {
        if (id.HasValue)
        {
            var entity = _repo.GetObservationWithAttachments(id.Value);
            if (entity == null)
            {
                return NotFound();
            }

            Observation = new ObservationDTO
            {
                Id = entity.PostId,
                Author = entity.Author.Name,
                Message = entity.Text,
                Timestamp = entity.TimeStamp.ToString("u")
            };
            Comments = _repo.GetCommentsForObservation(id.Value);
            Proposals = _repo.GetProposalsForObservation(id.Value);
            return Page();
        }

        Observations = _repo.GetAllObservations(page);
        return Page();
    }
}
