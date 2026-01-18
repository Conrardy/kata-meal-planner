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
| State | NgRx SignalStore | - | Global state with signal-based selectors |
| Async | RxJS | 7 | Async streams, HTTP, WebSocket |
| Styling | Tailwind CSS | 3 | Utility-first styling |
| UI | Angular CDK | - | Accessible primitives (dialogs, overlays, drag-drop) |
| Icons | Lucide Icons | - | Consistent iconography |
| Forms | Reactive Forms | - | Type-safe form handling |
| Validation | Zod | - | Runtime schema validation |
| HTTP | HttpClient | - | HTTP requests with interceptors |
| API Client | OpenAPI Generator | - | Type-safe API client generation |
| Testing | Karma | - | Test runner |
| Testing | Jasmine | - | Unit test framework |
| Observability | OpenTelemetry JS | - | Distributed tracing |

## Backend (.NET)

| Category | Technology | Version | Purpose |
|----------|------------|---------|---------|
| Framework | .NET | 9 | LTS, performance, native AOT support |
| Language | C# | 13 | Primary expressions, collection expressions |
| API Style | FastEndpoints | - | Endpoint-per-file, REPR pattern |
| Package Manager | NuGet | - | Dependency management |
| CQRS | MediatR | 14 | Command/Query dispatching |
| Validation | FluentValidation | - | Request validation |
| Error Handling | ErrorOr | - | Result monad for error handling |
| ORM | EF Core | 9 | ORM, migrations, query optimization |
| Database | PostgreSQL | 17 | Primary database |
| Cache | Redis | 7 | Distributed caching |
| Messaging | MassTransit | - | Message bus (RabbitMQ/Azure Service Bus) |
| Docs | Scalar | - | OpenAPI documentation |
| Logging | Serilog | - | Structured logging |
| Metrics | Prometheus | - | Metrics export |
| Tracing | OpenTelemetry .NET | - | Distributed tracing |
| Tracing | Jaeger | - | Trace visualization |
| Auth | ASP.NET Identity | - | User management |
| Auth | JWT Bearer | - | API authentication |
| Testing | xUnit | - | Test framework |
| Testing | FluentAssertions | - | Readable assertions |
| Testing | Testcontainers | - | Integration tests with real DB |
| Testing | Bogus | - | Test data generation |
| Testing | Respawn | - | Database reset between tests |

## Shared / Cross-Cutting

| Category | Technology | Purpose |
|----------|------------|---------|
| API Contract | OpenAPI 3.1 | API specification |
| Validation | Zod (FE) + FluentValidation (BE) | Consistent validation |
| Containerization | Docker | Containerization |
| Local Dev | Docker Compose | Local development |
| CI/CD | GitHub Actions | CI/CD |
| Orchestration | .NET Aspire | Cloud-native orchestration |

## Local Development Services

```yaml
# docker-compose.yml services
- postgres:17
- redis:7
- rabbitmq:3-management
- jaeger:latest
- seq:latest (optional, for log viewing)
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