# Codebase Structure

## Overview

MealPlanner is a full-stack application with Angular 19 frontend and .NET 9 backend following Clean Architecture principles.

## Root Structure

```plaintext
kata-meal-planner/
├── frontend/                    # Angular 19 standalone app
├── backend/                     # .NET 9 Clean Architecture
├── docs/                        # Documentation
│   ├── memory-bank/            # Shared context & decisions
│   ├── rules/                  # Project rules & patterns
│   ├── agents/                 # AI agent instructions
│   └── issues/                 # Feature specifications
├── aidd/                        # AI-driven development config
├── CLAUDE.md                    # Project instructions
├── README.md
└── prd.json                     # Product requirements
```

## Frontend

@frontend/package.json

```plaintext
frontend/src/app/
├── core/
│   ├── models/                  # Domain models
│   │   ├── daily-digest.model.ts
│   │   ├── preferences.model.ts
│   │   ├── recipe.model.ts
│   │   ├── shopping-list.model.ts
│   │   └── weekly-plan.model.ts
│   └── services/                # HTTP clients
│       ├── daily-digest.service.ts
│       ├── meal-plan.service.ts
│       ├── preferences.service.ts
│       ├── recipe.service.ts
│       ├── shopping-list.service.ts
│       └── weekly-plan.service.ts
├── features/
│   ├── daily-digest/            # Daily meals view, swap modal
│   ├── weekly-plan/             # Weekly calendar view
│   ├── recipe-browse/           # Recipe search with filters
│   ├── recipe-details/          # Recipe view
│   ├── shopping-list/           # Shopping list management
│   └── preferences/             # User dietary preferences
├── shared/
│   └── components/
│       └── sidebar/             # Navigation sidebar
├── app.routes.ts                # Feature-based routing
└── app.config.ts                # Providers, interceptors
```

## Backend

@backend/MealPlanner.sln

```plaintext
backend/src/
├── Domain/MealPlanner.Domain/
│   ├── Meals/                   # MealType, PlannedMeal, IPlannedMealRepository
│   ├── Recipes/                 # Recipe, IRecipeRepository
│   ├── ShoppingList/            # ShoppingItem, ItemCategory, IShoppingListStateRepository
│   └── Preferences/             # DietaryPreference, Allergy, IUserPreferencesRepository
├── Application/MealPlanner.Application/
│   ├── DailyDigest/             # GetDailyDigestQuery, DailyDigestDto
│   ├── Meals/                   # SuggestionsDto
│   ├── Recipes/                 # SearchRecipesQuery, GetRecipeDetailsQuery
│   ├── ShoppingList/            # ToggleShoppingItemCommand, AddCustomItemCommand, RemoveShoppingItemCommand
│   ├── WeeklyPlan/              # GetWeeklyPlanQuery, WeeklyPlanDto
│   └── Preferences/             # GetUserPreferencesQuery, UpdateUserPreferencesCommand
├── Infrastructure/MealPlanner.Infrastructure/
│   └── Persistence/             # InMemory repositories
└── Api/MealPlanner.Api/
    ├── Program.cs               # Route configuration, middleware
    ├── appsettings.json
    └── Properties/launchSettings.json
```

## Key Files

| File | Purpose |
|------|---------|
| @frontend/package.json | Frontend dependencies |
| @frontend/angular.json | Angular build config |
| @frontend/tailwind.config.js | Tailwind design tokens |
| @frontend/src/app/app.routes.ts | Feature routing |
| @backend/MealPlanner.sln | Solution file |
| @backend/src/Api/MealPlanner.Api/MealPlanner.Api.csproj | API project |
| @backend/src/Api/MealPlanner.Api/appsettings.json | Backend config |
| @prd.json | User stories tracking |
