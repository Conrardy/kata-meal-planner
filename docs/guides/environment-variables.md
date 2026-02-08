# Environment Variables

This document lists all environment variables required and optional for running MealPlanner in different environments.

## Backend (.NET)

### Required Variables

| Variable | Description | Example | Production Default |
|----------|-------------|---------|-------------------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string | `Host=db;Port=5432;Database=mealplanner;Username=user;Password=pass` | Must be set |
| `ConnectionStrings__Redis` | Redis connection string | `redis:6379` | Must be set |
| `Jwt__Secret` | JWT signing secret (min 32 characters) | `YourSecureSecretKeyHere123456789!` | Must be set |

### Optional Variables

| Variable | Description | Default | Production Default |
|----------|-------------|---------|-------------------|
| `ASPNETCORE_ENVIRONMENT` | Environment name | `Production` | `Production` |
| `ASPNETCORE_URLS` | Listen URLs | `http://+:8080` | `http://+:8080` |
| `Jwt__Issuer` | JWT issuer | `MealPlanner` | `MealPlanner` |
| `Jwt__Audience` | JWT audience | `MealPlannerApp` | `MealPlannerApp` |
| `Jwt__AccessTokenExpirationMinutes` | Access token TTL (minutes) | `15` | `15` |
| `Jwt__RefreshTokenExpirationDays` | Refresh token TTL (days) | `7` | `7` |
| `RateLimiting__DefaultPolicy__PermitLimit` | Default requests per window | `100` | `100` |
| `RateLimiting__DefaultPolicy__WindowInSeconds` | Default window duration | `60` | `60` |
| `RateLimiting__AuthPolicy__PermitLimit` | Auth requests per window | `10` | `10` |
| `RateLimiting__AuthPolicy__WindowInSeconds` | Auth window duration | `60` | `60` |
| `Cors__PolicyName` | CORS policy name | `MealPlannerCorsPolicy` | `MealPlannerCorsPolicy` |
| `Cors__AllowedOrigins__0` | First allowed origin | (empty) | Must be set for production |
| `Cors__AllowCredentials` | Allow credentials | `true` | `true` |
| `Cors__PreflightMaxAgeSeconds` | Preflight cache duration | `600` | `600` |

### Configuration Hierarchy

.NET configuration reads values in this order (later overrides earlier):
1. `appsettings.json`
2. `appsettings.{Environment}.json`
3. Environment variables
4. Command-line arguments

### Environment Variable Naming

For nested JSON configuration, use double underscore (`__`) as separator:

```bash
# JSON: { "ConnectionStrings": { "DefaultConnection": "..." } }
ConnectionStrings__DefaultConnection="Host=..."

# JSON: { "Jwt": { "Secret": "..." } }
Jwt__Secret="YourSecret"

# JSON: { "Cors": { "AllowedOrigins": ["http://example.com"] } }
Cors__AllowedOrigins__0="http://example.com"
Cors__AllowedOrigins__1="http://example2.com"
```

## Frontend (Angular)

### Build-Time Variables

| Variable | Description | Default | Production Default |
|----------|-------------|---------|-------------------|
| `API_URL` | Backend API base URL | `http://localhost:5000` | Must be set at build time |

### Configuration

Frontend uses Angular environment files:
- Development: `src/environments/environment.ts`
- Production: `src/environments/environment.prod.ts`

The production build replaces `environment.ts` with `environment.prod.ts`.

For production deployment, replace the placeholder `${API_URL}` in `environment.prod.ts` before building:

```bash
# Option 1: Replace in source before build
sed -i "s|\${API_URL}|https://api.yourdomain.com|g" src/environments/environment.prod.ts
npm run build

# Option 2: Use envsubst after build (requires nginx or runtime config)
npm run build
envsubst < dist/meal-planner/browser/main.*.js > dist/meal-planner/browser/main.*.js.tmp
mv dist/meal-planner/browser/main.*.js.tmp dist/meal-planner/browser/main.*.js
```

