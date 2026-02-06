# Architecture des flux API

Cette page explique le flux d'une requête depuis le frontend Angular jusqu'à la base de données, en passant par l'API .NET.

## Vue d'ensemble

```mermaid
graph LR
  Browser["Browser"] --> Angular["Angular 19<br/>HttpClient"]
  Angular --> Nginx["nginx<br/>(Docker)"]
  Nginx --> API[".NET 9 API<br/>Minimal API"]
  API --> Mediator["Custom Mediator<br/>Pipeline"]
  Mediator --> Handler["Handler<br/>(CQRS)"]
  Handler --> Infra["Infrastructure<br/>EF Core"]
  Infra --> DB[("PostgreSQL")]
```

---

## Flux détaillé d'une requête

### 1. Frontend (Angular)

L'utilisateur interagit avec un composant Angular qui appelle un service du dossier `core/services/`.

```
frontend/src/app/
├── features/daily-digest/         # Composant UI
└── core/services/
    └── daily-digest.service.ts    # Appel HTTP
```

Le service utilise `HttpClient` pour envoyer la requête :

```typescript
// daily-digest.service.ts
getDailyDigest(date: string): Observable<DailyDigest> {
  return this.http.get<DailyDigest>(`${this.baseUrl}/daily-digest/${date}`);
}
```

**Intercepteurs** appliqués automatiquement :

- **Auth Interceptor** : Ajoute le header `Authorization: Bearer <token>`
- **Correlation ID Interceptor** : Ajoute le header `X-Correlation-ID`
- **Accept-Language Interceptor** : Ajoute la langue courante

### 2. API Layer (.NET Minimal API)

La requête arrive dans `Program.cs` où les endpoints sont définis :

```
backend/src/Api/MealPlanner.Api/
└── Program.cs                     # Endpoints + middleware
```

```csharp
app.MapGet("/api/v1/daily-digest/{date}", async (DateOnly date, IMediator mediator) =>
{
    var query = new GetDailyDigestQuery(date);
    var result = await mediator.Send(query);
    return Results.Ok(result);
})
.RequireAuthorization();
```

