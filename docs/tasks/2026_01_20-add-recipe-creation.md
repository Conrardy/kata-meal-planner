# Instruction: Add Recipe Creation

## Feature

- **Summary**: Enable users to create a new recipe from the “Add Recipe” quick action by navigating to a dedicated page, filling required fields (name, description, ingredients, steps) plus an optional image URL, then saving so the recipe appears in recipe browse and can be opened in recipe details.
- **Stack**: `Angular 19, TypeScript 5.7+, RxJS 7, Tailwind CSS 3, .NET 9, C# 13, MediatR 14, EF Core 9, FluentValidation, PostgreSQL 17`
- **Branch name**: `feature/add-recipe-creation`

## Existing files

- @frontend/src/app/app.component.ts
- @frontend/src/app/app.component.html
- @frontend/src/app/shared/components/sidebar/sidebar.component.ts
- @frontend/src/app/shared/components/sidebar/sidebar.component.html
- @frontend/src/app/app.routes.ts
- @frontend/src/app/core/models/recipe.model.ts
- @frontend/src/app/core/services/recipe.service.ts
- @backend/src/Api/MealPlanner.Api/Program.cs
- @backend/src/Application/MealPlanner.Application/Recipes/GetRecipeDetailsQuery.cs
- @backend/src/Application/MealPlanner.Application/Recipes/SearchRecipesQuery.cs
- @backend/src/Domain/MealPlanner.Domain/Recipes/Recipe.cs
- @backend/src/Domain/MealPlanner.Domain/Recipes/IRecipeRepository.cs
- @backend/src/Infrastructure/MealPlanner.Infrastructure/Persistence/EfCoreRecipeRepository.cs

### New file to create

- frontend/src/app/features/recipe-create/recipe-create.component.ts
- frontend/src/app/features/recipe-create/recipe-create.component.html
- backend/src/Application/MealPlanner.Application/Recipes/CreateRecipeCommand.cs

## Implementation phases

### Phase 1: Confirm existing recipe shape and UI expectations

> Ensure the create flow matches existing domain/UI models for ingredients and steps.

1. Confirm the frontend recipe model format for `ingredients` and `steps` (name/quantity/unit and stepNumber/instruction).
2. Confirm current navigation patterns and recipe detail route shape used by the app.
3. Confirm backend recipe entity fields required to create a valid `Recipe`:
    - Required: `Id`, `Name`, `Description`, `Ingredients`, `Steps`
    - Optional: `ImageUrl`
    - Defaults: `MealType` (default to `MealType.Lunch`), `Tags` (empty array)
    - Ingredient format: `new Ingredient(name, quantity, unit)` where `unit` can be null
    - Example ingredient list:
      ```csharp
      new Ingredient("Chicken breast", "200", "g"),
      new Ingredient("Mixed greens", "4", "cups"),
      new Ingredient("Salt and pepper", "to taste", null)
      ```

### Phase 2: Add navigation entrypoint for “Add Recipe”

> Make the quick action open the dedicated “New Recipe” page.

1. Add a new route for the recipe creation page.
2. Replace the current “no-op” behavior (signal/log) with navigation to the new route.
3. Keep existing sidebar UI unchanged except wiring the click to the new flow.

### Phase 3: Implement the “New Recipe” form (frontend)

> Provide a minimal, validated form for required fields and dynamic lists.

1. Build a standalone page with a typed reactive form.
2. Implement required fields: name, description, ingredients list, steps list.
3. Implement optional field: image URL with URL validation.
4. Implement add/remove rows for ingredients and steps; ensure steps are ordered and step numbers remain consistent.
5. Define clear validation errors and disable save until valid.

### Phase 4: Persist the recipe (backend + frontend)

> Add an API endpoint and application command to create a recipe and return its identifier.

1. Add a `CreateRecipe` application command with validation rules matching requirements (non-empty name/description, at least 1 ingredient, at least 1 step).
2. Extend the recipe repository contract to support adding a recipe.
3. Implement repository add behavior in the EF Core repository and persist via `SaveChangesAsync`.
4. Add an authenticated API endpoint for recipe creation that calls the command and returns `201 Created` with the new recipe id.
5. Add a frontend service method to call the create endpoint and handle success/failure.

### Phase 5: Post-create UX + tests

> Ensure the recipe is visible and the flow is reliable.

1. On success, redirect to the recipe details page for the created recipe.
2. Ensure the created recipe is visible from recipe browse (refresh or re-query).
3. Add focused tests:
   - Backend: command validation + handler behavior (happy path).
   - Frontend: form validation behavior and submission wiring (minimal unit tests).

## Reviewed implementation

- [ ] Phase 1
- [ ] Phase 2
- [ ] Phase 3
- [ ] Phase 4
- [ ] Phase 5

## Validation flow

1. Start the app and click “Add Recipe” in the sidebar quick actions.
2. Verify you land on the “New Recipe” page.
3. Try saving with missing fields and verify validation errors.
4. Fill name + description.
5. Add at least one ingredient (name, quantity, optional unit).
6. Add at least one step (instruction).
7. Optionally set an image URL and verify invalid URLs are rejected.
8. Click “Save” and verify success.
9. Verify you are redirected to the recipe details page and the recipe data matches what you entered.
10. Go to recipe browse and verify the new recipe appears.

