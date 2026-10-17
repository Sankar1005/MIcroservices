# shared

Libraries used by all services. A service uses them through a `ProjectReference` (never by copying code).

| Library | What it contains |
|---|---|
| `src/Microservices.Shared.Contracts` | Types that more than one service uses, e.g. `ProductDto` (what ProductService returns and OrderService reads) |
| `src/Microservices.Shared.Logging` | `AddSharedLogging()` / `UseSharedLogging()`: global exception handler (clean 500, no stack trace) and request logging without personal data |

Each library has one test project in `tests/` named `<Library>.UnitTests`.

Rules:
- Keep these libraries small and stable: a change here rebuilds and retests **every** service.
- No service-specific logic and no database access in `shared/`.
- Planned when needed: `Microservices.Shared.Auth` (token check, current user) and `Microservices.Shared.Testing` (test helpers).
