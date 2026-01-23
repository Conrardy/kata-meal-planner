# Deployment

## Local Development

### Frontend

- **Command**: `npm start`
- **Port**: 4200 (Angular default)
- **Build**: `npm run build`
- **Test**: `npm test`
- **Config**: @frontend/angular.json

### Backend

- **Command**: `dotnet run --project backend/src/Api/MealPlanner.Api`
- **Ports**: HTTP 5000, HTTPS 5001
- **Config**: @backend/src/Api/MealPlanner.Api/Properties/launchSettings.json

## Environment Variables

### Backend

| File | Purpose |
|------|---------|
| @backend/src/Api/MealPlanner.Api/appsettings.json | Production settings |
| @backend/src/Api/MealPlanner.Api/appsettings.Development.json | Development settings |

### Frontend

- Environment files not yet configured
- Uses Angular default configuration

## CI/CD Pipeline

Not configured.

## Containerization

### Docker Compose

Docker Compose is the recommended way to run the full application stack locally.

#### Configuration Files

| File | Purpose |
|------|---------|
| `docker-compose.yml` | Main configuration for all services |
| `docker-compose.override.yml` | Development-specific overrides (auto-loaded) |
| `.env` | Environment variables (copy from `.env.example`) |
| `.env.example` | Template for environment variables |

#### Services

| Service | Image/Build | Port | Description |
|---------|-------------|------|-------------|
| `api` | `./backend` | 5000 | .NET 9 Backend API |
| `frontend` | `./frontend` | 4200 | Angular 19 Frontend |
| `postgres` | `postgres:17-alpine` | 5432 | PostgreSQL Database |
| `redis` | `redis:7-alpine` | 6379 | Redis Cache |

#### Quick Start

```bash
# 1. Copy environment file
cp .env.example .env

# 2. Start all services (development mode)
docker-compose up -d

# 3. View logs
docker-compose logs -f

# 4. Stop all services
docker-compose down
```

#### Common Commands

```bash
# Start all services in background
docker-compose up -d

# Start specific service
docker-compose up -d postgres redis

# Rebuild and start (after code changes)
docker-compose up -d --build

# View service logs
docker-compose logs -f api
docker-compose logs -f frontend

# Check service status
docker-compose ps

# Stop all services
docker-compose down

# Stop and remove volumes (clean slate)
docker-compose down -v

# Production mode (without override file)
docker-compose -f docker-compose.yml up -d
```

#### Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `POSTGRES_USER` | `mealplanner` | PostgreSQL username |
| `POSTGRES_PASSWORD` | `mealplanner_dev` | PostgreSQL password |
| `POSTGRES_DB` | `mealplanner` | PostgreSQL database name |
| `POSTGRES_PORT` | `5432` | PostgreSQL exposed port |
| `REDIS_PORT` | `6379` | Redis exposed port |
| `API_PORT` | `5000` | Backend API exposed port |
| `FRONTEND_PORT` | `4200` | Frontend exposed port |
| `ASPNETCORE_ENVIRONMENT` | `Development` | .NET environment |
| `JWT_SECRET` | - | JWT signing secret (required) |
| `JWT_ISSUER` | `MealPlanner` | JWT issuer |
| `JWT_AUDIENCE` | `MealPlannerApp` | JWT audience |
| `JWT_ACCESS_TOKEN_EXPIRATION_MINUTES` | `15` | Access token TTL |
| `JWT_REFRESH_TOKEN_EXPIRATION_DAYS` | `7` | Refresh token TTL |

#### Volumes

| Volume | Purpose |
|--------|---------|
| `mealplanner-postgres-data` | PostgreSQL data persistence |
| `mealplanner-redis-data` | Redis data persistence |

#### Networks

| Network | Purpose |
|---------|---------|
| `mealplanner-network` | Bridge network isolating all services |

#### Health Checks

All services include health checks:

| Service | Endpoint | Interval |
|---------|----------|----------|
| `api` | `/health/live` | 30s |
| `frontend` | `/` | 30s |
| `postgres` | `pg_isready` | 10s |
| `redis` | `redis-cli ping` | 10s |

### Docker Images

#### Backend

**Dockerfile**: `backend/Dockerfile`

**Build Commands**:
```bash
# Development build
cd backend
docker build -t mealplanner-api:dev .

# Production build
docker build -t mealplanner-api:latest --build-arg CONFIGURATION=Release .

# Build with specific version tag
docker build -t mealplanner-api:v1.0.0 --build-arg CONFIGURATION=Release .
```

**Build Arguments**:
| Argument | Default | Description |
|----------|---------|-------------|
| `CONFIGURATION` | `Release` | Build configuration (Debug/Release) |

**Runtime Configuration**:
| Environment Variable | Description |
|---------------------|-------------|
| `ASPNETCORE_ENVIRONMENT` | Environment name (Development/Staging/Production) |
| `ASPNETCORE_URLS` | Listen URLs (default: http://+:8080) |
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string |
| `ConnectionStrings__Redis` | Redis connection string |
| `Jwt__Secret` | JWT signing secret |

**Run Command**:
```bash
docker run -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e "ConnectionStrings__DefaultConnection=Host=db;Database=mealplanner;..." \
  mealplanner-api:latest
```

#### Frontend

**Dockerfile**: `frontend/Dockerfile`

**Build Commands**:
```bash
# Development build
cd frontend
docker build -t mealplanner-frontend:dev --build-arg CONFIGURATION=development .

# Production build
docker build -t mealplanner-frontend:latest .

# Build with specific version tag
docker build -t mealplanner-frontend:v1.0.0 --build-arg CONFIGURATION=production .
```

**Build Arguments**:
| Argument | Default | Description |
|----------|---------|-------------|
| `CONFIGURATION` | `production` | Angular build configuration |

**Run Command**:
```bash
docker run -p 8080:8080 mealplanner-frontend:latest
```

### Image Specifications

| Image | Base | Target Size | Port |
|-------|------|-------------|------|
| mealplanner-api | mcr.microsoft.com/dotnet/aspnet:9.0-alpine | < 200MB | 8080 |
| mealplanner-frontend | nginx:alpine | < 50MB | 8080 |

### Dev Container

Dev container available for AIDD tooling:
- @aidd/supports/.devcontainer/Dockerfile

## Scripts

| Script | Purpose |
|--------|---------|
| @manage-symlinks.sh | Symlink management (Unix) |
| @manage-symlinks.ps1 | Symlink management (Windows) |

## URLs

| Environment | Frontend | Backend |
|-------------|----------|---------|
| Development | http://localhost:4200 | http://localhost:5000 |
| Docker Compose | http://localhost:4200 | http://localhost:5000 |
