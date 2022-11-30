using System.Net;

namespace RepositoryAnalysis.Internal;

public class RepositoryVerifier
{
    private readonly HttpClient _client;

    public RepositoryVerifier(
        HttpClient client) =>
        _client = client;

    public async Task<bool> RepositoryExists(
        string url)
    {
        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));
        return response.StatusCode switch
        {
            HttpStatusCode.OK => true,
            HttpStatusCode.NotFound => false,
            _ => throw new Exception("something went wrong")
        };
    }
}