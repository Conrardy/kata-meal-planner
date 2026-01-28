# Instruction: Authentication & Login System

## Feature

- **Summary**: Implement username/password authentication with seeded users (Emmanuel, Gabrielle), JWT token-based auth, login page, route protection, and logout functionality. All routes redirect to login when unauthenticated.
- **Stack**: `Angular 19`, `TypeScript 5.7+`, `.NET 9`, `C# 13`, `ASP.NET Identity`, `JWT Bearer`, `Tailwind CSS 3`, `Reactive Forms`, `Zod`, `FluentValidation`, `ErrorOr`
- **Branch name**: `feature/authentication-login-system`
- **dependencies**: None

## Existing files

- @frontend/src/app/core/services/auth.service.ts (existing JWT auth service)
- @frontend/src/app/core/models/auth.model.ts (existing auth models)
- @frontend/src/app/core/interceptors/auth.interceptor.ts (existing auth interceptor)
- @frontend/src/app/core/helpers/dev-auth.helper.ts (dev auto-login, will be adapted)
- @frontend/src/app/app.routes.ts (route configuration)
- @frontend/src/app/app.config.ts (providers configuration)
- @frontend/src/main.ts (app bootstrap with dev-auth call)
- @backend/src/Api/MealPlanner.Api/Program.cs (API configuration)
- @backend/src/Api/MealPlanner.Api/appsettings.json (JWT config)
- @backend/src/Api/MealPlanner.Api/appsettings.Development.json (dev JWT config)

## Test files to update or create

- @frontend/src/app/features/login/login.component.spec.ts
- @frontend/src/app/core/guards/auth.guard.spec.ts
- @backend/tests/Api.Tests/Auth/LoginEndpointTests.cs
- @backend/tests/Api.Tests/Auth/AuthGuardTests.cs

## New files to create

- frontend/src/app/features/login/login.component.ts
- frontend/src/app/features/login/login.component.html
- frontend/src/app/core/guards/auth.guard.ts
- backend/src/Application/Auth/Login/LoginCommand.cs
- backend/src/Application/Auth/Login/LoginCommandHandler.cs
- backend/src/Application/Auth/Login/LoginCommandValidator.cs
- backend/src/Application/Auth/Login/LoginDto.cs
- backend/src/Api/MealPlanner.Api/Endpoints/Auth/LoginEndpoint.cs
- backend/src/Infrastructure/MealPlanner.Infrastructure/Auth/SeededUserRepository.cs
- backend/src/Domain/MealPlanner.Domain/Auth/SeededUser.cs

## Implementation phases

### Phase 1: Backend Authentication Infrastructure

> Read @docs/memory-bank/backend/CONVENTIONS.md, @docs/memory-bank/STACK.md, @docs/rules/ddd.md, @docs/rules/api-design.md

> Setup JWT infrastructure, seeded user configuration, password validation endpoint

1. Create SeededUser domain entity in Domain layer
   1.1 Define Username, PasswordHash value objects with validation
   1.2 Commit: "feat(auth): add SeededUser domain entity"

2. Add seeded user configuration to appsettings
   2.1 Define SeededUsers section with Emmanuel and Gabrielle usernames
   2.2 Add SeededPassword config key (overridable by env variable)
   2.3 Commit: "feat(auth): add seeded users configuration"

3. Create SeededUserRepository in Infrastructure
   3.1 Implement ISeededUserRepository interface
   3.2 Load seeded users from configuration at startup
   3.3 Validate password against configured value
   3.4 Commit: "feat(auth): implement seeded user repository"

4. Create Login CQRS in Application layer
   4.1 LoginCommand with Username and Password
   4.2 LoginCommandHandler with password validation via repository
   4.3 LoginCommandValidator using FluentValidation
   4.4 Return JWT token on success using existing JWT service
   4.5 Return ErrorOr with appropriate errors
   4.6 Commit: "feat(auth): implement login command and handler"

5. Create LoginEndpoint in API layer
   5.1 POST /api/v1/auth/login endpoint
   5.2 Map domain errors to HTTP status codes (401 for invalid credentials)
   5.3 Problem Details response for errors
   5.4 Commit: "feat(auth): add login endpoint"

6. Update JWT configuration
   6.1 Ensure JWT_SECRET can be overridden by environment variable
   6.2 Update Program.cs to read from env vars
   6.3 Commit: "feat(auth): add JWT environment variable override"

### Phase 2: Frontend Login Page

> Read @docs/memory-bank/frontend/CONVENTIONS.md, @docs/memory-bank/frontend/DESIGN.md, @docs/memory-bank/frontend/FORMS.md, @docs/rules/craft.md

> Build login form component with username/password validation

1. Create LoginComponent feature
   1.1 Create login.component.ts with Reactive Forms
   1.2 Username field (text input)
   1.3 Password field (password input)
   1.4 Form validation (required fields)
   1.5 Commit: "feat(auth): create login component structure"

2. Design login UI with Tailwind
   2.1 Create login.component.html with form layout
   2.2 Center-aligned card design
   2.3 Error message display for invalid credentials
   2.4 Loading state during login
   2.5 Primary button for login action
   2.6 Commit: "feat(auth): design login page UI"

3. Integrate AuthService with LoginComponent
   3.1 Call AuthService.login() on form submit
   3.2 Store token in localStorage via AuthService
   3.3 Handle success and redirect to /
   3.4 Handle error and display generic error message
   3.5 Commit: "feat(auth): integrate login with auth service"

4. Update AuthService for username-based login
   4.1 Modify LoginRequest model to use username instead of email
   4.2 Update login endpoint path if needed
   4.3 Commit: "feat(auth): update auth service for username login"

