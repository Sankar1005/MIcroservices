# Decision: how services talk to each other

**Decision:** services call each other with synchronous HTTP (REST/JSON) through a typed `HttpClient`.
A service never reads another service's tables.

## Example: OrderService → ProductService

```
Client ──POST /api/orders──► OrderService (5102)
                               │  for each product in the order:
                               ├──GET /api/products/{id}──► ProductService (5101) ──► product.Products
                               │◄──── 200 ProductDto / 404 ─┘
                               │  copy name + price, check stock, total
                               └──► INSERT ordering.Orders + ordering.OrderItems (one transaction)
Client ◄──201 Created── OrderService
```

## Rules

- **Typed client per remote service**, in `Clients/` (e.g. `IProductServiceClient` / `ProductServiceClient`).
  The service layer depends on the interface, so unit tests use a fake and never do real HTTP.
- **Base address from configuration** (`Services:<Name>:BaseUrl` in `appsettings.json`), never hard-coded.
- **Resilience on every client** (`AddStandardResilienceHandler`): retries with backoff, per-try and total
  timeouts, and a circuit breaker. Only safe calls (GET) are retried automatically.
- **Shared response types** come from `Microservices.Shared.Contracts` (see `contracts.md`).
- **Map remote failures to clear answers**: remote 404 → our 400 ("product does not exist");
  remote down/timeout → our 503 ("try again later"). Never let it become a 500.
- **Snapshot what you need**: OrderService stores product name and price in its own table when the order is placed,
  so reading an order never depends on ProductService being up, and later price changes don't alter old orders.

## Known limits (next steps for learning)

- **Stock is checked, not reserved.** Two orders at the same moment can both pass the check.
  Fixing this needs ProductService to own a "reserve stock" operation, or events (see below).
- **One HTTP call per product.** Fine for small orders; a batch endpoint (`GET api/products?ids=1,2,3`) would reduce calls.
- **Synchronous coupling.** If ProductService is down, nobody can place orders. The usual next step is
  asynchronous messaging (a message broker, e.g. RabbitMQ): ProductService publishes "ProductPriceChanged",
  OrderService keeps its own copy, and orders no longer need ProductService to be up.
