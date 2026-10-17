# Developer setup

## Install
- .NET SDK 10.0.401 or a later 10.0 feature band (pinned in `global.json`)
- SQL Server LocalDB (comes with Visual Studio) and SSMS
- Visual Studio 2022/2026 with the ASP.NET workload.
  To open `database/Microservices.Database` in Visual Studio, also install the
  "SQL Server Data Tools, SDK-style" component. Building from the command line works without it.

## First run
```bash
# 1. Create the database once in SSMS: right-click Databases → New Database → "Microservices"

# 2. Create/upgrade the tables and sample data
.\tools\deploy-database.ps1

# 3. Build and test everything
dotnet build Microservices.slnx
dotnet test --solution Microservices.slnx

# 4. Run a service
dotnet run --project services/product-service/src/ProductService.Api --launch-profile http
```
Then open http://localhost:5101/swagger. Service ports are listed in `CLAUDE.md`.

## Adding a NuGet package
1. Add the version once in `Directory.Packages.props`: `<PackageVersion Include="Name" Version="x.y.z" />`
2. In the project: `<PackageReference Include="Name" />` (no Version).
