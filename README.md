# Microservices

ASP.NET Core (.NET 10) microservices solution using **ADO.NET** (`Microsoft.Data.SqlClient`) for data access and **SQL Server LocalDB** as the database.

> This is the project skeleton only. The services contain no business logic yet.

## Solution structure

```
Microservices_Project/
├── Microservices.slnx
└── src/
    ├── ApiGateway/                  # Entry point for clients (YARP reverse proxy package added)
    └── Services/
        ├── ProductService/
        ├── OrderService/
        ├── CustomerService/
        ├── PaymentService/
        └── InventoryService/
```

Every service is an ASP.NET Core Web API project with the same layout:

```
<Name>Service/
├── Controllers/        # API endpoints
├── Models/             # Entities and request/response models
├── Repositories/       # ADO.NET data access (SqlConnection / SqlCommand)
├── Properties/launchSettings.json
├── appsettings.json    # Connection string to the Microservices database
└── Program.cs
```

## Database

All services connect to the `Microservices` database on `(localdb)\MSSQLLocalDB` using Windows Authentication.
The connection string is in each service's `appsettings.json` under `ConnectionStrings:MicroservicesDb`:

```
Server=(localdb)\MSSQLLocalDB;Database=Microservices;Integrated Security=True;TrustServerCertificate=True
```

Read it in code with `builder.Configuration.GetConnectionString("MicroservicesDb")`.

## Getting started

Requirements: .NET 10 SDK, SQL Server LocalDB, and a `Microservices` database created in SSMS.

```bash
dotnet build Microservices.slnx
dotnet run --project src/Services/ProductService
```

In Visual Studio, open `Microservices.slnx` and use **Configure Startup Projects → Multiple startup projects** to run several services at once.
