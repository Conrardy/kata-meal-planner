# Backend Conventions (.NET)

## Architecture (Clean/Hexagonal)

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

### Architecture Patterns

- Dependency inversion: Domain depends on nothing
- Ports & Adapters: Interfaces in Application, implementations in Infrastructure
- DTOs at boundaries only

## Domain Layer Patterns

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

## Application Layer Patterns (CQRS)

- One handler per use case
- Commands mutate, return `ErrorOr<Unit>` or ID
- Queries read, return DTOs directly
- Pipeline behaviors for cross-cutting concerns (validation, logging, transactions)

### Custom Mediator

Custom mediator implementation in `Application/Common/Mediator/` (replaces MediatR).

**Core Interfaces:**
| Interface | Purpose |
|-----------|---------|
| `IMediator` | Dispatch requests via `Send<TResponse>()` and notifications via `Publish()` |
| `IRequest<TResponse>` | Marker for commands/queries |
| `IRequestHandler<TRequest, TResponse>` | Handler with `Handle(request, cancellationToken)` |
| `INotification` | Marker for domain events |
| `INotificationHandler<T>` | Event handler |
| `IPipelineBehavior<TRequest, TResponse>` | Cross-cutting behavior with `Handle(request, next, cancellationToken)` |
| `ITransactionalRequest` | Marker for commands requiring transaction wrapping |

**Registration:**
```csharp
// Program.cs - Assembly scanning for handlers
services.AddMediator<ApplicationAssemblyMarker>();

// Register behaviors in execution order
services.AddMediatorBehavior(typeof(LoggingBehavior<,>));
services.AddMediatorBehavior(typeof(ValidationBehavior<,>));
services.AddMediatorBehavior(typeof(TransactionBehavior<,>));
```

**Pipeline Behaviors:**
| Behavior | Purpose |
|----------|---------|
| `LoggingBehavior<,>` | Logs request start/end with duration |
| `ValidationBehavior<,>` | Runs FluentValidation validators, throws `ValidationException` |
| `TransactionBehavior<,>` | Wraps `ITransactionalRequest` in DB transaction via `IUnitOfWork` |

```csharp
// Command example
public sealed record PlanMealCommand(
    Guid MealPlanId,
    DateOnly Date,
    MealTime MealTime,
    Guid RecipeId
) : IRequest<ErrorOr<Unit>>, ITransactionalRequest;
```

## Infrastructure Layer Patterns

- Repository pattern for aggregate persistence
- Unit of Work via EF Core `DbContext` (implements `IUnitOfWork` for transaction control)
- Outbox pattern for reliable event publishing
- Read replicas for query optimization

### UserIdentityMap (Firebase Migration)

Table `user_identity_map` maps ASP.NET Identity users to Firebase UIDs for migration support.

```csharp
// Infrastructure/Identity/UserIdentityMap.cs
public sealed class UserIdentityMap
{
    public Guid AspNetUserId { get; private set; }
    public string FirebaseUid { get; private set; }  // max 128 chars, unique index
    public DateTime CreatedAt { get; private set; }
}
```

## API Layer Patterns

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

## Error Handling Patterns

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

### API Localization

Error messages are localized based on `Accept-Language` header (supports `fr`, `en`).

**Files:**
- `Api/Resources/SharedResource.resx` (default/fallback)
- `Api/Resources/SharedResource.fr.resx`
- `Api/Resources/SharedResource.en.resx`

**Usage:**
```csharp
// Api/Localization/ErrorLocalizer.cs
public string Localize(string errorCode, string fallbackMessage, params object[] args);
```

**Configuration** (Program.cs):
```csharp
builder.Services.AddLocalization();
app.UseRequestLocalization(new RequestLocalizationOptions { ... });
```

## Testing Patterns

- Unit tests for domain logic (no mocks needed for pure functions)
- Integration tests with Testcontainers
- Arrange-Act-Assert structure
- Test behavior, not implementation

## Observability Patterns

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

## Security Patterns

- JWT with short expiry + refresh tokens
- Role-based authorization
- Input validation at API boundary
- No sensitive data in logs

## Performance Patterns

- Pagination for all list endpoints
- Projection queries (select only needed fields)
- Batch operations for bulk updates
- Connection pooling

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
