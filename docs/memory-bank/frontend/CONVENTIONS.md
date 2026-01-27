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

## Environment Configuration

### Environment Files

| File | Purpose |
|------|---------|
| `src/environments/environment.ts` | Development configuration (default) |
| `src/environments/environment.prod.ts` | Production configuration |

### Environment Variables

| Variable | Type | Development | Production | Description |
|----------|------|-------------|------------|-------------|
| `production` | `boolean` | `false` | `true` | Indicates production mode |
| `apiUrl` | `string` | `http://localhost:5000` | Configurable via build | Base URL for API requests |

### ApiConfigService

Centralized service for API URL access:

```typescript
import { ApiConfigService } from './api-config.service';

// In your service
private readonly apiConfig = inject(ApiConfigService);
private readonly baseUrl = this.apiConfig.apiBaseUrl;

// Properties available:
// - apiConfig.apiUrl        // Raw API URL (e.g., http://localhost:5000)
// - apiConfig.apiBaseUrl    // API URL with version (e.g., http://localhost:5000/api/v1)
// - apiConfig.getEndpoint('/recipes')  // Helper to build full endpoints
```

### Build Commands

```bash
# Development (uses environment.ts)
npm start
npm run build -- --configuration development

# Production (uses environment.prod.ts)
npm run build
npm run build -- --configuration production
```

### Production Deployment

For production, set the `API_URL` placeholder in `environment.prod.ts` before building, or use a build-time environment variable replacement strategy.

## Routing & Lazy Loading

### Lazy Loading Strategy

All feature components are lazy loaded using `loadComponent()` for standalone components:

```typescript
// app.routes.ts
export const routes: Routes = [
  {
    path: 'feature',
    loadComponent: () =>
      import('./features/feature/feature.component').then(
        (m) => m.FeatureComponent
      ),
    title: 'Feature Title',
  },
];
```

### Preloading Strategy

The application uses `PreloadAllModules` strategy to preload lazy-loaded routes in the background after the initial bundle loads. This provides:
- Fast initial load (small main bundle)
- Instant navigation to other routes (preloaded in background)

```typescript
// app.config.ts
provideRouter(routes, withPreloading(PreloadAllModules))
```

### Named Chunks

Named chunks are enabled in `angular.json` for easier debugging:

```json
{
  "options": {
    "namedChunks": true
  }
}
```

This produces readable chunk names like `daily-digest.component.js` instead of hashed names.

## Bundle Analysis

### Using source-map-explorer

Analyze the production bundle to identify large dependencies and optimization opportunities:

```bash
# Install (already in devDependencies)
npm install --save-dev source-map-explorer

# Build with source maps and analyze
npm run build:analyze
```

This command:
1. Builds the production bundle with source maps
2. Opens an interactive treemap visualization of all bundles

### Key Metrics to Monitor

| Metric | Target | Description |
|--------|--------|-------------|
| Initial bundle | < 500kB | Main bundle loaded on first visit |
| Largest chunk | < 200kB | Any single lazy-loaded chunk |
| Total size | < 1MB | All chunks combined |

### Optimization Tips

1. **Review large dependencies** - Look for oversized libraries in the treemap
2. **Check for duplicates** - Same code appearing in multiple chunks
3. **Verify lazy loading** - Features should appear in separate chunks
4. **Monitor @angular/* size** - Core framework should be in main bundle only