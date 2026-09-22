using System.Net.Http.Json;

namespace Bison.CLI;

public sealed class HttpService : ITHttpService, IDisposable
{
    private readonly HttpClient _client;

    public HttpService(string baseUrl = "http://localhost:5195")
    {
        _client = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    // --- Cheeps (observations) ---

    public async Task SendCheepAsync(Cheep cheep)
    {
        var response = await _client.PostAsJsonAsync("observation", cheep);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<Cheep>> GetCheepsAsync()
    {
        var response = await _client.GetAsync("observations");
        response.EnsureSuccessStatusCode();
        var cheeps = await response.Content.ReadFromJsonAsync<List<Cheep>>();
        return cheeps ?? [];
    }

    // --- Comments ---

    public async Task SendCommentAsync(Comment comment)
    {
        var response = await _client.PostAsJsonAsync("comment", comment);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<Comment>> GetCommentsAsync(long observationId)
    {
        var response = await _client.GetAsync($"comments?id={observationId}");
        response.EnsureSuccessStatusCode();
        var comments = await response.Content.ReadFromJsonAsync<List<Comment>>();
        return comments ?? [];
    }

    public void Dispose() => _client.Dispose();
}
