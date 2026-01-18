# Frontend Conventions (Angular)

## Project Structure

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

## State Management Patterns

- Signals for synchronous reactive state
- SignalStore for feature-level state slices
- `computed()` for derived state (pure functions)
- `effect()` sparingly, only for side effects at boundaries

## UI & Styling Patterns

- Component-scoped styles with Tailwind
- Design tokens via CSS custom properties
- Mobile-first responsive design

## Forms & Validation Patterns

- Typed form groups with `FormGroup<T>`
- Zod schemas shared with backend DTOs
- Validation at boundaries only
- Form state as immutable snapshots

## HTTP & API Communication Patterns

- Centralized error handling via interceptors
- Correlation ID injection via interceptor
- Retry with exponential backoff for transient failures
- Response caching with `TransferState` for SSR

## Testing Patterns

- Test behavior, not implementation
- No mocking of functional components
- Given-When-Then structure
- `data-testid` for E2E selectors

## Observability Patterns

- Structured logging to console in dev
- Trace context propagation via headers
- Performance marks for Core Web Vitals
- Error boundary components

## Key Principles Applied

### From DDD Rules
- Value Objects as immutable records
- Domain Events for decoupling

### From Craft Rules
- Small functions (< 20 lines)
- Tell Don't Ask
- Fail fast at boundaries
- Immutability by default

### From Functional Rules
- Pure functions for domain logic
- Declarative data transformations
- No shared mutable state

### From API Design Rules
- Contract-first API design
- Generate TypeScript client from OpenAPI spec
- Shared validation rules concept (not code)