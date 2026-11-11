namespace HttpClientsWithResiliency.Models;

public record RatesApiConfiguration
{
    public string BaseUrl { get; init; } = string.Empty;
    public string AppId { get; init; } = string.Empty;
    public string BaseIsoCode { get; init; } = string.Empty;
    public string IsoCodesToSearch { get; init; } = string.Empty;
}
