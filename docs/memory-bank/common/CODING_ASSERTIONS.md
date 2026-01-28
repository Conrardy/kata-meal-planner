# Coding Guidelines

> Those rules must be minimal because they MUST be checked after EVERY CODE GENERATION.

## Requirements to complete a feature

**A feature is really completed if ALL of the above are satisfied: if not, iterate to fix all until all are green.**

## Steps to follow

1. Check there is no duplication
2. Ensure code is re-used
3. Run all those commands, in order to ensure code is perfect:

| Order | Command | Description |
|-------|---------|-------------|
| 1 | `dotnet build` (Backend) | Build all backend projects |
| 2 | `dotnet test` (Backend) | Run all backend tests (xUnit + FluentAssertions) |
| 3 | `npm run build` (Frontend) | Build frontend with Angular CLI |
| 4 | `npm run test` (Frontend) | Run frontend tests (Vitest) |
| 5 | `npm run lint` (Frontend) | Lint TypeScript code |
| 6 | `npm run format` (Frontend) | Format code with Prettier |

## Backend (.NET) Assertions

### Domain Layer
- Value Objects as `record` types with private constructors
- Entities with private setters, public behavior methods
- No infrastructure dependencies in domain
- Domain logic throws meaningful exceptions early

### Application Layer (CQRS)
- Commands return `ErrorOr<T>`, never throw for business logic
- Queries return DTOs directly
- One handler per use case
- FluentValidation for input validation at boundaries

### API Layer
- REPR pattern (Request-Endpoint-Response)
- Problem Details (RFC 9457) for errors
- Correlation ID in all requests/responses
- Versioned endpoints (`/api/v1/`)

### Testing
- Arrange-Act-Assert structure
- FluentAssertions for readable expectations
- No mocking of domain models
- Integration tests with Testcontainers for real DB
- Test behavior, not implementation details

## Frontend (Angular) Assertions

### Component Architecture
- Standalone components with explicit imports
- Signal-based reactivity for local state
- NgRx SignalStore for shared state
- Dependency injection via `inject()` function

### Services & Data
- Services in `core/services/` use HttpClient
- Models in `core/models/` match backend DTOs
- No business logic in components
- Observable streams for async operations

### Styling & UI
- Tailwind CSS utility classes only
- No inline styles
- Mobile-first responsive design
- Accessibility attributes (`aria-*`, semantic HTML)

### Testing
- Vitest + @analogjs/vitest-angular
- No mocking of functional components
- Test behavior from user perspective
- HttpClientTesting for HTTP mocks

## Cross-Cutting Assertions

### Naming
- PascalCase for classes, types, records (C#)
- camelCase for variables, methods (C# & TS)
- SCREAMING_SNAKE_CASE for constants
- Descriptive names (no abbreviations except common ones: `Id`, `Dto`, `Url`)

### Error Handling
- Fail fast at boundaries
- No silent failures (log or throw)
- Domain-specific error types
- Never return `null` collections (return empty)

### Immutability
- Prefer `readonly` (C#) and `readonly` (TS)
- Value Objects immutable by design
- DTOs as `record` (C#) or `interface` (TS)

### Code Quality
- Functions < 20 lines
- Max 3 parameters per method
- One responsibility per class/function
- No commented-out code in commits
