namespace HttpClientsWithResiliency.Models;

public record ExchangeRatesResponse
{
    public string Disclaimer { get; init; } = string.Empty;
    public string License { get; init; } = string.Empty;
    public long Timestamp { get; init; }
    public string Base { get; init; } = string.Empty;
    public Dictionary<string, decimal> Rates { get; init; } = [];
}
