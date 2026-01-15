# Stack

> State-of-the-art Angular + .NET stack aligned with DDD, Craft, and Functional principles.

## Frontend (Angular)

@frontend/package.json

### Core

| Category | Technology | Purpose |
|----------|------------|---------|
| Framework | Angular 19 | Standalone components, signals-based reactivity |
| Language | TypeScript 5 | Strict mode enabled |
| Build | esbuild | Fast builds via Angular CLI |
| Package Manager | pnpm | Fast, disk-efficient |

### State Management

| Technology | Purpose |
|------------|---------|
| Angular Signals | Local component state, fine-grained reactivity |
| NgRx SignalStore | Global state with signal-based selectors |
| RxJS 7 | Async streams, HTTP, WebSocket |

**Patterns:**
- Signals for synchronous reactive state
- SignalStore for feature-level state slices
- `computed()` for derived state (pure functions)
- `effect()` sparingly, only for side effects at boundaries

### UI & Styling

| Technology | Purpose |
|------------|---------|
| Tailwind CSS 4 | Utility-first styling |
| Angular CDK | Accessible primitives (dialogs, overlays, drag-drop) |
| Lucide Icons | Consistent iconography |

**Patterns:**
- Component-scoped styles with Tailwind
- Design tokens via CSS custom properties
- Mobile-first responsive design

### Forms & Validation

| Technology | Purpose |
|------------|---------|
| Reactive Forms | Type-safe form handling |
| Zod | Runtime schema validation |

**Patterns:**
- Typed form groups with `FormGroup<T>`
- Zod schemas shared with backend DTOs
- Validation at boundaries only
- Form state as immutable snapshots

### HTTP & API Communication

| Technology | Purpose |
|------------|---------|
| HttpClient | HTTP requests with interceptors |
| OpenAPI Generator | Type-safe API client generation |

**Patterns:**
- Centralized error handling via interceptors
- Correlation ID injection via interceptor
- Retry with exponential backoff for transient failures
- Response caching with `TransferState` for SSR

### Testing

| Technology | Purpose |
|------------|---------|
| Vitest | Unit tests (fast, ESM-native) |
| Testing Library | Component testing (behavior-focused) |
| Playwright | E2E tests |
| MSW | API mocking |

**Patterns:**
- Test behavior, not implementation
- No mocking of functional components
- Given-When-Then structure
- `data-testid` for E2E selectors

### Observability

| Technology | Purpose |
|------------|---------|
| OpenTelemetry JS | Distributed tracing |
| Custom ErrorHandler | Centralized error capture |

**Patterns:**
- Structured logging to console in dev
- Trace context propagation via headers
- Performance marks for Core Web Vitals
- Error boundary components

### Project Structure

```
frontend/
├── src/
│   ├── app/
│   │   ├── core/                 # Singletons, guards, interceptors
│   │   ├── shared/               # Reusable components, pipes, directives
│   │   ├── features/             # Feature modules (package by feature)
│   │   │   ├── daily-digest/
│   │   │   ├── meal-plan/
│   │   │   ├── recipes/
│   │   │   └── shopping-list/
│   │   └── app.config.ts
│   ├── environments/
│   └── styles/
├── e2e/
└── package.json
```

---

## Backend (.NET)

@backend/backend.csproj

### Core

| Category | Technology | Purpose |
|----------|------------|---------|
| Framework | .NET 9 | LTS, performance, native AOT support |
| Language | C# 13 | Primary expressions, collection expressions |
| API Style | Minimal APIs + FastEndpoints | Thin controllers, endpoint-per-file |
| Package Manager | NuGet | Dependency management |

### Architecture (Clean/Hexagonal)

```
backend/
├── src/
│   ├── Domain/                   # Entities, Value Objects, Domain Events
│   ├── Application/              # Use cases, Commands, Queries, DTOs
│   ├── Infrastructure/           # EF Core, external services, repositories
│   └── Api/                      # Endpoints, middleware, configuration
└── tests/
    ├── Domain.Tests/
    ├── Application.Tests/
    └── Api.Tests/
```

**Patterns:**
- Dependency inversion: Domain depends on nothing
- Ports & Adapters: Interfaces in Application, implementations in Infrastructure
- DTOs at boundaries only

### Domain Layer

| Technology | Purpose |
|------------|---------|
| Records | Immutable Value Objects |
| Result pattern | Error handling without exceptions |

**Patterns:**
- Value Objects as `record` types with validation in constructor
- Entities with private setters, behavior methods
- Aggregates as consistency boundaries
- Domain Events as `record` types, past tense naming

```csharp
// Value Object example
public sealed record MealTime
{
    public static readonly MealTime Breakfast = new("Breakfast");
    public static readonly MealTime Lunch = new("Lunch");
    public static readonly MealTime Dinner = new("Dinner");

    public string Value { get; }
    private MealTime(string value) => Value = value;
}
```

### Application Layer (CQRS)

| Technology | Purpose |
|------------|---------|
| MediatR 12 | Command/Query dispatching |
| FluentValidation | Request validation |
| ErrorOr | Result monad for error handling |

**Patterns:**
- One handler per use case
- Commands mutate, return `Result<Unit>` or ID
- Queries read, return DTOs directly
- Pipeline behaviors for cross-cutting concerns (validation, logging)

