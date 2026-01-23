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
