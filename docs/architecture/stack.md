# Stack

> State-of-the-art Angular + .NET stack aligned with DDD, Craft, and Functional principles.

## Frontend (Angular)

| Category | Technology | Version | Purpose |
|----------|------------|---------|---------|
| Framework | Angular | 19 | Standalone components, signals-based reactivity |
| Language | TypeScript | 5.7+ | Strict mode enabled |
| Build | esbuild | - | Fast builds via Angular CLI |
| Package Manager | npm | - | Dependency management |
| State | Angular Signals | - | Local component state, fine-grained reactivity |
| Async | RxJS | 7 | Async streams, HTTP, WebSocket |
| Styling | Tailwind CSS | 3 | Utility-first styling |
| Icons | Lucide Icons | - | Consistent iconography |
| Forms | Reactive Forms | - | Type-safe form handling |
| HTTP | HttpClient | - | HTTP requests with interceptors |
| i18n | @angular/localize | 19 | Internationalization (fr/en) |
| Testing | Vitest | 2 | Test runner + framework |
| Testing | @analogjs/vitest-angular | 1 | Angular Vitest integration |

## Backend (.NET)

| Category | Technology | Version | Purpose |
|----------|------------|---------|---------|
| Framework | .NET | 9 | LTS, performance, native AOT support |
| Language | C# | 13 | Primary expressions, collection expressions |
| API Style | Minimal API | - | Route-based endpoints in Program.cs |
| Package Manager | NuGet | - | Dependency management |
| CQRS | Custom Mediator | - | Internal dispatcher with pipeline behaviors |
| Validation | FluentValidation | 11 | Request validation |
| Error Handling | ErrorOr | 2 | Result monad for error handling |
| ORM | EF Core | 9 | ORM, migrations, query optimization |
| Database | PostgreSQL | 17 | Primary database |
| Cache | Redis | 7 | Distributed caching |
| Logging | Serilog | 9 | Structured logging |
| Auth | ASP.NET Identity | - | User management |
| Auth | JWT Bearer | 9 | API authentication |
| i18n | Microsoft.Extensions.Localization | 10 | Backend message localization |
| Testing | xUnit | - | Test framework |
| Testing | FluentAssertions | - | Readable assertions |
| Testing | Testcontainers | - | Integration tests with real DB |
| Testing | Bogus | - | Test data generation |
| Testing | Respawn | - | Database reset between tests |

## Shared / Cross-Cutting

| Category | Technology | Purpose |
|----------|------------|---------|
| API Contract | OpenAPI 3.1 | API specification |
| Validation | FluentValidation (BE) | Request validation |
| Containerization | Docker | Containerization |
| Local Dev | Docker Compose | Local development |
| Documentation | MkDocs + Material | Static documentation site |
| CI/CD | GitHub Actions | CI/CD |

## Local Development Services

```yaml
# docker-compose.yml services
- postgres:17
- redis:7
```

## Version Summary

| Component | Version | Notes |
|-----------|---------|-------|
| Angular | 19 | Standalone, signals |
| TypeScript | 5.7+ | Strict mode |
| .NET | 9 | LTS |
| C# | 13 | Latest features |
| PostgreSQL | 17 | Primary DB |
| Redis | 7 | Caching |
| Node.js | 22 LTS | Frontend tooling |