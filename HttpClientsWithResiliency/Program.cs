using HttpClientsWithResiliency.Models;
using HttpClientsWithResiliency.Routes;

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

app.AddExchangeRatesRoutes(ratesApiConfiguration);

app.Run();
