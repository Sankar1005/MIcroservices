# Decision: shared contracts

**Decision:** a response type that another service reads lives in `shared/src/Microservices.Shared.Contracts`.
Everything else stays inside the service.

| Type | Where | Why |
|---|---|---|
| `ProductDto` (response of ProductService) | `Shared.Contracts/Products` | OrderService deserializes it, so both sides must use the same shape |
| `CreateProductDto`, `UpdateProductDto` | `ProductService.Api/Models` | Only ProductService's own clients send these |
| `Product` (table row) | `ProductService.Api/Models` | Internal: the database shape is never shared |

**Rules**
- A contract is public: renaming or removing a property breaks other services. Add new properties instead.
- `Microservices.Shared.Contracts.UnitTests` checks the JSON property names, so an accidental rename fails the build.
- Contracts contain no logic and no dependencies.
