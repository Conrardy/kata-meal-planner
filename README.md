MealPlanner — Local Development Guide

This repo contains an Angular frontend and a .NET 9 Web API backend. Follow the steps below to run both locally on Windows.

Prerequisites
- Node.js: Node 20+ (LTS recommended). Install from nodejs.org or via nvm-windows.
- .NET SDK: .NET 9 SDK. Install from dotnet.microsoft.com.
- Git (optional): For source control.

Quick Start
Run backend and frontend in two terminals.

1) Start the API (port 5000/5001)

```powershell
# From the repo root
dotnet restore .\backend\MealPlanner.sln
dotnet run --project .\backend\src\Api\MealPlanner.Api
# The API listens on http://localhost:5000 (and https://localhost:5001)
```

Verify the API is up:

```powershell
curl http://localhost:5000/api/v1/daily-digest/2026-01-15
```

2) Start the frontend (port 4200)

```powershell
# From the repo root
cd .\frontend
npm ci
npm start
# Open http://localhost:4200
```

Details

- Frontend: Angular 19 app in [frontend/](frontend). Dev server runs on http://localhost:4200 by default. See [frontend/package.json](frontend/package.json) for scripts.
- Backend: ASP.NET Core minimal API in [backend/src/Api/MealPlanner.Api](backend/src/Api/MealPlanner.Api). Launch profiles expose http://localhost:5000 and https://localhost:5001 (see [launchSettings.json](backend/src/Api/MealPlanner.Api/Properties/launchSettings.json)).
- CORS: API allows origin http://localhost:4200 (configured in [Program.cs](backend/src/Api/MealPlanner.Api/Program.cs)).
- API base URL in frontend: hardcoded to http://localhost:5000/api/v1 (see [daily-digest.service.ts](frontend/src/app/core/services/daily-digest.service.ts)). Prefer using HTTP (5000) to avoid local HTTPS certificate prompts during dev.

Running Tests

- Backend tests:

```powershell
# From the repo root
dotnet test .\backend\MealPlanner.sln
```

- Frontend tests:

```powershell
cd .\frontend
npm test
```

Troubleshooting
- Port in use: If 5000/5001 or 4200 are occupied, stop the other service or change the port (Angular: `ng serve --port 4300`; API: adjust applicationUrl in launchSettings or use `ASPNETCORE_URLS`). If you change the API port, update the frontend base URL in the service.
- HTTPS warnings: Use http://localhost:5000 during development to avoid certificate prompts, or trust the dev cert: `dotnet dev-certs https --trust`.
- Node version mismatch: Ensure Node 20+ for Angular 19.

Local DB initialization

- Use the included Docker Compose to run PostgreSQL locally. From the repo root:

```powershell
docker compose up -d postgres
```

- The `docker-compose.yml` defines:
	- `POSTGRES_USER`: mealplanner
	- `POSTGRES_PASSWORD`: mealplanner_dev
	- `POSTGRES_DB`: mealplanner

- If you previously ran a different Postgres version and the container fails to start with "database files are incompatible", remove the named volume to allow fresh initialization (this deletes local DB data):

```powershell
docker compose down -v
docker compose up -d postgres
```

- To inspect the database or create the DB/role manually inside the container:

```powershell
# open a psql shell as the mealplanner user
docker exec -it mealplanner-postgres psql -U mealplanner -d mealplanner

# or run SQL directly from the host (example creates DB and user if needed)
docker exec -it mealplanner-postgres psql -U postgres -c "CREATE ROLE mealplanner WITH LOGIN PASSWORD 'mealplanner_dev';"
docker exec -it mealplanner-postgres psql -U postgres -c "CREATE DATABASE mealplanner OWNER mealplanner;"
```

- After the DB is reachable with the credentials above, run the API. On first run the app will apply EF Core migrations and seed data in the `Development` environment.

Project Structure
- Frontend: [frontend/](frontend)
- Backend solution: [backend/MealPlanner.sln](backend/MealPlanner.sln)
- API project: [backend/src/Api/MealPlanner.Api](backend/src/Api/MealPlanner.Api)
