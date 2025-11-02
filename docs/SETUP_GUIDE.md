# Project Setup Guide: Meal Planner

## Prerequisites

### Required Tools

- **Node.js**: v20.x LTS ([Download](https://nodejs.org/))
  - Verify: `node --version`
  - Includes npm package manager

- **.NET SDK**: v8.0+ ([Download](https://dotnet.microsoft.com/download))
  - Verify: `dotnet --version`
  - Required for ASP.NET Core backend development

- **Git**: Latest version ([Download](https://git-scm.com/downloads))
  - Verify: `git --version`

- **Docker Desktop**: Latest stable ([Download](https://www.docker.com/products/docker-desktop))
  - Required for PostgreSQL database and Testcontainers integration tests
  - Verify: `docker --version` and `docker compose version`

### Optional Tools

- **Visual Studio Code**: Recommended IDE ([Download](https://code.visualstudio.com/))
  - Extensions: Angular Language Service, C# Dev Kit, Tailwind CSS IntelliSense

- **Visual Studio 2022**: Alternative IDE for .NET development ([Download](https://visualstudio.microsoft.com/))

- **Postman/Insomnia**: API testing tool (optional)

## Installation

### 1. Clone Repository

```bash
git clone https://github.com/Conrardy/kata-meal-planner.git
cd kata-meal-planner
```

### 2. Frontend Setup (Angular 20 + Tailwind CSS)

Install Angular CLI globally:

```bash
npm install -g @angular/cli@20
```

Verify installation:

```bash
ng version
```

Create Angular project (from project root):

```bash
ng new frontend --routing --style=css --standalone --skip-git
```

Project options:

- **Routing**: Enabled (for navigation between pages)
- **Stylesheet**: CSS (we'll use Tailwind CSS)
- **Standalone**: Yes (Angular 20 default, standalone components)
- **Git**: Skipped (project already in git repository)

Navigate to frontend directory:

```bash
cd frontend
```

Install Tailwind CSS and dependencies:

```bash
npm install -D tailwindcss@latest postcss autoprefixer
npx tailwindcss init
```

### 3. Backend Setup (ASP.NET Core 8)

Create ASP.NET Core Web API project (from project root):

```bash
dotnet new webapi -n backend -f net8.0 --use-controllers --no-https
```

Navigate to backend directory:

```bash
cd backend
```

Restore NuGet packages:

```bash
dotnet restore
```

Install PostgreSQL and Entity Framework Core packages:

```bash
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 8.0.0
dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.0
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 8.0.0
```

Install Testcontainers and testing packages:

```bash
dotnet add package Testcontainers.PostgreSql --version 3.10.0
dotnet add package xunit --version 2.8.0
dotnet add package xunit.runner.visualstudio --version 2.8.0
dotnet add package Microsoft.AspNetCore.Mvc.Testing --version 8.0.0
```

Create test project:

```bash
cd ..
dotnet new xunit -n backend.Tests -f net8.0
cd backend.Tests
dotnet add reference ../backend/backend.csproj
dotnet add package Testcontainers.PostgreSql --version 3.10.0
dotnet add package Microsoft.AspNetCore.Mvc.Testing --version 8.0.0
cd ../backend
```

### 4. Database Setup (Docker PostgreSQL)

Create `docker-compose.yml` in project root:

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:16
    container_name: mealplanner-postgres
    environment:
      POSTGRES_USER: mealplanner
      POSTGRES_PASSWORD: mealplanner_dev_password
      POSTGRES_DB: mealplanner_dev
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U mealplanner"]
      interval: 10s
      timeout: 5s
      retries: 5

volumes:
  postgres_data:
```

Start PostgreSQL container:

```bash
docker compose up -d postgres
```

Verify container is running:

```bash
docker ps
```

Check PostgreSQL logs:

```bash
docker logs mealplanner-postgres
```

Create development database (if not auto-created):

```bash
docker exec -it mealplanner-postgres psql -U mealplanner -c "CREATE DATABASE mealplanner_dev;"
```

**Note**: Data persists in Docker volume `postgres_data` even if container stops.

### 5. Environment Configuration

Create `.env` file in backend root (if using dotenv):

```bash
cd backend
# Create .env file with:
# DATABASE_CONNECTION_STRING=Host=localhost;Port=5432;Database=mealplanner_dev;Username=mealplanner;Password=mealplanner_dev_password
# ASPNETCORE_ENVIRONMENT=Development
```

Create `appsettings.Development.json` in backend:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=mealplanner_dev;Username=mealplanner;Password=mealplanner_dev_password"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

Create `environment.ts` in frontend `src/environments/`:

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api'
};
```

### 6. Docker Setup (for Testcontainers)

Ensure Docker Desktop is running and accessible:

```bash
docker ps
```

Verify Docker Compose is available:

```bash
docker compose version
```

If Docker requires authentication, log in:

```bash
docker login
```

### 7. Install Playwright (E2E Testing)

Navigate to frontend directory:

```bash
cd frontend
```

Install Playwright:

```bash
npm install -D @playwright/test
npx playwright install
```

Install Playwright browsers:

```bash
npx playwright install --with-deps
```

## Configuration

### Frontend Configuration

Configure Tailwind CSS in `tailwind.config.js`:

```javascript
module.exports = {
  content: [
    "./src/**/*.{html,ts}",
  ],
  theme: {
    extend: {
      colors: {
        primary: '#FF8C00',
        secondary: '#FFD700',
        gray: {
          dark: '#424242',
          mid: '#616161',
          light: '#757575',
          lighter: '#9E9E9E',
        },
        background: {
          light: '#F8F8F8',
          border: '#E0E0E0',
        }
      },
      spacing: {
        '12': '12px',
        '16': '16px',
      },
      borderRadius: {
        '8': '8px',
        '20': '20px',
      }
    },
  },
  plugins: [],
}
```

Update `angular.json` to include Tailwind directives in `styles` array:

```json
"styles": [
  "src/styles.css"
]
```

Add Tailwind directives to `src/styles.css`:

```css
@tailwind base;
@tailwind components;
@tailwind utilities;
```

### Backend Configuration

Update `Program.cs` to configure CORS for frontend:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add CORS middleware (after app is built)
app.UseCors("AllowAngularApp");
```

**Note**: Entity Framework Core packages are already installed in step 3.

### Playwright Configuration

Create `playwright.config.ts` in frontend root:

```typescript
import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './e2e',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: 'html',
  use: {
    baseURL: 'http://localhost:4200',
    trace: 'on-first-retry',
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
    {
      name: 'Mobile Chrome',
      use: { ...devices['Pixel 5'] },
    },
  ],
  webServer: {
    command: 'npm start',
    url: 'http://localhost:4200',
    reuseExistingServer: !process.env.CI,
  },
});
```

## Verification

### Frontend Verification

Start Angular development server:

```bash
cd frontend
ng serve
```

Verify in browser: `http://localhost:4200`

Run Angular unit tests:

```bash
npm test
```

Run Playwright E2E tests:

```bash
npx playwright test
```

### Database Verification

Verify PostgreSQL container is running:

```bash
docker ps | grep mealplanner-postgres
```

Test database connection:

```bash
docker exec -it mealplanner-postgres psql -U mealplanner -d mealplanner_dev -c "SELECT version();"
```

Verify Docker volume exists:

```bash
docker volume ls | grep postgres_data
```

### Backend Verification

Run database migrations (if using EF Core):

```bash
cd backend
dotnet ef database update
```

Start ASP.NET Core API:

```bash
dotnet run
```

Verify API health endpoint: `http://localhost:5000/api/health`

Run backend unit tests:

```bash
dotnet test
```

Run integration tests with Testcontainers:

```bash
dotnet test --filter Category=Integration
```

### Full Stack Verification

Start both services and verify integration:

```bash
# Terminal 1: Backend
cd backend
dotnet run

# Terminal 2: Frontend
cd frontend
ng serve
```

Test API connection from frontend: Open browser DevTools → Network tab → Verify API calls succeed.

## Troubleshooting

### Node.js Version Conflicts

**Error**: `ng: command not found` or version mismatch

**Fix**:

```bash
# Verify Node.js version (should be v20.x)
node --version

# Reinstall Angular CLI if needed
npm uninstall -g @angular/cli
npm install -g @angular/cli@20
```

### .NET SDK Not Found

**Error**: `dotnet: command not found`

**Fix**:

- Add .NET SDK to PATH environment variable
- Restart terminal/IDE after installation
- Verify: `dotnet --version` should show 8.0.x

### PostgreSQL Connection Issues

**Error**: `could not connect to server` or authentication failed

**Fix**:

```bash
# Verify PostgreSQL container is running
docker ps | grep mealplanner-postgres

# If container is not running, start it:
docker compose up -d postgres

# Check container logs for errors:
docker logs mealplanner-postgres

# Verify connection string in appsettings.Development.json matches docker-compose.yml
# Default credentials: Username=mealplanner, Password=mealplanner_dev_password

# Test connection from host:
docker exec -it mealplanner-postgres psql -U mealplanner -d mealplanner_dev -c "SELECT 1;"
```

### Docker/Testcontainers Issues

**Error**: `Cannot connect to Docker daemon` or Testcontainers timeout

**Fix**:

```bash
# Ensure Docker Desktop is running
docker ps

# If on Linux, add user to docker group:
sudo usermod -aG docker $USER
# Log out and back in

# Verify Docker can pull images:
docker pull postgres:16

# If PostgreSQL container fails to start:
docker compose down
docker volume rm kata-meal-planner_postgres_data  # Remove volume if corrupted
docker compose up -d postgres
```

### Tailwind CSS Not Working

**Error**: Styles not applying, classes not recognized

**Fix**:

```bash
# Verify tailwind.config.js content paths include your component files
# Ensure @tailwind directives are in styles.css
# Restart Angular dev server after Tailwind config changes
```

### Port Already in Use

**Error**: `Port 4200/5000 already in use`

**Fix**:

```bash
# Find process using port:
# Windows: netstat -ano | findstr :4200
# Linux: lsof -i :4200

# Kill process or change port in angular.json / launchSettings.json
```

### Playwright Browser Installation Fails

**Error**: Playwright browsers not installing

**Fix**:

```bash
# Install with system dependencies:
npx playwright install --with-deps

# If on Linux, may need:
sudo npx playwright install-deps
```

### Testcontainers Tests Timeout

**Error**: Integration tests fail with timeout

**Fix**:

- Ensure Docker has sufficient resources (4GB+ RAM)
- Check Docker Desktop settings → Resources
- Verify network connectivity for pulling container images
- Increase timeout in test configuration if needed

### CORS Errors in Browser

**Error**: `Access-Control-Allow-Origin` errors

**Fix**:

- Verify CORS configuration in backend `Program.cs`
- Ensure frontend URL matches exactly (including port)
- Check browser console for specific CORS error details
