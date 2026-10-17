# database

`Microservices.Database/` is an SDK-style SQL project (`Microsoft.Build.Sql`). It is the **only** place for schema changes.

```
Microservices.Database/
├── Microservices.Database.sqlproj
├── product/                       ← one folder per schema (one schema per service)
│   ├── product.sql                ← CREATE SCHEMA
│   └── Tables/Products.sql        ← CREATE TABLE (write the final shape, not ALTER statements)
└── Scripts/Script.PostDeployment.sql   ← sample data, safe to run again
```

How to change the schema:
1. Edit or add the `CREATE ...` file (for example add a column to `product/Tables/Products.sql`).
2. `dotnet build database/Microservices.Database/Microservices.Database.sqlproj`: catches mistakes such as a wrong column name.
3. `.\tools\deploy-database.ps1 -ReportOnly`: shows what would change.
4. `.\tools\deploy-database.ps1`: applies it. SqlPackage writes the ALTER statements for you and stops if data would be lost.
