namespace Bison.Razor.Models;

/// DTO used to transport the simplified observation data needed by the Razor views.
/// Only primitive display values are exposed here.

public class ObservationDTO
{
    public int Id { get; set; }
    public string Author { get; set; } = "";
    public string Message { get; set; } = "";
    public string Timestamp { get; set; } = "";
}
