using Bison.CLI;

namespace BisonTest;

public class FakeHttpService : ITHttpService
{
    public List<Cheep> Cheeps { get; } = new();
    public List<Comment> Comments { get; } = new();

    public Task<IReadOnlyList<Cheep>> GetCheepsAsync()
    {
        return Task.FromResult<IReadOnlyList<Cheep>>(Cheeps);
    }

    public Task SendCheepAsync(Cheep cheep)
    {
        Cheeps.Add(cheep);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Comment>> GetCommentsAsync(long observationId)
    {
        var comments = Comments
            .Where(c => c.ObservationId == observationId)
            .ToList();

        return Task.FromResult<IReadOnlyList<Comment>>(comments);
    }

    public Task SendCommentAsync(Comment comment)
    {
        Comments.Add(comment);
        return Task.CompletedTask;
    }

    public List<Proposal> Proposals { get; } = new();

    public Task PostProposalAsync(Proposal proposal)
    {
        Proposals.Add(proposal);
        return Task.CompletedTask;
    }

    public Task<List<Proposal>> GetProposalsAsync(long observationId)
    {
        var filtered = Proposals
            .Where(p => p.ObservationId == observationId)
            .ToList();

        return Task.FromResult(filtered);
    }

    public Task<IReadOnlyList<Cheep>> GetObservationsAsync()
    {
        return Task.FromResult<IReadOnlyList<Cheep>>(Cheeps);
    }


}