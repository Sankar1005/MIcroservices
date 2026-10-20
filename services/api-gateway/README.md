# api-gateway

The single entry point for clients (browser / Angular app). Built with **YARP** (Yet Another Reverse Proxy).
No business logic and no database: it only forwards requests.

| | |
|---|---|
| Project | `src/ApiGateway` |
| Tests | `tests/ApiGateway.UnitTests` (checks the routing configuration) |
| Ports | http://localhost:5100, https://localhost:7100 |
| Routing | `src/ApiGateway/appsettings.json` → `ReverseProxy` |
| Try it | `src/ApiGateway/ApiGateway.http` (start ProductService and OrderService first) |

| Public path | Forwarded to |
|---|---|
| `/api/products/**` | ProductService, http://localhost:5101 |
| `/api/orders/**` | OrderService, http://localhost:5102 |
| `/health` | answered by the gateway |
| anything else | 404 from the gateway |

What the gateway does: routing, CORS for `http://localhost:4200`, request logging, and adding
`X-Forwarded-Host/-Proto/-For` headers so the services build links with the gateway's public address.
When a service is down the gateway answers **502 Bad Gateway**; the other routes keep working.

To add a service: add one route and one cluster in `appsettings.json`, plus a test case in
`RoutingConfigurationTests`. See `docs/decisions/api-gateway.md`.
