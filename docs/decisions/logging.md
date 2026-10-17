# Decision: logging and error handling

**Decision:** every service calls `AddSharedLogging()` and `UseSharedLogging()` from `shared/src/Microservices.Shared.Logging`.

What it gives a service:
- **Global exception handler:** any unhandled exception becomes a `500` `application/problem+json` response with a
  generic message and a `traceId`. No stack trace or exception message reaches the client.
  The full exception is logged on the server, together with the same `traceId`.
- **Request logging:** one line per request: method, path, status code and duration.

**No personal data in logs**
- Never log request bodies, headers or query strings: they can contain names, e-mails or tokens.
- Log ids (product id, order id), not names or e-mail addresses.
- Use message templates (`"Product {ProductId} not found"`), not string concatenation.

To see request lines a service needs `"Microsoft.AspNetCore.HttpLogging": "Information"` under `Logging:LogLevel` in its `appsettings.json`.