## Docker Compose

### Development (.env file)

```bash
# PostgreSQL
POSTGRES_USER=mealplanner
POSTGRES_PASSWORD=mealplanner_dev
POSTGRES_DB=mealplanner
POSTGRES_PORT=5432

# Redis
REDIS_PORT=6379

# API
API_PORT=5000
ASPNETCORE_ENVIRONMENT=Development

# Frontend
FRONTEND_PORT=4200

# JWT (Development only - DO NOT use in production)
JWT_SECRET=ThisIsASecretKeyThatShouldBeAtLeast32CharactersLong!
JWT_ISSUER=MealPlanner
JWT_AUDIENCE=MealPlannerApp
JWT_ACCESS_TOKEN_EXPIRATION_MINUTES=15
JWT_REFRESH_TOKEN_EXPIRATION_DAYS=7
```

### Production Docker Deployment

For production Docker deployments, pass environment variables directly:

```bash
docker run -d \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="Host=prod-db;Database=mealplanner;Username=user;Password=secure_pass" \
  -e ConnectionStrings__Redis="prod-redis:6379" \
  -e Jwt__Secret="$(openssl rand -base64 48)" \
  -e Cors__AllowedOrigins__0="https://yourdomain.com" \
  -p 8080:8080 \
  mealplanner-api:latest
```

## Security Best Practices

### Secrets Management

**DO NOT** commit secrets to source control:
- ❌ Never commit `.env` files with real credentials
- ❌ Never commit `appsettings.Production.json` with secrets
- ✅ Use environment variables for all secrets
- ✅ Use secret management tools (Azure Key Vault, AWS Secrets Manager, HashiCorp Vault)
- ✅ Rotate secrets regularly

### JWT Secret Generation

Generate a strong JWT secret:

```bash
# Linux/macOS
openssl rand -base64 48

# PowerShell
[Convert]::ToBase64String((1..48 | ForEach-Object { Get-Random -Maximum 256 }))

# Online (use with caution)
# https://generate-random.org/api-key-generator (48 characters minimum)
```

### Connection String Security

For production PostgreSQL:
- Use strong passwords (min 16 characters, mixed case, numbers, symbols)
- Enable SSL/TLS: `Host=db;Database=mealplanner;Username=user;Password=pass;SSL Mode=Require`
- Restrict database user permissions (principle of least privilege)
- Use connection pooling: `Host=db;Database=mealplanner;Username=user;Password=pass;Pooling=true;Minimum Pool Size=0;Maximum Pool Size=100`

## Validation

### Startup Validation

The application validates critical configuration at startup (see `Program.cs`):
- JWT settings (Secret, Issuer, Audience)
- Rate limiting settings
- CORS settings

Missing or invalid configuration will throw `InvalidOperationException` with descriptive messages.

### Health Checks

Verify configuration correctness:

```bash
# Liveness (app is running)
curl http://localhost:5000/health/live

# Readiness (app + dependencies ready)
curl http://localhost:5000/health/ready
```

## Troubleshooting

### Common Issues

**"JWT settings are missing or invalid"**
- Ensure `Jwt__Secret`, `Jwt__Issuer`, and `Jwt__Audience` are set
- Secret must be at least 32 characters

**"Database connection failed"**
- Verify PostgreSQL is running: `docker ps | grep postgres`
- Check connection string: `ConnectionStrings__DefaultConnection`
- Ensure database exists: `psql -h localhost -U mealplanner -d mealplanner`

**"CORS policy error in browser"**
- Add frontend origin to `Cors__AllowedOrigins__0`
- Verify CORS headers in response: `curl -H "Origin: http://localhost:4200" -v http://localhost:5000/health/live`

**"Rate limit exceeded (429)"**
- Increase `RateLimiting__DefaultPolicy__PermitLimit` or `WindowInSeconds`
- Check logs for rate limit violations
