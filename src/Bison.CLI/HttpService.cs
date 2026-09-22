using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

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

    // --- Proposals ---

    public async Task<List<Proposal>> GetProposalsAsync(long observationId)
    {
        var response = await _client.GetAsync($"/proposals?observationId={observationId}");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Proposal>>(json)!;
    }

    public async Task PostProposalAsync(Proposal proposal)
    {
        var json = JsonSerializer.Serialize(proposal);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/proposal", content);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<Cheep>> GetObservationsAsync()
    {
        var response = await _client.GetAsync("observations");
        response.EnsureSuccessStatusCode();

        var cheeps = await response.Content.ReadFromJsonAsync<List<Cheep>>();
        return cheeps ?? [];
    }




    public void Dispose() => _client.Dispose();
}
