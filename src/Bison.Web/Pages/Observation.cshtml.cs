using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Bison.Core;
using Bison.Services;
using System.Security.Claims;

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
    public List<Taxon> Taxa { get; set; } = new();

    [BindProperty] public string? CommentText { get; set; }
    [BindProperty] public int? ProposalTaxonId { get; set; }

    public IActionResult OnGet(int? id, [FromQuery] int page = 1)
    {
        if (id.HasValue)
        {
            if (!LoadObservation (id.Value)) return NotFound();
            return Page();
        }


        Observations = _service.GetObservations(page);
        return Page();
    }


    public IActionResult OnPostComment(int id)
    {
        if(User?.Identity?.IsAuthenticated != true)
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(CommentText))
            ModelState.AddModelError(nameof(CommentText), "DUMB DUMB. Write a comment first");



        if (!ModelState.IsValid)
        {   
            if (!LoadObservation(id))  return NotFound();
            return Page();
        }


        var (name, email) = GetAuthor();
        _service.CreateComment(name, email, CommentText!, id);
        return RedirectToPage(new { id });

    }


    public IActionResult OnPostProposal (int id)
    {
        if (User?.Identity?. IsAuthenticated != true)
        return Unauthorized();


        if (!ProposalTaxonId.HasValue)
            ModelState.AddModelError(nameof(ProposalTaxonId), "CHOOSE YOUR POKEMON (animal)");


        if (!ModelState.IsValid)
        {
            if(!LoadObservation(id)) return NotFound();
            return Page();
        }

        var (name, email) = GetAuthor();
        _service.CreateProposal(name, email, id, ProposalTaxonId!.Value);
        return RedirectToPage(new { id });
    }

    private bool LoadObservation (int id)
    {
        Observation = _service.GetObservation(id);
        if (Observation == null) return false;


        Comments = _service.GetComments(id);
        Proposals = _service.GetProposals(id);
        Taxa = _service.GetAllTaxa()
            .Where(t => !string.IsNullOrEmpty (t.VernacularName))
            .OrderBy( t => t.VernacularName)
            .ToList();
        return true;

    }

    private (string Name, string Email) GetAuthor()
    {
        var userName = User.Identity!.Name!;
        var email = User.FindFirst(ClaimTypes.Email)?.Value ?? userName;
        var name = User.FindFirst("FullName")?.Value ?? userName;
        return (name, email);
    }

}