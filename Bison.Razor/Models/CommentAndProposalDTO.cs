
namespace Bison.Razor.Models;


/// DTO used to transport comment data to the UI without exposing the underlying repository model.
public class CommentViewModel
{
    public int Id { get; set; }
    public int ObservationId { get; set; }
    public string Author { get; set; } = "";
    public string Message { get; set; } = "";
    public string Timestamp { get; set; } = "";
}


/// DTO used to transport proposal data to the UI without exposing the underlying repository model.

public class ProposalViewModel
{
    public int Id { get; set; }
    public int ObservationId { get; set; }
    public string Author { get; set; } = "";
    public string TaxonId { get; set; } = "";
    public string Timestamp { get; set; } = "";
}