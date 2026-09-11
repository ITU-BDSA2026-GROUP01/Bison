using CsvHelper.Configuration;

namespace Bison.CLI;

public record Cheep(long Id, string Author, string Message, long Timestamp)
{
    public Cheep() : this(0, string.Empty, string.Empty, 0) { }
}

public sealed class CheepMap : ClassMap<Cheep>
{
    public CheepMap()
    {
        Map(item => item.Id).Name("Id");
        Map(item => item.Author).Name("Author");
        Map(item => item.Message).Name("Message");
        Map(item => item.Timestamp).Name("Timestamp");
    }
}
