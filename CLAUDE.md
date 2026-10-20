# CLAUDE.md

Rules for AI agents and developers working in this repository. See README.md for the layout
and `docs/decisions/` for the reasons behind these rules.

## Hard rules

- **No secrets in the repository.** Connection strings use Windows Authentication only. Never commit passwords,
  keys or tokens (not in `appsettings*.json`, scripts, docs or tests).
- **Only change what the task is about.** Working on one service = change only `services/<that-service>/`.
  Changes to `shared/`, `database/` or root build files affect every service: mention them explicitly.
- **ADO.NET only** (`Microsoft.Data.SqlClient`). Do not add Entity Framework or Dapper.
- **Schema changes only in `database/Microservices.Database`** (SDK-style `.sqlproj`). No CREATE/ALTER scripts anywhere else.
- **NuGet versions only in `Directory.Packages.props`.** `PackageReference` items never have a `Version`.
- **No personal data in logs** (no bodies, headers, query strings, names or e-mails). See `docs/decisions/logging.md`.

## Layout

```
services/<name>-service/
├── src/<Name>Service.Api/        Web API project
└── tests/<Name>Service.UnitTests/
shared/src/Microservices.Shared.* libraries used by every service (ProjectReference)
shared/tests/<Library>.UnitTests  one test project per library
database/Microservices.Database   the database schema (one schema per service)
```

Folders inside `<Name>Service.Api`:
- `Controllers/`: thin; HTTP status codes only.
- `Services/`: business logic and Model ↔ DTO mapping.
- `Repositories/`: ADO.NET SQL.
- `Models/`: the table entity and request DTOs.
- Response DTOs read by other services go in `Microservices.Shared.Contracts` (`docs/decisions/contracts.md`).

## Code conventions

- Every service calls `builder.Services.AddSharedLogging()` and `app.UseSharedLogging()` (first in the pipeline).
- SQL: always `SqlParameter` with an explicit `SqlDbType`. Never build SQL by joining strings.
  `await using` for connection, command and reader.
- Deletes are soft deletes (`IsActive = 0`). Reads return active rows only.
- Each service keeps a `<Name>Service.Api.http` file with a request for every endpoint.
- New behaviour needs unit tests in the matching `*.UnitTests` project (`docs/decisions/testing.md`).
- Shared settings (`net10.0`, Nullable, ImplicitUsings) come from `Directory.Build.props`; don't repeat them in projects.

## Commands

```bash
dotnet build Microservices.slnx
dotnet test --solution Microservices.slnx
dotnet run --project services/product-service/src/ProductService.Api --launch-profile http
.\tools\deploy-database.ps1 -ReportOnly     # preview database changes
.\tools\deploy-database.ps1                 # apply database changes to (localdb)\MSSQLLocalDB
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
