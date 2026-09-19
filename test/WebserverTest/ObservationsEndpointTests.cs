namespace test;

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

public class ObservationsEndpointTests
{
    private static readonly HttpClient Client = new()
    {
        BaseAddress = new Uri("http://localhost:5195")
    };

    [Fact]
    public async Task GetObservationsReturns200WithJsonListOfObservations()
    {
        // When: an HTTP GET request is sent to the /observations endpoint.
        using var response = await Client.GetAsync("observations");
        var body = await response.Content.ReadAsStringAsync();

        // Then: the response status code is 200 OK...
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // ...and the body is a JSON list of Observation objects.
        using var document = JsonDocument.Parse(body);
        var list = document.RootElement;

        Assert.Equal(JsonValueKind.Array, list.ValueKind);

        foreach (var observation in list.EnumerateArray())
        {
            Assert.Equal(JsonValueKind.Object, observation.ValueKind);
            Assert.True(observation.TryGetProperty("id", out var id),
                $"Observation is missing the 'id' property: {body}");
            Assert.Equal(JsonValueKind.Number, id.ValueKind);
            Assert.True(observation.TryGetProperty("author", out _),
                $"Observation is missing the 'author' property: {body}");
            Assert.True(observation.TryGetProperty("message", out _),
                $"Observation is missing the 'message' property: {body}");
            Assert.True(observation.TryGetProperty("timestamp", out var timestamp),
                $"Observation is missing the 'timestamp' property: {body}");
            Assert.Equal(JsonValueKind.Number, timestamp.ValueKind);
            Assert.True(observation.TryGetProperty("location", out _),
                $"Observation is missing the 'location' property: {body}");
        }
    }

    [Fact]
    public async Task PostObservationReturns201()
    {
        // POST /observation appends to the server's observations.csv, so back up
        // the store and restore it afterwards.
        var observeDb = ServerData.Observations;
        var commentDb = ServerData.Comments;
        var observeBackup = Path.GetTempFileName();
        var commentBackup = Path.GetTempFileName();
        File.Copy(observeDb, observeBackup, true);
        File.Copy(commentDb, commentBackup, true);

        try
        {
            // Given: an Observation object serialized to JSON.
            var observation = new
            {
                id = 987654L,
                author = $"webserver test {Guid.NewGuid():N}",
                message = "posted by the webserver test",
                timestamp = 1684229348L,
                location = "DR Byen"
            };

            // When: it is sent as the body of a POST request to /observation.
            using var response = await Client.PostAsJsonAsync("observation", observation);
            var body = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"POST /observation -> {(int)response.StatusCode} {response.StatusCode}");
            Console.WriteLine(body);

            // Then: the response status code is 201 Created, since the request
            // creates a new observation resource on the server.
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }
        finally
        {
            File.Copy(observeBackup, observeDb, true);
            File.Copy(commentBackup, commentDb, true);
            File.Delete(observeBackup);
            File.Delete(commentBackup);
        }
    }
}