**Middleware pipeline** (ordre d'exécution) :

```mermaid
graph TD
  A["Correlation ID Middleware"] --> B["Request Localization"]
  B --> C["Serilog Request Logging"]
  C --> D["Exception Handler"]
  D --> E["CORS"]
  E --> F["HSTS (production)"]
  F --> G["HTTPS Redirection"]
  G --> H["Rate Limiter"]
  H --> I["Authentication (JWT)"]
  I --> J["Authorization"]
  J --> K["Endpoint Handler"]
```

### 3. Mediator + Pipeline Behaviors

Le `IMediator` dispatche la requête vers le handler correspondant. Avant d'atteindre le handler, la requête traverse les **pipeline behaviors** :

```mermaid
graph LR
  Request["Query/Command"] --> L["LoggingBehavior"]
  L --> V["ValidationBehavior"]
  V --> T["TransactionBehavior"]
  T --> H["Handler"]
```

| Behavior | Rôle |
|----------|------|
| `LoggingBehavior` | Journalise la requête, la durée d'exécution et le résultat |
| `ValidationBehavior` | Valide la requête avec FluentValidation, retourne `400` si invalide |
| `TransactionBehavior` | Encadre les Commands dans une transaction EF Core |

```
backend/src/Application/MealPlanner.Application/
├── Common/
│   ├── Mediator/                  # IMediator, IRequest, IRequestHandler
│   └── Behaviors/                 # Pipeline behaviors
└── DailyDigest/
    ├── GetDailyDigestQuery.cs     # Query definition
    └── GetDailyDigestHandler.cs   # Handler implementation
```

### 4. Application Layer (Handlers)

Le handler exécute la logique métier en utilisant les interfaces du domaine :

```csharp
// GetDailyDigestHandler.cs
public sealed class GetDailyDigestHandler
    : IRequestHandler<GetDailyDigestQuery, DailyDigestDto>
{
    public async Task<DailyDigestDto> Handle(
        GetDailyDigestQuery query, CancellationToken ct)
    {
        var meals = await _plannedMealRepository.GetByDateAsync(query.Date, ct);
        return MapToDto(meals);
    }
}
```

**Pattern CQRS** :

| Type | Interface | Retour | Usage |
|------|-----------|--------|-------|
| Query | `IRequest<TResponse>` | DTO directement | Lecture seule |
| Command | `IRequest<ErrorOr<TResponse>>` | Result monad | Mutation avec gestion d'erreur |

### 5. Infrastructure Layer

Les repositories implémentent les interfaces définies dans le domaine :

```
backend/src/Infrastructure/MealPlanner.Infrastructure/
└── Persistence/
    ├── MealPlannerDbContext.cs     # EF Core DbContext
    └── Repositories/              # Repository implementations
```

```mermaid
graph TD
  Handler --> Repo["Repository<br/>(Interface: Domain)"]
  Repo --> EF["EF Core<br/>DbContext"]
  EF --> PG[("PostgreSQL")]
```

---

## Flux par type d'opération

### Lecture (Query)

```mermaid
sequenceDiagram
  participant FE as Frontend
  participant API as API Endpoint
  participant Med as Mediator
  participant H as Handler
  participant DB as Database

  FE->>API: GET /api/v1/daily-digest/2026-02-05
  API->>Med: Send(GetDailyDigestQuery)
  Med->>Med: LoggingBehavior
  Med->>Med: ValidationBehavior
  Med->>H: Handle(query)
  H->>DB: SELECT planned_meals WHERE date = ...
  DB-->>H: PlannedMeal[]
  H-->>Med: DailyDigestDto
  Med-->>API: DailyDigestDto
  API-->>FE: 200 OK + JSON
```

### Écriture (Command)

```mermaid
sequenceDiagram
  participant FE as Frontend
  participant API as API Endpoint
  participant Med as Mediator
  participant H as Handler
  participant DB as Database

  FE->>API: POST /api/v1/meals/{id}/swap
  API->>Med: Send(SwapMealCommand)
  Med->>Med: LoggingBehavior
  Med->>Med: ValidationBehavior
  Med->>Med: TransactionBehavior (BEGIN)
  Med->>H: Handle(command)
  H->>DB: UPDATE planned_meals SET recipe_id = ...
  DB-->>H: OK
  H-->>Med: ErrorOr<SwapMealResultDto>
  Med->>Med: TransactionBehavior (COMMIT)
  Med-->>API: SwapMealResultDto
  API-->>FE: 200 OK + JSON
```

### Erreur de validation

```mermaid
sequenceDiagram
  participant FE as Frontend
  participant API as API Endpoint
  participant Med as Mediator
  participant V as ValidationBehavior

  FE->>API: POST /api/v1/auth/login
  API->>Med: Send(LoginCommand)
  Med->>Med: LoggingBehavior
  Med->>V: ValidationBehavior
  V-->>Med: ValidationException (password empty)
  Med-->>API: ErrorOr.Validation
  API-->>FE: 400 Bad Request + Problem Details
```

---

## Organisation du code par feature

Chaque fonctionnalité suit la même structure dans les deux couches :

```
Frontend                              Backend
────────                              ───────
features/daily-digest/                Application/DailyDigest/
├── daily-digest.component.ts         ├── GetDailyDigestQuery.cs
├── daily-digest.component.html       ├── GetDailyDigestHandler.cs
└── daily-digest.component.spec.ts    └── DailyDigestDto.cs

core/services/
└── daily-digest.service.ts           Domain/Meals/
    (HttpClient → /api/v1/...)        └── IPlannedMealRepository.cs
```

---

## Gestion des erreurs à travers les couches

```mermaid
graph TD
  Domain["Domain Error<br/>(ErrorOr)"] --> App["Application<br/>Handler returns ErrorOr&lt;T&gt;"]
  App --> API["API Layer<br/>MatchResult()"]
  API --> PD["Problem Details<br/>(RFC 9457)"]
  PD --> FE["Frontend<br/>Error Interceptor"]
  FE --> UI["User Notification"]
```

| Couche | Mécanisme |
|--------|-----------|
| Domain | `Error.NotFound()`, `Error.Conflict()`, `Error.Validation()` |
| Application | `ErrorOr<T>` comme type de retour |
| API | `MatchResult()` extension convertit en HTTP status |
| Frontend | Intercepteur HTTP global + messages localisés |
