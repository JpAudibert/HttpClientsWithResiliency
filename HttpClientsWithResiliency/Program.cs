using HttpClientsWithResiliency.Models;
using HttpClientsWithResiliency.Strategies;

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
            ratesApiConfiguration.BaseUrl,
            ratesApiConfiguration.AppId
        ).CreateClient("default");

        string requestUri =
            $"latest.json?app_id={ratesApiConfiguration.AppId}&base={ratesApiConfiguration.BaseIsoCode}&symbols={currency}";

        HttpResponseMessage response = await httpClient.GetAsync(requestUri);

        return response.IsSuccessStatusCode
            ? Results.Ok(await response.Content.ReadFromJsonAsync<ExchangeRatesResponse>())
            : Results.Problem($"Error retrieving exchange rates: {response.ReasonPhrase}");
    }
);

app.Run();
