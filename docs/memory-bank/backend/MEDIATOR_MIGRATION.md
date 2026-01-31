# Custom Mediator Migration Guide

## Overview

This document describes the migration from MediatR to a custom mediator implementation completed in US-023. The custom mediator provides equivalent functionality while eliminating a commercial dependency.

## Migration Summary

| Item | Before | After |
|------|--------|-------|
| Package | `MediatR 14.0.0` | Custom implementation |
| Request Interface | `MediatR.IRequest<TResponse>` | `MealPlanner.Application.Common.Mediator.IRequest<TResponse>` |
| Handler Interface | `MediatR.IRequestHandler<TRequest, TResponse>` | `MealPlanner.Application.Common.Mediator.IRequestHandler<TRequest, TResponse>` |
| Unit Type | `MediatR.Unit` | `MealPlanner.Application.Common.Mediator.Unit` |
| Mediator Interface | `MediatR.IMediator` | `MealPlanner.Application.Common.Mediator.IMediator` |

## Custom Mediator Components

Located in `MealPlanner.Application/Common/Mediator/`:

| File | Purpose |
|------|---------|
| `IRequest.cs` | Marker interface for commands/queries |
| `IRequestHandler.cs` | Handler interface with `Handle(TRequest, CancellationToken)` |
| `INotification.cs` | Marker interface for domain events |
| `INotificationHandler.cs` | Handler for notifications |
| `IMediator.cs` | Dispatcher interface with `Send()` and `Publish()` |
| `Mediator.cs` | Implementation with DI resolution and pipeline execution |
| `IPipelineBehavior.cs` | Pipeline behavior contract |
| `ITransactionalRequest.cs` | Marker for transactional commands |
| `Unit.cs` | Void type for handlers without return value |
| `MediatorServiceCollectionExtensions.cs` | DI registration helpers |

## Pipeline Behaviors

Located in `MealPlanner.Application/Common/Behaviors/`:

| Behavior | Purpose | Execution Order |
|----------|---------|-----------------|
| `LoggingBehavior` | Logs request start, completion, duration | First |
| `ValidationBehavior` | FluentValidation integration | Second |
| `TransactionBehavior` | EF Core transaction management | Third |

## Registration

In `Program.cs`:

```csharp
// Custom mediator with pipeline behaviors
builder.Services.AddMediator(applicationAssembly);
builder.Services.AddMediatorBehavior(typeof(LoggingBehavior<,>));
builder.Services.AddMediatorBehavior(typeof(ValidationBehavior<,>));
builder.Services.AddMediatorBehavior(typeof(TransactionBehavior<,>));
```

## Handler Migration Pattern

### Before (MediatR)

```csharp
using MediatR;

public sealed record GetDailyDigestQuery(DateOnly Date) : IRequest<DailyDigestDto>;

public sealed class GetDailyDigestQueryHandler : IRequestHandler<GetDailyDigestQuery, DailyDigestDto>
{
    public async Task<DailyDigestDto> Handle(GetDailyDigestQuery request, CancellationToken cancellationToken)
    {
        // implementation
    }
}
```

### After (Custom Mediator)

```csharp
using MealPlanner.Application.Common.Mediator;

public sealed record GetDailyDigestQuery(DateOnly Date) : IRequest<DailyDigestDto>;

public sealed class GetDailyDigestQueryHandler : IRequestHandler<GetDailyDigestQuery, DailyDigestDto>
{
    public async Task<DailyDigestDto> Handle(GetDailyDigestQuery request, CancellationToken cancellationToken)
    {
        // implementation unchanged
    }
}
```

## Key Differences

1. **Namespace Change**: Only the `using` statement changes from `MediatR` to `MealPlanner.Application.Common.Mediator`
2. **API Compatibility**: Handler signatures remain identical
3. **Pipeline Behaviors**: Same pattern, different namespace for `IPipelineBehavior<,>`
4. **Unit Type**: Use `MealPlanner.Application.Common.Mediator.Unit` instead of `MediatR.Unit`

## Testing

All existing handler tests continue to work without modification. The custom mediator has dedicated unit tests in:

- `MealPlanner.Application.Tests/Common/Mediator/MediatorTests.cs`
- `MealPlanner.Application.Tests/Common/Mediator/MediatorServiceCollectionExtensionsTests.cs`
- `MealPlanner.Application.Tests/Common/Behaviors/PipelineBehaviorTests.cs`

## Performance

The custom mediator uses:
- Reflection for handler resolution (cached by DI container)
- Generic type resolution for pipeline behaviors
- Async/await throughout

Performance is equivalent to MediatR as both use similar patterns.

## Future Enhancements

The custom mediator supports:
- `INotification` and `INotificationHandler` for domain events (not currently used)
- `ITransactionalRequest` marker for automatic transaction wrapping
