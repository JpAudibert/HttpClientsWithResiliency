using HttpClientsWithResiliency.Models;
using HttpClientsWithResiliency.Routes;
using Microsoft.Extensions.Http.Resilience;
using System.Net.Http.Headers;
using Polly;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

RatesApiConfiguration ratesApiConfiguration =
    builder.Configuration.GetSection("RatesApi").Get<RatesApiConfiguration>()
    ?? new RatesApiConfiguration();

builder.Services
    .AddHttpClient("RatesApi", client =>
    {
        client.BaseAddress = new Uri(ratesApiConfiguration.BaseUrl);
        client.Timeout = Timeout.InfiniteTimeSpan;
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json")
        );
    })
    .AddStandardResilienceHandler(options =>
    {
        options.Retry.MaxRetryAttempts = 3;
        options.Retry.Delay = TimeSpan.FromMilliseconds(500);
        options.Retry.BackoffType = DelayBackoffType.Exponential;
        options.Retry.UseJitter = true;
        options.Retry.DisableForUnsafeHttpMethods();

        options.CircuitBreaker.FailureRatio = 0.5;
        options.CircuitBreaker.MinimumThroughput = 5;
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
        options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(15);

        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(20);
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(5);
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.AddExchangeRatesRoutes(ratesApiConfiguration);

app.Run();