### Phase 3: Route Protection

> Read @docs/memory-bank/frontend/CONVENTIONS.md, @docs/memory-bank/STACK.md

> Implement auth guard and protect all routes except login

1. Create AuthGuard
   1.1 Implement CanActivateFn guard
   1.2 Check AuthService.isAuthenticated signal
   1.3 Redirect to /login if not authenticated
   1.4 Allow access if authenticated
   1.5 Commit: "feat(auth): implement auth guard"

2. Apply AuthGuard to all routes
   2.1 Add canActivate to all existing routes in app.routes.ts
   2.2 Exclude /login route from guard
   2.3 Commit: "feat(auth): protect all routes with auth guard"

3. Add /login route
   3.1 Define /login route without guard
   3.2 Lazy load LoginComponent
   3.3 Set title to "Login"
   3.4 Commit: "feat(auth): add login route"

4. Handle token expiration
   4.1 Update auth.interceptor.ts to detect 401 responses
   4.2 Clear auth state and redirect to /login on 401
   4.3 Commit: "feat(auth): handle token expiration"

### Phase 4: Logout Functionality

> Read @docs/memory-bank/frontend/CONVENTIONS.md, @docs/memory-bank/frontend/DESIGN.md

> Add logout button and clear session state

1. Update sidebar with logout button
   1.1 Add logout button to @frontend/src/app/shared/components/sidebar
   1.2 Style with Tailwind (secondary button)
   1.3 Commit: "feat(auth): add logout button to sidebar"

2. Implement logout logic
   2.1 Call AuthService.logout() on button click
   2.2 Clear localStorage tokens
   2.3 Clear all cached state
   2.4 Redirect to /login
   2.5 Commit: "feat(auth): implement logout functionality"

3. Add logout confirmation (optional)
   3.1 Optional: Add confirmation dialog before logout
   3.2 Skip if user wants immediate logout
   3.3 Commit: "feat(auth): add logout confirmation" (if applicable)

### Phase 5: Development Mode Adaptation

> Read @docs/memory-bank/infra/DEPLOYMENT.md, @docs/memory-bank/frontend/CONVENTIONS.md

> Update dev-auth helper to work with new username-based login

1. Update autoLoginForDevelopment helper
   1.1 Remove auto-login call from main.ts (force real login)
   1.2 Or adapt to use username: "Emmanuel" for dev auto-login
   1.3 Update dev credentials to match seeded users
   1.4 Commit: "feat(auth): update dev auto-login for username auth"

2. Add development mode indicator
   2.1 Optional: Show "Development Mode" badge on login page
   2.2 Optional: Pre-fill username in development
   2.3 Commit: "feat(auth): add dev mode indicators" (if applicable)

### Phase 6: Testing & Validation

> Read @docs/memory-bank/common/TESTING.md, @docs/memory-bank/common/CODING_ASSERTIONS.md

> Comprehensive testing of authentication flow

1. Backend tests
   1.1 LoginEndpoint tests (valid credentials, invalid credentials, missing fields)
   1.2 SeededUserRepository tests (password validation, user lookup)
   1.3 LoginCommandHandler tests (business logic)
   1.4 Commit: "test(auth): add backend authentication tests"

2. Frontend tests
   2.1 LoginComponent tests (form validation, submit, error handling)
   2.2 AuthGuard tests (authenticated, unauthenticated, redirect)
   2.3 AuthService tests (login, logout, token storage)
   2.4 Commit: "test(auth): add frontend authentication tests"

3. Integration validation
   3.1 Manual test: login with Emmanuel
   3.2 Manual test: login with Gabrielle
   3.3 Manual test: invalid credentials
   3.4 Manual test: logout and re-login
   3.5 Manual test: direct navigation to protected route
   3.6 Commit: "test(auth): validate complete authentication flow"

## Reviewed implementation

- [ ] Phase 1: Backend Authentication Infrastructure
- [ ] Phase 2: Frontend Login Page
- [ ] Phase 3: Route Protection
- [ ] Phase 4: Logout Functionality
- [ ] Phase 5: Development Mode Adaptation
- [ ] Phase 6: Testing & Validation

## Validation flow

1. Start application (docker-compose up or npm start + dotnet run)
2. Navigate to http://localhost:4200
3. Should redirect to /login (not authenticated)
4. Enter username "Emmanuel" and configured password
5. Click "Login" button
6. Should redirect to / (daily digest)
7. Navigate to /recipes
8. Should display recipes (authenticated)
9. Click "Logout" in sidebar
10. Should redirect to /login
11. Attempt to navigate to /weekly-plan directly
12. Should redirect to /login (route protected)
13. Login with username "Gabrielle" and password
14. Should access all routes successfully
15. Test invalid credentials (generic error message displayed)

## Estimations

- **Confidence**: 9/10
  - ✅ Authentication infrastructure already exists (JWT, AuthService, interceptors)
  - ✅ Clear requirements and seeded user approach
  - ✅ Stack is aligned with existing codebase conventions
  - ✅ Route guard pattern is standard Angular practice
  - ✅ Memory bank provides comprehensive guidance for each layer
  - ❌ Minor risk: ensuring environment variable override works correctly in production
  
- **Time to implement**: 4-6 hours for AI
  - Phase 1 (Backend): 1.5 hours
  - Phase 2 (Frontend Login): 1 hour
  - Phase 3 (Route Protection): 0.5 hours
  - Phase 4 (Logout): 0.5 hours
  - Phase 5 (Dev Mode): 0.5 hours
  - Phase 6 (Testing): 1-2 hours
