using CsvHelper.Configuration;

namespace Bison.CLI;

public record Comment(long ObservationId, string Author, string Message, long Timestamp)
{
    public Comment() : this(0, string.Empty, string.Empty, 0) { }
}

public sealed class CommentMap : ClassMap<Comment>
{
    public CommentMap()
    {
        Map(item => item.ObservationId).Name("ObservationId");
        Map(item => item.Author).Name("Author");
        Map(item => item.Message).Name("Message");
        Map(item => item.Timestamp).Name("Timestamp");
    }
}
