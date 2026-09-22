namespace Bison.CLI;

public interface ITHttpService
{
    Task SendCheepAsync(Cheep cheep);
    Task<IReadOnlyList<Cheep>> GetCheepsAsync();
    Task SendCommentAsync(Comment comment);
    Task<IReadOnlyList<Comment>> GetCommentsAsync(long observationId);
}