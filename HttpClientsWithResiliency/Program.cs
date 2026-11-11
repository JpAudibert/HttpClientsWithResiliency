using Flurl.Http;
using HttpClientsWithResiliency.Models;
using HttpClientsWithResiliency.Strategies;
using RestSharp;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

RatesApiConfiguration ratesApiConfiguration =
    builder.Configuration.GetSection("RatesApi").Get<RatesApiConfiguration>()
    ?? new RatesApiConfiguration();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet(
    "/exchange-rates/default",
    async (string currency) =>
    {
        HttpClient httpClient = new DefaultHttpClientFactory(
            ratesApiConfiguration.BaseUrl
        ).CreateClient("default");

        string requestUri =
            $"latest.json?app_id={ratesApiConfiguration.AppId}&base={ratesApiConfiguration.BaseIsoCode}&symbols={currency}";

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
            ratesApiConfiguration.BaseUrl
        ).CreateClient();

        string requestUri = "latest.json";

        ExchangeRatesResponse response = await flurlClient
            .Request(requestUri)
            .SetQueryParams(
                new
                {
                    app_id = ratesApiConfiguration.AppId,
                    @base = ratesApiConfiguration.BaseIsoCode,
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
            ratesApiConfiguration.BaseUrl
        ).CreateClient();

        string requestUri =
            $"latest.json?app_id={ratesApiConfiguration.AppId}&base={ratesApiConfiguration.BaseIsoCode}&symbols={currency}";

        RestRequest request = new(requestUri, Method.Get);

        ExchangeRatesResponse? response = await restClient.GetAsync<ExchangeRatesResponse>(request);

        return response is not null
            ? Results.Ok(response)
            : Results.Problem($"Error retrieving exchange rates with RestSharp");
    }
);

app.Run();
