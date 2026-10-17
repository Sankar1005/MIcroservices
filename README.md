# Microservices

ASP.NET Core (.NET 10) microservices using **ADO.NET** (`Microsoft.Data.SqlClient`) and **SQL Server LocalDB**.

> **ProductService** is implemented (CRUD, `api/products`). The other services are still empty skeletons.
> Rules for contributors and AI agents, plus service ports, are in [CLAUDE.md](CLAUDE.md).

## Repository layout

```
Microservices_Project/
├── Microservices.slnx           ← one solution with all projects
├── Directory.Build.props        ← shared .NET settings (net10.0, Nullable, ImplicitUsings, test setup)
├── Directory.Packages.props     ← ONE place for all NuGet versions (central package management)
├── global.json                  ← pins the .NET SDK, the SQL project SDK and the test runner
├── dotnet-tools.json            ← local tools (SqlPackage)
├── CLAUDE.md                    ← rules for AI agents and developers
├── README.md
│
├── services/
│   ├── product-service/         ← products (CRUD)            src/ProductService.Api, tests/ProductService.UnitTests
│   ├── order-service/           ← orders (skeleton)
│   ├── customer-service/        ← customers (skeleton)
│   ├── payment-service/         ← payments (skeleton)
│   ├── inventory-service/       ← inventory (skeleton)
│   └── api-gateway/             ← single entry point for clients (skeleton, YARP)
│
├── shared/                      ← libraries used by all services (by ProjectReference)
│   ├── src/
│   │   ├── Microservices.Shared.Contracts/   ← types other services read, e.g. ProductDto
│   │   └── Microservices.Shared.Logging/     ← global error handler + request logging, no personal data
│   ├── tests/                   ← one *.UnitTests project per library
│   └── README.md
│
├── database/                    ← SDK-style .sqlproj (the ONLY place for schema changes)
├── frontend/                    ← Angular app (not created yet)
├── tools/                       ← helper scripts (deploy-database.ps1)
├── docs/
│   ├── decisions/               ← data-access.md, contracts.md, logging.md, testing.md
│   └── development/             ← setup.md
└── .github/
    └── workflows/               ← GitHub Actions (path-filtered: only changed parts build)
```

## Getting started

Full steps: [docs/development/setup.md](docs/development/setup.md).

```bash
.\tools\deploy-database.ps1
dotnet build Microservices.slnx
dotnet test --solution Microservices.slnx
dotnet run --project services/product-service/src/ProductService.Api --launch-profile http
```

Then open http://localhost:5101/swagger.
