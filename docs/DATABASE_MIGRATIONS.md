# Database Migrations

This document describes the database migration workflow for the MealPlanner application.

## Overview

The application uses Entity Framework Core 9 migrations to manage database schema changes. Migrations are versioned, reproducible, and can be applied automatically in development or manually in production.

## Migration Files

Migrations are located in:
```
backend/src/Infrastructure/MealPlanner.Infrastructure/Persistence/Migrations/
```

Current migrations:
- `20260120212450_InitialCreate` - Initial schema (recipes, planned_meals, user_preferences, shopping_list_states)
- `20260120213710_AddIdentityAndRefreshTokens` - ASP.NET Identity tables and refresh token support

## Prerequisites

Ensure you have:
- .NET 9 SDK installed
- EF Core CLI tools installed (`dotnet tool install --global dotnet-ef`)
- PostgreSQL server running (for development: `docker compose up -d postgres`)

## Common Operations

### List Migrations

View all migrations and their status:

```bash
cd backend/src/Api/MealPlanner.Api
dotnet ef migrations list --project ../../Infrastructure/MealPlanner.Infrastructure/MealPlanner.Infrastructure.csproj
```

### Add New Migration

When you modify the DbContext or entity configurations, create a new migration:

```bash
cd backend/src/Api/MealPlanner.Api
dotnet ef migrations add <MigrationName> --project ../../Infrastructure/MealPlanner.Infrastructure/MealPlanner.Infrastructure.csproj
```

**Naming Convention**: Use PascalCase with descriptive names:
- `AddUserProfileTable`
- `UpdateRecipeIndexes`
- `AddNutritionInfo`

### Apply Migrations (Development)

Migrations are applied automatically on startup in development mode (see `Program.cs:249`).

To manually apply migrations:

```bash
cd backend/src/Api/MealPlanner.Api
dotnet ef database update --project ../../Infrastructure/MealPlanner.Infrastructure/MealPlanner.Infrastructure.csproj
```

### Rollback Migration

Revert to a previous migration:

```bash
cd backend/src/Api/MealPlanner.Api
dotnet ef database update <PreviousMigrationName> --project ../../Infrastructure/MealPlanner.Infrastructure/MealPlanner.Infrastructure.csproj
```

Revert all migrations:

```bash
cd backend/src/Api/MealPlanner.Api
dotnet ef database update 0 --project ../../Infrastructure/MealPlanner.Infrastructure/MealPlanner.Infrastructure.csproj
```

### Remove Last Migration

If you haven't applied a migration yet, you can remove it:

```bash
cd backend/src/Api/MealPlanner.Api
dotnet ef migrations remove --project ../../Infrastructure/MealPlanner.Infrastructure/MealPlanner.Infrastructure.csproj
```

**Warning**: Only remove migrations that haven't been applied to any database (dev, staging, production).

## Production Deployment

### Generate SQL Script

For production deployments, generate an idempotent SQL script instead of applying migrations directly:

```bash
cd backend/src/Api/MealPlanner.Api
dotnet ef migrations script \
  --project ../../Infrastructure/MealPlanner.Infrastructure/MealPlanner.Infrastructure.csproj \
  --idempotent \
  --output migration-script.sql
```

**Options**:
- `--idempotent`: Generates script that can be run multiple times safely (checks migration history)
- `--output`: Saves script to file for review and execution
- `--no-transactions`: Skip transaction wrappers (use if applying manually in transactions)

### Apply SQL Script

Review the generated script, then apply it to production:

```bash
psql -h <host> -U <user> -d <database> -f migration-script.sql
```

### Generate Script for Specific Range

Generate migrations between two versions:

```bash
dotnet ef migrations script <FromMigration> <ToMigration> \
  --project ../../Infrastructure/MealPlanner.Infrastructure/MealPlanner.Infrastructure.csproj \
  --idempotent \
  --output partial-migration.sql
```

## Database Seeding

The application includes a `DatabaseSeeder` that seeds reference data in development:

**Seed Data**:
- Sample recipes (10 recipes covering breakfast, lunch, dinner, desserts)

**Location**: `backend/src/Infrastructure/MealPlanner.Infrastructure/Persistence/DatabaseSeeder.cs`

**Execution**: Automatically runs on startup in development mode (see `Program.cs:254`)

**Behavior**: Idempotent - only seeds if the database is empty

## Environment Configuration

### Development

Migrations are applied automatically on startup:

```csharp
// Program.cs (development only)
if (app.Environment.IsDevelopment())
{
    await dbContext.Database.MigrateAsync();
    await seeder.SeedAsync();
}
```

### Production

**Recommended**: Apply migrations manually using SQL scripts before deployment.

**Alternative**: Enable auto-migration in production by removing the `IsDevelopment()` check (not recommended for large-scale apps).

## Connection Strings

| Environment | Configuration File | Example |
|-------------|-------------------|---------|
| Development | `appsettings.Development.json` | `Host=localhost;Port=5432;Database=mealplanner;Username=mealplanner;Password=mealplanner_dev` |
| Production | Environment Variable | `ConnectionStrings__DefaultConnection` |

## Troubleshooting

### Migration Not Applied

Check migration history in database:

```sql
SELECT * FROM "__EFMigrationsHistory" ORDER BY "MigrationId";
```

### Connection Errors

Ensure PostgreSQL is running:

```bash
docker compose up -d postgres
docker compose ps
```

Check connection string in `appsettings.json` or environment variables.

### Migration Conflicts

If multiple developers create migrations simultaneously:
1. Pull latest migrations from version control
2. Remove your local migration (`dotnet ef migrations remove`)
3. Rebase/merge changes
4. Recreate your migration

### Reset Development Database

Complete database reset:

```bash
# Stop containers and remove volumes
docker compose down -v

# Start fresh database
docker compose up -d postgres

# Migrations will auto-apply on next app startup
```

## Best Practices

1. **Review Generated Migrations**: Always inspect the generated migration code before applying
2. **Test Migrations**: Apply migrations to a staging environment before production
3. **Backup Before Migration**: Always backup production database before applying migrations
4. **Idempotent Scripts**: Use `--idempotent` flag for production scripts
5. **Version Control**: Commit migrations to version control immediately
6. **No Manual Schema Changes**: All schema changes must go through migrations
7. **Descriptive Names**: Use clear, descriptive migration names
8. **One Concern Per Migration**: Keep migrations focused on a single feature/change

## Related Documentation

- [EF Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [Deployment Guide](./memory-bank/infra/DEPLOYMENT.md)
- [Database Architecture](./memory-bank/common/ARCHITECTURE.md#database)
