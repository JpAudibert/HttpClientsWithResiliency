using System;
using RestSharp;

namespace HttpClientsWithResiliency.Strategies;

public class RestSharpHttpClientFactory(string baseAddress)
{
    public IRestClient CreateClient()
    {
        RestClientOptions options = new()
        {
            BaseUrl = new Uri(baseAddress),
            ThrowOnAnyError = false,
            Timeout = TimeSpan.FromSeconds(10),
        };

        IRestClient restClient = new RestClient(options);

        return restClient;
    }
}
