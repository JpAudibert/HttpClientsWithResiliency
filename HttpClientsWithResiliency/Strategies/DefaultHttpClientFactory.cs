namespace HttpClientsWithResiliency.Strategies;

public class DefaultHttpClientFactory(string baseAddress, string apiKey) : IHttpClientFactory
{
    public HttpClient CreateClient(string name)
    {
        HttpClient httpClient = new()
        {
            BaseAddress = new Uri(baseAddress),
            Timeout = TimeSpan.FromSeconds(10),
            DefaultRequestHeaders =
            {
                { "Accept", "application/json" },
                { "Authorization", $"Bearer {apiKey}" },
            },
        };

        return httpClient;
    }
}
