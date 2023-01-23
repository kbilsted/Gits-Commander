namespace GitsCommander.Gateways;

public record HttpClient(IHttpClientFactory httpClientFactory)
{
    public async Task<T> GetAsync<T>(Action<System.Net.Http.HttpClient> mutate, string uri)
    {
        var client = httpClientFactory.CreateClient();
        mutate(client);

        var response = await client.GetAsync(uri);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Cannot fetch from '{uri}' {response.ReasonPhrase}");
        string json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<T>(json);
        if (result == null)
            throw new NullReferenceException($"Got null from '{uri}'");

        return result;
    }
}
