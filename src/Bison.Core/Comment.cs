namespace Bison.Core;

public class Comment : Post
{
    public int ObservationId { get; set; }
    public Observation Observation { get; set; } = null!;
}