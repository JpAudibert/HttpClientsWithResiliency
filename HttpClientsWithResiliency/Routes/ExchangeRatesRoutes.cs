using Flurl.Http;
using HttpClientsWithResiliency.Models;
using HttpClientsWithResiliency.Strategies;
using RestSharp;

namespace HttpClientsWithResiliency.Routes;

public static class ExchangeRatesRoutes
{
    public static WebApplication AddExchangeRatesRoutes(
        this WebApplication app,
        RatesApiConfiguration configuration
    )
    {
        app.MapGet(
            "/exchange-rates/default",
            async (string currency) =>
            {
                HttpClient httpClient = new DefaultHttpClientFactory(
                    configuration.BaseUrl
                ).CreateClient("default");

                string requestUri =
                    $"latest.json?app_id={configuration.AppId}&base={configuration.BaseIsoCode}&symbols={currency}";

                HttpResponseMessage response = await httpClient.GetAsync(requestUri);

                return response.IsSuccessStatusCode
                    ? Results.Ok(await response.Content.ReadFromJsonAsync<ExchangeRatesResponse>())
                    : Results.Problem($"Error retrieving exchange rates: {response.ReasonPhrase}");
            }
        );

        app.MapGet(
            "/exchange-rates/flurl",
            async (string currency) =>
            {
                FlurlClient flurlClient = new FlurlHttpClientFactory(
                    configuration.BaseUrl
                ).CreateClient();

                string requestUri = "latest.json";

                ExchangeRatesResponse response = await flurlClient
                    .Request(requestUri)
                    .SetQueryParams(
                        new
                        {
                            app_id = configuration.AppId,
                            @base = configuration.BaseIsoCode,
                            symbols = currency,
                        }
                    )
                    .GetJsonAsync<ExchangeRatesResponse>();

                return response is not null
                    ? Results.Ok(response)
                    : Results.Problem($"Error retrieving exchange rates with Flurl");
            }
        );

        app.MapGet(
            "/exchange-rates/restsharp",
            async (string currency) =>
            {
                IRestClient restClient = new RestSharpHttpClientFactory(
                    configuration.BaseUrl
                ).CreateClient();

                string requestUri =
                    $"latest.json?app_id={configuration.AppId}&base={configuration.BaseIsoCode}&symbols={currency}";

                RestRequest request = new(requestUri, Method.Get);

                ExchangeRatesResponse? response = await restClient.GetAsync<ExchangeRatesResponse>(
                    request
                );

                return response is not null
                    ? Results.Ok(response)
                    : Results.Problem($"Error retrieving exchange rates with RestSharp");
            }
        );

        return app;
    }
}
