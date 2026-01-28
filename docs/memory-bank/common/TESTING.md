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