```csharp
// Command example
public sealed record PlanMealCommand(
    Guid MealPlanId,
    DateOnly Date,
    MealTime MealTime,
    Guid RecipeId
) : IRequest<ErrorOr<Unit>>;
```

### Infrastructure Layer

| Technology | Purpose |
|------------|---------|
| EF Core 9 | ORM, migrations, query optimization |
| PostgreSQL | Primary database |
| Redis | Distributed caching |
| MassTransit | Message bus (RabbitMQ/Azure Service Bus) |

**Patterns:**
- Repository pattern for aggregate persistence
- Unit of Work via EF Core `DbContext`
- Outbox pattern for reliable event publishing
- Read replicas for query optimization

### API Layer

| Technology | Purpose |
|------------|---------|
| FastEndpoints | Endpoint-per-file, REPR pattern |
| Scalar | OpenAPI documentation |
| FluentValidation | Request validation |

**Patterns:**
- REPR: Request-Endpoint-Response pattern
- Versioning via URL (`/api/v1/`)
- Problem Details (RFC 9457) for errors
- Correlation ID middleware

```csharp
// Endpoint example
public sealed class GetDailyDigestEndpoint
    : Endpoint<GetDailyDigestRequest, DailyDigestResponse>
{
    public override void Configure()
    {
        Get("/api/v1/daily-digest/{Date}");
        AllowAnonymous();
    }
}
```

### Error Handling

| Technology | Purpose |
|------------|---------|
| ErrorOr | Result monad (Success/Failure) |
| Problem Details | Standardized error responses |

**Patterns:**
- No exceptions for business logic
- `ErrorOr<T>` for all use case results
- Map domain errors to HTTP status codes
- Structured error responses with codes

```csharp
public static class DomainErrors
{
    public static Error RecipeNotFound(Guid id) =>
        Error.NotFound("Recipe.NotFound", $"Recipe {id} not found");
}
```

### Testing

| Technology | Purpose |
|------------|---------|
| xUnit | Test framework |
| FluentAssertions | Readable assertions |
| Testcontainers | Integration tests with real DB |
| Bogus | Test data generation |
| Respawn | Database reset between tests |

**Patterns:**
- Unit tests for domain logic (no mocks needed for pure functions)
- Integration tests with Testcontainers
- Arrange-Act-Assert structure
- Test behavior, not implementation

### Observability

| Technology | Purpose |
|------------|---------|
| OpenTelemetry .NET | Traces, metrics, logs |
| Serilog | Structured logging |
| Prometheus | Metrics export |
| Jaeger | Distributed tracing |

**Patterns:**
- Structured logging with Serilog
- Correlation ID in all logs
- Four golden signals (latency, traffic, errors, saturation)
- Health checks (`/health/live`, `/health/ready`)

```csharp
// Logging example
Log.Information("Meal planned {@MealPlan}", new {
    MealPlanId = mealPlan.Id,
    Date = command.Date,
    MealTime = command.MealTime.Value
});
```

### Security

| Technology | Purpose |
|------------|---------|
| ASP.NET Identity | User management |
| JWT Bearer | API authentication |
| Data Protection | Encryption at rest |

**Patterns:**
- JWT with short expiry + refresh tokens
- Role-based authorization
- Input validation at API boundary
- No sensitive data in logs

### Performance

| Technology | Purpose |
|------------|---------|
| Response Caching | HTTP cache headers |
| Redis | Distributed cache |
| EF Core Compiled Queries | Query optimization |

**Patterns:**
- Pagination for all list endpoints
- Projection queries (select only needed fields)
- Batch operations for bulk updates
- Connection pooling

---

## Shared / Cross-Cutting

### API Contract

| Technology | Purpose |
|------------|---------|
| OpenAPI 3.1 | API specification |
| Zod (FE) + FluentValidation (BE) | Consistent validation |

**Patterns:**
- Contract-first API design
- Generate TypeScript client from OpenAPI spec
- Shared validation rules concept (not code)

### DevOps

| Technology | Purpose |
|------------|---------|
| Docker | Containerization |
| Docker Compose | Local development |
| GitHub Actions | CI/CD |
| .NET Aspire | Cloud-native orchestration |

### Local Development

```yaml
# docker-compose.yml services
- postgres:17
- redis:7
- rabbitmq:3-management
- jaeger:latest
- seq:latest (optional, for log viewing)
```

---

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

---

## Key Principles Applied

### From DDD Rules
- Value Objects as immutable records
- Aggregates as transaction boundaries
- Domain Events for decoupling
- CQRS separation

### From Craft Rules
- Small functions (< 20 lines)
- Tell Don't Ask
- Fail fast at boundaries
- Immutability by default

### From Functional Rules
- Pure functions for domain logic
- Result monad for error handling
- Declarative data transformations
- No shared mutable state

### From API Design Rules
- Resource-oriented URLs
- Proper HTTP status codes
- Versioning from day one
- Problem Details for errors

### From Observability Rules
- Structured logging everywhere
- Correlation IDs across services
- Four golden signals
- OpenTelemetry standard

### From Performance Rules
- Measure before optimizing
- Pagination always
- Appropriate caching
- Connection pooling
