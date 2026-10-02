namespace Bison.Razor.Models;

public class Author
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";

    public List<Post> Posts { get; set; } = new();
}