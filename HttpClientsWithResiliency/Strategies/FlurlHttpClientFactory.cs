using Flurl.Http;

namespace HttpClientsWithResiliency.Strategies;

public class FlurlHttpClientFactory(string baseAddress)
{
    public FlurlClient CreateClient()
    {
        FlurlClient flurlClient = new()
        {
            BaseUrl = baseAddress,
            Headers = { { "Accept", "application/json" } },
        };

        flurlClient.AllowAnyHttpStatus();

        return flurlClient;
    }
}
