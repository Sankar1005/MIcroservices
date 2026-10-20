# CLAUDE.md

Guidance for working in this repository. See README.md for the overall layout.

## Project

- .NET 10 microservices solution: `Microservices.slnx`.
- Data access is **ADO.NET only** (`Microsoft.Data.SqlClient`). Do not add Entity Framework.
- Database: `Microservices` on `(localdb)\MSSQLLocalDB`, Windows Authentication.
  Connection string name: `MicroservicesDb` (in each service's `appsettings.json`).

## Conventions

- One Web API project per service in `src/Services/<Name>Service`. Gateway: `src/ApiGateway`.
- Only change the service you are working on.
- Layers inside a service:
  - `Controllers/`: thin; HTTP status codes only.
  - `Services/`: business logic and Model ↔ DTO mapping.
  - `Repositories/`: ADO.NET SQL.
  - `Models/`: the table entity and the DTOs.
- SQL: always use `SqlParameter`, never build SQL by joining strings.
  Use `await using` for connection, command and reader.
- Deletes are soft deletes (`IsActive = 0`). Reads return active rows only.
- Each service owns its own schema (ProductService → `product`).
  Scripts live in `database/`, are numbered (`001_...sql`), and must be safe to run again.
- Each service keeps a `<Name>Service.http` file with a request for every endpoint.

## Commands

```bash
dotnet build Microservices.slnx
dotnet run --project src/Services/ProductService --launch-profile http
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -d Microservices -i database\001_product_schema.sql
```

## Service ports

| Service          | HTTP                  | HTTPS                  | Swagger UI (Development)      |
|------------------|-----------------------|------------------------|-------------------------------|
| ProductService   | http://localhost:5101 | https://localhost:7101 | http://localhost:5101/swagger |
| OrderService     | not fixed yet         | not fixed yet          |                               |
| CustomerService  | not fixed yet         | not fixed yet          |                               |
| PaymentService   | not fixed yet         | not fixed yet          |                               |
| InventoryService | not fixed yet         | not fixed yet          |                               |
| ApiGateway       | not fixed yet         | not fixed yet          |                               |

Other services call ProductService at `http://localhost:5101/api/products/{id}`
(HTTPS redirection is turned off in Development).
