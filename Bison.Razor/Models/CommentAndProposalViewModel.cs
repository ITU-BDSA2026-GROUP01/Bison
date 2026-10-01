
namespace Bison.Razor.Models;

public class CommentViewModel
{
    public int Id { get; set; }
    public int ObservationId { get; set; }
    public string Author { get; set; } = "";
    public string Message { get; set; } = "";
    public string Timestamp { get; set; } = "";
}

public class ProposalViewModel
{
    public int Id { get; set; }
    public int ObservationId { get; set; }
    public string Author { get; set; } = "";
    public string TaxonId { get; set; } = "";
    public string Timestamp { get; set; } = "";
}