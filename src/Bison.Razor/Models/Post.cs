namespace Bison.Razor.Models;

public abstract class Post
{
    public int PostId { get; set; }
    public string Text { get; set; } = "";
    public DateTime TimeStamp { get; set; }

    public int AuthorId { get; set; }
    public Author Author { get; set; } = null!;
}