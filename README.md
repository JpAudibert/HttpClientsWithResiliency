# HttpClientsWithResiliency

A demonstration of calling the Open Exchange Rates API with `HttpClient`, Flurl, and RestSharp.

All three routes share a named `HttpClient` configured with the .NET standard resilience handler:

- Up to three retries with exponential backoff and jitter for transient failures; retries are disabled for unsafe HTTP methods.
- A circuit breaker opens after at least five requests in a 30-second window when at least half fail, then permits requests again after 15 seconds.
- A 5-second per-attempt timeout and a 20-second overall request timeout.

The routes are `GET /exchange-rates/default`, `GET /exchange-rates/flurl`, and
`GET /exchange-rates/restsharp`. Each accepts `currency` and `appId` query parameters.
