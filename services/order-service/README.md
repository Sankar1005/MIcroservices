# order-service

Orders. Placing an order calls **ProductService** to check each product and copy its name and price.

| | |
|---|---|
| Project | `src/OrderService.Api` |
| Tests | `tests/OrderService.UnitTests` |
| Ports | http://localhost:5102, https://localhost:7102 |
| Swagger | http://localhost:5102/swagger (Development) |
| Schema | `ordering` (`Orders`, `OrderItems`) in `database/Microservices.Database` |
| Depends on | ProductService at `Services:ProductService:BaseUrl` (default http://localhost:5101/) |
| Try it | `src/OrderService.Api/OrderService.Api.http` (start ProductService first) |

| Endpoint | Result |
|---|---|
| `GET api/orders` | 200 with the active orders and their items |
| `GET api/orders/{id}` | 200, or 404 (does not call ProductService) |
| `POST api/orders` | 201; 400 if a product does not exist or stock is too low; 503 if ProductService is down |
| `DELETE api/orders/{id}` | 204 or 404 (soft delete = cancel) |

Request flow for `POST`: `OrdersController` → `OrderService` → `ProductServiceClient` (HTTP GET to ProductService,
once per product) → `OrderRepository` (INSERT order + items in one transaction).
See `docs/decisions/service-communication.md`.
