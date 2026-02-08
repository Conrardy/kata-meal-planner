MealPlanner — Local Development Guide

[![CI](https://github.com/Conrardy/kata-meal-planner/actions/workflows/ci.yml/badge.svg)](https://github.com/Conrardy/kata-meal-planner/actions/workflows/ci.yml)

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

## Ralph-tui

Ralph-tui is a terminal user interface (TUI) application that serves as an orchestrator for AI agent loops. It allows users to create Product Requirement Documents (PRDs), run AI agents, and manage tasks through a command-line interface.

### Setup

Follow installation instructions at https://ralph-tui.com/docs/getting-started/installation

### Usage 

Ralph TUI - AI Agent Loop Orchestrator

Usage: ralph-tui [command] [options]

Commands:
  (none)              Start Ralph execution (same as 'run')
  create-prd [opts]   Create a new PRD interactively (alias: prime)
  convert [options]   Convert PRD markdown to JSON format
  run [options]       Start Ralph execution
  resume [options]    Resume an interrupted session
  status [options]    Check session status (headless, for CI/scripts)
  logs [options]      View/manage iteration output logs
  setup [options]     Run interactive project setup (alias: init)
  config show         Display merged configuration
  template show       Display current prompt template
  template init       Copy default template for customization
  plugins agents      List available agent plugins
  plugins trackers    List available tracker plugins
  docs [section]      Open documentation in browser
  help, --help, -h    Show this help message
  version, --version, -v  Show version number

Run Options:
  --epic <id>         Epic ID for beads tracker
  --prd <path>        PRD file path (auto-switches to json tracker)
  --agent <name>      Override agent plugin (e.g., claude, opencode)
  --model <name>      Override model (e.g., opus, sonnet)
  --tracker <name>    Override tracker plugin (e.g., beads, beads-bv, json)
  --iterations <n>    Maximum iterations (0 = unlimited)
  --resume            Resume existing session (deprecated, use 'resume' command)
  --headless          Run without TUI (alias: --no-tui)
  --no-tui            Run without TUI, output structured logs to stdout
  --no-setup          Skip interactive setup even if no config exists
  --notify            Force enable desktop notifications
  --no-notify         Force disable desktop notifications

Resume Options:
  --cwd <path>        Working directory
  --headless          Run without TUI
  --force             Override stale lock

Status Options:
  --json              Output in JSON format for CI/scripts
  --cwd <path>        Working directory

Convert Options:
  --to <format>       Target format: json
  --output, -o <path> Output file path (default: ./prd.json)
  --branch, -b <name> Git branch name (prompts if not provided)
  --force, -f         Overwrite existing files

Examples:
  ralph-tui                              # Start execution (same as 'run')
  ralph-tui create-prd                   # Create a new PRD interactively
  ralph-tui create-prd --chat            # Create PRD with AI chat mode
  ralph-tui convert --to json ./prd.md   # Convert PRD to JSON
  ralph-tui run                          # Start execution with defaults
  ralph-tui run --epic myproject-epic    # Run with specific epic
  ralph-tui run --prd ./prd.json         # Run with PRD file
  ralph-tui resume                       # Resume interrupted session
  ralph-tui status                       # Check session status
  ralph-tui status --json                # JSON output for CI/scripts
  ralph-tui logs                         # List iteration logs
  ralph-tui logs --iteration 5           # View specific iteration
  ralph-tui logs --task US-005           # View logs for a task
  ralph-tui logs --clean --keep 10       # Clean up old logs
  ralph-tui plugins agents               # List agent plugins
  ralph-tui plugins trackers             # List tracker plugins
  ralph-tui template show                # Show current prompt template
  ralph-tui template init                # Create custom template
  ralph-tui docs                         # Open documentation in browser
  ralph-tui docs quickstart              # Open quick start guide