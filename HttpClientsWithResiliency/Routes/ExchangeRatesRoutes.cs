using Flurl.Http;
using HttpClientsWithResiliency.Helpers;
using HttpClientsWithResiliency.Models;
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
            async (
                string currency,
                string appId,
                IHttpClientFactory clientFactory,
                ILogger<Program> logger,
                CancellationToken cancellationToken
            ) =>
            {
                return await ExchangeRatesRouteHelpers.ExecuteSafely(
                    async () =>
                    {
                        using HttpClient httpClient = clientFactory.CreateClient("RatesApi");
                        using HttpResponseMessage response = await httpClient.GetAsync(
                            ExchangeRatesRouteHelpers.BuildRequestUri(
                                configuration,
                                currency,
                                appId
                            ),
                            cancellationToken
                        );

                        if (!response.IsSuccessStatusCode)
                        {
                            return ExchangeRatesRouteHelpers.CreateUpstreamFailure(
                                response.StatusCode,
                                "default",
                                logger
                            );
                        }

                        ExchangeRatesResponse? rates =
                            await response.Content.ReadFromJsonAsync<ExchangeRatesResponse>(
                                cancellationToken: cancellationToken
                            );

                        return rates is null
                            ? ExchangeRatesRouteHelpers.CreateInvalidResponse("default", logger)
                            : Results.Ok(rates);
                    },
                    logger
                );
            }
        );

        app.MapGet(
            "/exchange-rates/flurl",
            async (
                string currency,
                string appId,
                IHttpClientFactory clientFactory,
                ILogger<Program> logger,
                CancellationToken cancellationToken
            ) =>
            {
                return await ExchangeRatesRouteHelpers.ExecuteSafely(
                    async () =>
                    {
                        using FlurlClient flurlClient = new(clientFactory.CreateClient("RatesApi"));
                        flurlClient.AllowAnyHttpStatus();

                        using IFlurlResponse response = await flurlClient
                            .Request("latest.json")
                            .SetQueryParams(
                                new
                                {
                                    app_id = appId,
                                    @base = configuration.BaseIsoCode,
                                    symbols = currency,
                                }
                            )
                            .GetAsync(cancellationToken: cancellationToken);

                        if (!response.ResponseMessage.IsSuccessStatusCode)
                        {
                            return ExchangeRatesRouteHelpers.CreateUpstreamFailure(
                                response.ResponseMessage.StatusCode,
                                "Flurl",
                                logger
                            );
                        }

                        ExchangeRatesResponse? rates =
                            await response.GetJsonAsync<ExchangeRatesResponse>();

                        return rates is null
                            ? ExchangeRatesRouteHelpers.CreateInvalidResponse("Flurl", logger)
                            : Results.Ok(rates);
                    },
                    logger
                );
            }
        );

        app.MapGet(
            "/exchange-rates/restsharp",
            async (
                string currency,
                string appId,
                IHttpClientFactory clientFactory,
                ILogger<Program> logger,
                CancellationToken cancellationToken
            ) =>
            {
                return await ExchangeRatesRouteHelpers.ExecuteSafely(
                    async () =>
                    {
                        using HttpClient httpClient = clientFactory.CreateClient("RatesApi");
                        RestClientOptions options = new()
                        {
                            BaseUrl = new Uri(configuration.BaseUrl),
                            ThrowOnAnyError = false,
                        };

                        using IRestClient restClient = new RestClient(
                            httpClient,
                            options,
                            disposeHttpClient: false
                        );

                        RestRequest request = new RestRequest("latest.json", Method.Get)
                            .AddQueryParameter("app_id", appId)
                            .AddQueryParameter("base", configuration.BaseIsoCode)
                            .AddQueryParameter("symbols", currency);

                        RestResponse<ExchangeRatesResponse> response =
                            await restClient.ExecuteAsync<ExchangeRatesResponse>(
                                request,
                                cancellationToken
                            );

                        if (!response.IsSuccessful)
                        {
                            return ExchangeRatesRouteHelpers.CreateUpstreamFailure(
                                response.StatusCode,
                                "RestSharp",
                                logger
                            );
                        }

                        return response.Data is null
                            ? ExchangeRatesRouteHelpers.CreateInvalidResponse("RestSharp", logger)
                            : Results.Ok(response.Data);
                    },
                    logger
                );
            }
        );

        return app;
    }
}
