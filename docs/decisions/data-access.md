# Decision: data access with ADO.NET

**Decision:** services use ADO.NET (`Microsoft.Data.SqlClient`) directly. No Entity Framework, no Dapper.

**Why:** the SQL that runs is exactly the SQL in the repository, which keeps it easy to read, debug and tune.

**Rules**
- SQL lives only in `Repositories/`. Controllers and services never touch `SqlConnection`.
- Every value goes through a `SqlParameter` with an explicit `SqlDbType` (and size/precision).
  Never build SQL by joining strings: this is what prevents SQL injection.
- `await using` for `SqlConnection`, `SqlCommand` and `SqlDataReader`, so connections always go back to the pool.
- Deletes are soft deletes (`IsActive = 0`). Reads filter on `IsActive = 1`.
- Each service has its own schema (`product`, later `ordering`, ...) and never queries another service's schema.
  To get another service's data, call its API.
- Schema changes are made only in `database/Microservices.Database` (see `database/README.md`).
