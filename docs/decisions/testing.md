# Decision: testing

**Decision:** xUnit v3, running on Microsoft.Testing.Platform (set in `global.json`).

**Layout**
- Each service: `services/<name>-service/tests/<Name>Service.UnitTests`.
- Each shared library: `shared/tests/<Library>.UnitTests`.
- Test packages are added automatically to every project whose name ends in `.UnitTests` (`Directory.Build.props`).

**What unit tests cover**
- Service layer: mapping, rules (e.g. `UpdatedAt` is set, soft delete hides the product).
- Controllers: the HTTP status code of each endpoint.
- Unit tests never touch the database. Use an in-memory fake of the repository interface
  (see `ProductService.UnitTests/FakeProductRepository.cs`).
- Repositories (real SQL) are checked by running the service against LocalDB with the `.http` file.
  Integration tests against a real database can be added later as `<Name>Service.IntegrationTests`.

**Run**
```bash
dotnet test --solution Microservices.slnx
```
Test names describe behaviour: `GetById_returns_404_when_missing`.
