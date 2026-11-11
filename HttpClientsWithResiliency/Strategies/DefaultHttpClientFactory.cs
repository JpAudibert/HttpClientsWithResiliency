namespace HttpClientsWithResiliency.Strategies;

public class DefaultHttpClientFactory(string baseAddress) : IHttpClientFactory
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
            },
        };

        return httpClient;
    }
}
