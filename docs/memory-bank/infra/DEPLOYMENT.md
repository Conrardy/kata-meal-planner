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

Not configured for application deployment.

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
