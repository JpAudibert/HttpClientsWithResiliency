using Flurl.Http;
using HttpClientsWithResiliency.Models;
using Microsoft.AspNetCore.WebUtilities;
using Polly.CircuitBreaker;
using Polly.Timeout;
using System.Net;
using System.Text.Json;

namespace HttpClientsWithResiliency.Helpers;

internal static class ExchangeRatesRouteHelpers
{
    internal static string BuildRequestUri(
        RatesApiConfiguration configuration,
        string currency,
        string appId
    ) =>
        QueryHelpers.AddQueryString(
            "latest.json",
            new Dictionary<string, string?>
            {
                ["app_id"] = appId,
                ["base"] = configuration.BaseIsoCode,
                ["symbols"] = currency,
            }
        );

    internal static async Task<IResult> ExecuteSafely(
        Func<Task<IResult>> request,
        ILogger logger
    )
    {
        try
        {
            return await request();
        }
        catch (BrokenCircuitException exception)
        {
            logger.LogWarning(exception, "Rates API circuit breaker is open.");
            return Results.Problem(
                title: "Rates API is temporarily unavailable",
                statusCode: StatusCodes.Status503ServiceUnavailable
            );
        }
        catch (TimeoutRejectedException exception)
        {
            logger.LogWarning(exception, "Rates API request timed out.");
            return Results.Problem(
                title: "Rates API request timed out",
                statusCode: StatusCodes.Status504GatewayTimeout
            );
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "Request to Rates API failed.");
            return Results.Problem(
                title: "Error retrieving exchange rates",
                statusCode: StatusCodes.Status502BadGateway
            );
        }
        catch (FlurlHttpException exception)
        {
            logger.LogError(exception, "Flurl request to Rates API failed.");
            return Results.Problem(
                title: "Error retrieving exchange rates",
                statusCode: StatusCodes.Status502BadGateway
            );
        }
        catch (JsonException exception)
        {
            logger.LogError(exception, "Rates API returned an invalid response.");
            return Results.Problem(
                title: "Rates API returned an invalid response",
                statusCode: StatusCodes.Status502BadGateway
            );
        }
    }

    internal static IResult CreateUpstreamFailure(
        HttpStatusCode statusCode,
        string clientName,
        ILogger logger
    )
    {
        logger.LogWarning(
            "Rates API returned HTTP {StatusCode} to the {ClientName} client.",
            (int)statusCode,
            clientName
        );

        int responseStatusCode =
            statusCode == HttpStatusCode.TooManyRequests
                ? StatusCodes.Status503ServiceUnavailable
                : StatusCodes.Status502BadGateway;

        return Results.Problem(
            title: "Rates API returned an unsuccessful response",
            statusCode: responseStatusCode
        );
    }

    internal static IResult CreateInvalidResponse(string clientName, ILogger logger)
    {
        logger.LogWarning(
            "Rates API returned an empty response to the {ClientName} client.",
            clientName
        );

        return Results.Problem(
            title: "Rates API returned an invalid response",
            statusCode: StatusCodes.Status502BadGateway
        );
    }
}
