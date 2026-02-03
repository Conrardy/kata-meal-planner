---
goal: Admin users management (US-024)
version: 1.0
date_created: 2026-01-31
last_updated: 2026-01-31
owner: AI Assistant
status: In Progress
tags: [feature, admin, security, frontend, backend]
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Implement admin-only user management with a minimal list and user creation form, backed by admin-protected API endpoints and JWT claim parsing for `IsAdmin`.

## 1. Requirements & Constraints

- **REQ-001**: Seeded admin users (Emmanuel, Gabrielle) remain admins and issue `IsAdmin` claim on login.
- **REQ-002**: Admin-only page at `/admin/users` with form fields `username` and `password`.
- **REQ-003**: Validation must enforce unique username and ASP.NET Identity password rules.
- **REQ-004**: Admin-only endpoint `POST /api/v1/admin/users`.
- **REQ-005**: Display success/error messages after submission.
- **REQ-006**: Display existing users list (minimal: `id`, `username`).
- **SEC-001**: Only admins can access admin routes and endpoints (HTTP 403 otherwise).
- **CON-001**: Follow existing Angular and .NET conventions in memory bank.
- **CON-002**: No extra features beyond minimal list and form.
- **GUD-001**: Use Tailwind utilities only; no inline styles.
- **PAT-001**: Minimal API route mapping in `Program.cs`.

## 2. Implementation Steps

### Implementation Phase 1

- GOAL-001: Backend admin endpoints and authorization

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Map `GET /api/v1/admin/users` and `POST /api/v1/admin/users` in `backend/src/Api/MealPlanner.Api/Program.cs`, enforcing `RequireAdmin` policy and returning RFC 9457 Problem Details on validation errors. |  |  |
| TASK-002 | Register admin application services/handlers in DI if missing (e.g., `AdminUserService` or handlers) in `backend/src/Api/MealPlanner.Api/Program.cs` or appropriate composition root. |  |  |
| TASK-003 | Add/extend backend tests in `backend/tests/Api.Tests` to verify 403 for non-admins, success for admins, and validation errors (duplicate username, weak password). |  |  |

### Implementation Phase 2

- GOAL-002: Frontend admin UI and guard with JWT claim parsing

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-004 | Parse JWT in `frontend/src/app/core/services/auth.service.ts` to derive `isAdmin` from `IsAdmin` claim; persist in auth state model in `frontend/src/app/core/models`. |  |  |
| TASK-005 | Add `adminGuard` in `frontend/src/app/core/guards` and protect `/admin/users` route in `frontend/src/app/app.routes.ts`; show Admin link in sidebar only when `isAdmin` is true. |  |  |
| TASK-006 | Create admin users feature component in `frontend/src/app/features/admin-users` with minimal list (id, username), form (username, password), and success/error messaging. |  |  |
| TASK-007 | Add frontend tests for admin guard and admin users page behavior in `frontend/src/app` tests using Vitest. |  |  |

## 3. Alternatives

- **ALT-001**: Use a backend “current user” endpoint to fetch roles instead of JWT parsing. Not chosen to minimize API changes.
- **ALT-002**: Add pagination and role management in admin list. Not chosen due to minimal scope requirement.

## 4. Dependencies

- **DEP-001**: Existing JWT `IsAdmin` claim issuance in backend auth service.
- **DEP-002**: Existing admin application services/handlers for create/list user.

## 5. Files

- **FILE-001**: `backend/src/Api/MealPlanner.Api/Program.cs` (admin endpoints and policy enforcement)
- **FILE-002**: `backend/src/Application/...` admin handlers/services (confirm registration)
- **FILE-003**: `backend/tests/Api.Tests/...` admin endpoint tests
- **FILE-004**: `frontend/src/app/core/services/auth.service.ts` (token parsing)
- **FILE-005**: `frontend/src/app/core/models/...` (auth state updates)
- **FILE-006**: `frontend/src/app/core/guards/admin.guard.ts` (new guard)
- **FILE-007**: `frontend/src/app/app.routes.ts` (admin route)
- **FILE-008**: `frontend/src/app/shared/components/sidebar/...` (admin link)
- **FILE-009**: `frontend/src/app/features/admin-users/...` (admin page)
- **FILE-010**: `frontend/src/app/**/**.spec.ts` (admin UI tests)

