# product-service

Products catalogue: CRUD on `product.Products` with ADO.NET.

| | |
|---|---|
| Project | `src/ProductService.Api` |
| Tests | `tests/ProductService.UnitTests` |
| Ports | http://localhost:5101, https://localhost:7101 |
| Swagger | http://localhost:5101/swagger (Development) |
| Schema | `product` in `database/Microservices.Database` |
| Try it | `src/ProductService.Api/ProductService.Api.http` |

| Endpoint | Result |
|---|---|
| `GET api/products` | 200 with the active products |
| `GET api/products/{id}` | 200, or 404 |
| `POST api/products` | 201 with the new product, or 400 |
| `PUT api/products/{id}` | 204, 404 or 400 |
| `DELETE api/products/{id}` | 204 or 404 (soft delete) |

Request flow: `ProductsController` → `ProductService` (DTO mapping) → `ProductRepository` (SQL) → `product.Products`.
The response type `ProductDto` lives in `shared/src/Microservices.Shared.Contracts` so OrderService can reuse it.
