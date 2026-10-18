using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Bison.Razor.Data;
using Bison.Razor.Models;
using Bison.Razor.Services;
using Microsoft.EntityFrameworkCore;

namespace Bison.Razor.Pages;

public class PublicModel : PageModel
{
    private readonly IPostService _service;
    private readonly BisonDBContext _db;

    public List<ObservationDTO> Cheeps { get; set; } = new();

    public PublicModel(IPostService service, BisonDBContext db)
    {
        _service = service;
        _db = db;
    }

    public ActionResult OnGet([FromQuery] int page = 1, [FromQuery] int? taxonId = null)
    {
        if (taxonId.HasValue)
        {
            var root = _db.Taxons
                .Include(t => t.Parent)
                .SingleOrDefault(t => t.TaxonId == taxonId.Value);

            if (root is null)
            {
                return NotFound();
            }

            var allObservations = _db.Observations
                .Include(o => o.Taxon)
                .ThenInclude(t => t.Parent)
                .OrderByDescending(o => o.TimeStamp)
                .Skip((page - 1) * 32)
                .Take(32)
                .ToList();

            var filterService = new ObservationTaxonFilterService();
            var filtered = filterService.FilterByTaxon(root, allObservations);

            Cheeps = filtered.Select(o => new ObservationDTO
            {
                Id = o.PostId,
                Message = o.Text,
                Timestamp = o.TimeStamp.ToString("u"),
                Author = o.Author?.Name ?? ""
            }).ToList();

            return Page();
        }

        Cheeps = _service.GetObservations(page);
        return Page();
    }
}