## 6. Testing

- **TEST-001**: Backend: admin endpoints return 403 for non-admin users.
- **TEST-002**: Backend: admin can create user; duplicate username/weak password returns validation errors.
- **TEST-003**: Frontend: `adminGuard` blocks non-admin navigation.
- **TEST-004**: Frontend: admin page renders user list and handles form success/error states.

## 7. Risks & Assumptions

- **RISK-001**: JWT claim name mismatch (`IsAdmin` vs lowercase) could block guard; mitigate with explicit claim mapping.
- **ASSUMPTION-001**: Admin application handlers/services already exist and only need wiring.

## 8. Related Specifications / Further Reading

- `prd.json` (US-024)
- `docs/memory-bank/frontend/CONVENTIONS.md`
- `docs/memory-bank/backend/CONVENTIONS.md`
- `docs/memory-bank/common/CODING_ASSERTIONS.md`

## Progress Tracking

**Overall Status:** In Progress - 70%

### Subtasks

| ID | Description | Status | Updated | Notes |
|----|-------------|--------|---------|-------|
| 1.1 | Add admin policy + endpoints | In Progress | 2026-01-31 | Admin routes added; needs verification/tests. |
| 1.2 | Wire admin services + auth updates | In Progress | 2026-01-31 | DI updated; token generation path adjusted. |
| 1.3 | Frontend auth admin flag | In Progress | 2026-01-31 | `isAdmin` stored from JWT. |
| 1.4 | Frontend guard + UI | Completed | 2026-01-31 | Admin route protected and sidebar link added. |
| 1.5 | Admin page + tests | Completed | 2026-01-31 | Admin users page and frontend tests added. |

## Progress Log

### 2026-01-31
- Added admin endpoints in [backend/src/Api/MealPlanner.Api/Program.cs](backend/src/Api/MealPlanner.Api/Program.cs) with `RequireAdmin` policy.
- Registered admin/user auth services and password hasher in [backend/src/Infrastructure/MealPlanner.Infrastructure/DependencyInjection.cs](backend/src/Infrastructure/MealPlanner.Infrastructure/DependencyInjection.cs).
- Extended auth state with `isAdmin` and JWT parsing in [frontend/src/app/core/services/auth.service.ts](frontend/src/app/core/services/auth.service.ts) and [frontend/src/app/core/models/auth.model.ts](frontend/src/app/core/models/auth.model.ts).
- Updated token generation to support authenticated users in [backend/src/Infrastructure/MealPlanner.Infrastructure/Auth/SeededUserAuthService.cs](backend/src/Infrastructure/MealPlanner.Infrastructure/Auth/SeededUserAuthService.cs) and [backend/src/Application/MealPlanner.Application/Auth/ISeededUserAuthService.cs](backend/src/Application/MealPlanner.Application/Auth/ISeededUserAuthService.cs).
- Added admin guard and route protection in [frontend/src/app/core/guards/admin.guard.ts](frontend/src/app/core/guards/admin.guard.ts) and [frontend/src/app/app.routes.ts](frontend/src/app/app.routes.ts).
- Added admin users UI, service, models, and tests in [frontend/src/app/features/admin-users/admin-users.component.ts](frontend/src/app/features/admin-users/admin-users.component.ts), [frontend/src/app/core/services/admin-user.service.ts](frontend/src/app/core/services/admin-user.service.ts), and [frontend/src/app/features/admin-users/admin-users.component.spec.ts](frontend/src/app/features/admin-users/admin-users.component.spec.ts).
