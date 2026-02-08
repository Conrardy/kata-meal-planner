---
name: testing
description: Cross-cutting testing strategy (frontend + backend)
---

# Testing

## Tools and Frameworks

- Backend: xUnit + FluentAssertions + Microsoft.NET.Test.Sdk + coverlet.collector (test projects under @backend/tests).
- Frontend: Vitest + @analogjs/vitest-angular (config in @frontend/vite.config.mts, scripts in @frontend/package.json).

## Test Coverage

- Frontend coverage thresholds are defined in @frontend/vite.config.mts (feature-level thresholds under src/app/features).
- Backend coverage collection is enabled via coverlet.collector in test project files (see @backend/tests/**/**.csproj).

## Testing Strategy

- Unit tests for domain/application logic; avoid implementation-coupled assertions.
- Frontend tests focus on component/service behavior; Angular test setup in @frontend/src/test-setup.ts.
- Integration/E2E: not explicitly configured in repo; add only when required by a feature.
- Module-specific testing patterns live in @docs/memory-bank/backend/CONVENTIONS.md and @docs/memory-bank/frontend/CONVENTIONS.md.

## Test Execution Process

- Backend: `dotnet test` from repo root (targets solution tests).
- Frontend: `npm run test` (or `npm run test:coverage`) from @frontend.
- CI hooks are not defined here; keep execution steps aligned with @docs/memory-bank/common/CODING_ASSERTIONS.md.

## Mocking and Stubbing

- Prefer minimal mocking; use framework-provided testing utilities (e.g., Angular HTTP testing) as documented in module conventions.
- Do not mock functional components; follow Given-When-Then/Arrange-Act-Assert per module docs.

## Documentation Synchronization Tests

Tests in `@backend/tests/Api/MealPlanner.Api.Tests/Documentation/` verify that API endpoints match documentation.

### Pattern: EndpointSynchronizationTests

| Test | Purpose |
|------|---------|
| `AllDocumentedEndpoints_ShouldExistInCode` | Fails if docs/api/endpoints.md lists endpoints not in Program.cs |
| `AllCodeEndpoints_ShouldBeDocumented` | Fails if Program.cs has endpoints missing from documentation |
| `Documentation_ShouldBeSynchronizedWithCode` | Combined check for full sync |
| `GenerateSkeletonForUndocumentedEndpoints` | Helper to generate doc templates for new endpoints |

### Components

| File | Purpose |
|------|---------|
| `EndpointDocumentationParser.cs` | Parses endpoints from docs/api/endpoints.md |
| `EndpointCodeExtractor.cs` | Extracts endpoint definitions from Program.cs |
| `EndpointSynchronizer.cs` | Compares documented vs. code endpoints |
| `EndpointSynchronizationReport.cs` | Report with sync status and discrepancies |
| `DocumentationSkeletonGenerator.cs` | Generates markdown templates for undocumented endpoints |

### Usage

```bash
# Run sync tests
dotnet test --filter "FullyQualifiedName~EndpointSynchronizationTests"

# View generated skeleton for new endpoints
dotnet test --filter "GenerateSkeletonForUndocumentedEndpoints" -- --logger "console;verbosity=detailed"
```
