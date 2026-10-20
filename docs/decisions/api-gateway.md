# Decision: API Gateway with YARP

**Decision:** clients (the browser / Angular app) call only the API Gateway (`services/api-gateway`, port 5100).
The gateway forwards each request to the service that owns the path. Built with YARP.

```
Angular (4200) ──► ApiGateway (5100) ──► /api/products/** ──► ProductService (5101)
                                     └─► /api/orders/**   ──► OrderService   (5102)

OrderService ─────────────────────────────────────────────► ProductService (5101)   (direct, not via the gateway)
```

## Rules

- **Routing lives in configuration** (`ReverseProxy` in the gateway's `appsettings.json`): a *route* matches a path,
  a *cluster* lists where to send it. A new service is a config change, not a code change.
- **No business logic and no database in the gateway.** It forwards; services decide.
- **The gateway is for traffic from outside.** Service-to-service calls go directly between services.
- **CORS is handled by the gateway.** Routes remove the `Origin` header before forwarding, so the services
  never add their own CORS headers to gateway traffic (avoids duplicate `Access-Control-Allow-Origin`).
- **Services read forwarded headers** (`app.UseForwardedHeaders()` with `X-Forwarded-For/-Proto/-Host`), so links such as
  the `Location` header of `201 Created` point to the gateway (`http://localhost:5100/...`) and not to an internal port.
  Only proxies on the same machine are trusted by default; in production set `KnownProxies`/`KnownNetworks`.
- **A service being down affects only its routes.** The gateway answers `502 Bad Gateway` for those; others keep working.

## Later

- Authentication: validate the user's token once at the gateway (and again in each service).
- Rate limiting at the gateway (`AddRateLimiter`).
- Several copies of a service: add more destinations to a cluster; YARP load-balances between them,
  with active health checks to skip unhealthy copies.
- In production only the gateway is reachable from the internet; the services are on an internal network.
