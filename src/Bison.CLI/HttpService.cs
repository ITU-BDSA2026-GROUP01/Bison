using System.Net.Http.Headers;
using Microsoft.VisualBasic;
using Bison.CLI;

var client = new HttpClient();
client.DefaultRequestHeaders.Accept.Clear();
client.DefaultRequestHeaders.Accept.Add(
    new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));

static async Task Post(HttpClient client, string endpoint, Bison.CLI.Cheep cheep)
{
    
}

static async Task<string> Get(HttpClient client, string endpoint)
{
    var json = await client.GetStringAsync(
    "https://api.github.com/orgs/dotnet/repos");

    return json;
}

