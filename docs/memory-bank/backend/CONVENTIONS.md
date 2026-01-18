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

## Infrastructure Layer Patterns

- Repository pattern for aggregate persistence
- Unit of Work via EF Core `DbContext`
- Outbox pattern for reliable event publishing
- Read replicas for query optimization

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
