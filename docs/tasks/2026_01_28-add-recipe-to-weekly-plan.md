# Instruction: Add Recipe to Weekly Plan from Empty Cell

## Feature

- **Summary**: Allow users to add a recipe to an empty meal slot in the weekly plan by clicking on the cell, selecting a recipe from a searchable modal, and submitting to update both the weekly plan and shopping list.
- **Stack**: Angular 19, TypeScript 5.7+, .NET 9, C# 13, PostgreSQL 17, Tailwind CSS 3
- **Branch name**: `feature/add-recipe-to-weekly-plan`
- **dependencies**: None

## Existing files

- @frontend/src/app/features/weekly-plan/weekly-plan.component.ts
- @frontend/src/app/features/weekly-plan/weekly-plan.component.html
- @frontend/src/app/core/services/meal-plan.service.ts
- @frontend/src/app/core/services/recipe.service.ts
- @frontend/src/app/core/models/weekly-plan.model.ts
- @backend/src/Application/MealPlanner.Application/Meals/AddRecipeToMealPlanCommand.cs
- @backend/src/Application/MealPlanner.Application/Meals/Validators/AddRecipeToMealPlanCommandValidator.cs

## New files to create

- @frontend/src/app/features/weekly-plan/components/add-recipe-modal/add-recipe-modal.component.ts
- @frontend/src/app/features/weekly-plan/components/add-recipe-modal/add-recipe-modal.component.html
- @frontend/src/app/features/weekly-plan/components/add-recipe-modal/add-recipe-modal.component.spec.ts

## Test files to update or create

- @frontend/src/app/features/weekly-plan/weekly-plan.component.spec.ts
- @backend/tests/Application.Tests/Meals/AddRecipeToMealPlanCommandHandlerTests.cs
- @backend/tests/Application.Tests/Meals/AddRecipeToMealPlanCommandValidatorTests.cs

## Implementation phases

### Phase 1: Backend - Enforce empty slot validation

> Modify backend to reject adding recipe to already filled slots

1. Update AddRecipeToMealPlanCommandHandler to return error if slot is filled
   1.1 Remove swap logic from handler (lines 38-53)
   1.2 Return ErrorOr.Error if existingMeal is not null
   1.3 Commit: "feat(backend): reject adding recipe to filled meal slots"

2. Update AddRecipeToMealPlanCommandValidator to add slot validation rule
   2.1 Add async validation rule to check slot is empty
   2.2 Commit: "feat(backend): add validation for empty meal slots"

3. Update tests for new validation behavior
   3.1 Add test case for filled slot rejection
   3.2 Update existing tests to reflect new behavior
   3.3 Commit: "test(backend): add tests for empty slot validation"

### Phase 2: Frontend - Create Add Recipe Modal Component

> Build reusable modal component to select recipes with search

1. Create AddRecipeModalComponent structure
   1.1 Generate component files (ts, html, spec)
   1.2 Add inputs: date (string), mealType (string)
   1.3 Add outputs: close (void), recipeSelected (string)
   1.4 Inject RecipeService
   1.5 Create signals: recipes, filteredRecipes, selectedRecipeId, isLoading, error, searchTerm
   1.6 Commit: "feat(frontend): create add recipe modal component structure"

2. Implement recipe loading and filtering logic
   2.1 Load all recipes on ngOnInit using RecipeService.searchRecipes()
   2.2 Sort recipes alphabetically by name
   2.3 Implement computed signal for filteredRecipes based on searchTerm
   2.4 Implement case-insensitive filtering
   2.5 Commit: "feat(frontend): implement recipe loading and search filtering"

3. Build modal template
   3.1 Add modal backdrop with click handler
   3.2 Add modal header with title "Add a Recipe" and close button
   3.3 Add search input with placeholder "Search recipes..."
   3.4 Add scrollable recipe list with radio selection
   3.5 Display recipe cards: image, name, tags
   3.6 Add footer with "Cancel" and "Add" buttons
   3.7 Disable "Add" button when no recipe selected
   3.8 Show "No recipes found" message when filter returns empty
   3.9 Commit: "feat(frontend): build add recipe modal template"

4. Style modal with Tailwind CSS
   4.1 Apply fixed modal positioning and backdrop
   4.2 Style modal container, header, search input
   4.3 Style recipe list with scroll, recipe cards
   4.4 Style buttons and states (hover, disabled)
   4.5 Commit: "style(frontend): apply tailwind styles to add recipe modal"

### Phase 3: Frontend - Integrate Modal into Weekly Plan

> Wire up empty cell clicks to open add recipe modal

1. Update WeeklyPlanComponent for empty cell interaction
   1.1 Add signals: addingRecipeDate, addingRecipeMealType, isAdding
   1.2 Add method onEmptyCellClick(date: string, mealType: string)
   1.3 Add method onCloseAddModal()
   1.4 Add method onRecipeSelected(recipeId: string)
   1.5 Commit: "feat(frontend): add modal state management to weekly plan"

2. Update weekly plan template
   2.1 Modify empty cell div to add click handler
   2.2 Change placeholder text to "Add a meal"
   2.3 Add hover styles: cursor-pointer, border-2 border-dashed border-primary-light
   2.4 Add AddRecipeModalComponent to imports
   2.5 Add modal instance with conditional rendering
   2.6 Bind modal inputs and outputs
   2.7 Commit: "feat(frontend): wire up empty cells to add recipe modal"

### Phase 4: Frontend - API Integration and State Management

> Connect modal to backend API and update UI on success

1. Implement onRecipeSelected handler
   1.1 Set isAdding loading state
   1.2 Call mealPlanService.addRecipeToMealPlan with date, mealType, recipeId
   1.3 Handle success: update weeklyPlan signal with new meal
   1.4 Close modal and show success toast
   1.5 Handle error: display error message in modal, keep modal open
   1.6 Commit: "feat(frontend): implement recipe addition with API call"

2. Update WeeklyPlan signal after successful addition
   2.1 Create helper method updatePlanWithNewMeal
   2.2 Map through days to find matching date
   2.3 Update appropriate meal property (breakfast/lunch/dinner)
   2.4 Transform API response to WeeklyMeal format
   2.5 Commit: "feat(frontend): update weekly plan state after adding recipe"

3. Add toast notification component or service
   3.1 Create simple toast service with signal-based state
   3.2 Add showToast method with message and type
   3.3 Display toast in weekly plan component template
   3.4 Auto-dismiss after 3 seconds
   3.5 Commit: "feat(frontend): add toast notification for user feedback"

### Phase 5: Testing

> Ensure comprehensive test coverage for new feature

1. Write AddRecipeModalComponent tests
   1.1 Test component initialization and recipe loading
   1.2 Test search filtering (case-insensitive, empty results)
   1.3 Test recipe selection and button states
   1.4 Test modal close and recipe selected events
   1.5 Commit: "test(frontend): add tests for add recipe modal component"

2. Update WeeklyPlanComponent tests
   2.1 Test empty cell click opens modal with correct props
   2.2 Test filled cell click does nothing
   2.3 Test successful recipe addition updates plan
   2.4 Test error handling displays error message
   2.5 Test modal close resets state
   2.6 Commit: "test(frontend): add tests for weekly plan recipe addition"

3. Backend integration tests
   3.1 Test adding recipe to empty slot succeeds
   3.2 Test adding recipe to filled slot returns 400 error
   3.3 Test shopping list is updated after addition
   3.4 Commit: "test(backend): add integration tests for meal plan addition"

## Reviewed implementation

- [ ] Phase 1
- [ ] Phase 2
- [ ] Phase 3
- [ ] Phase 4
- [ ] Phase 5

## Validation flow

1. Navigate to Weekly Plan view
2. Identify an empty meal cell (displays "Add a meal")
3. Hover over empty cell - verify cursor changes and border appears
4. Click on empty cell - modal opens with "Add a Recipe" title
5. Verify all recipes are displayed alphabetically
6. Type in search bar - verify list filters in real-time
7. Search for non-existent recipe - verify "No recipes found" message
8. Clear search and select a recipe - verify "Add" button becomes enabled
9. Click "Cancel" - modal closes without changes
10. Re-open modal, select a recipe, click "Add"
11. Verify modal closes, success toast appears
12. Verify weekly plan updates with selected recipe
13. Verify clicking on now-filled cell does NOT open add modal
14. Navigate to Shopping List - verify ingredients are added

## Estimations

- **Confidence**: 9/10

**✅ High Confidence Reasons**:
- Backend command and service already exist
- Similar modal pattern (SwapModal) exists as reference
- RecipeService already has searchRecipes method
- MealPlanService already has addRecipeToMealPlan method
- Weekly plan component structure is well-understood
- Clear, well-defined requirements with no ambiguity

**❌ Potential Risks**:
- Backend may need refactoring to properly distinguish swap vs add operations
- Toast notification system may need to be implemented from scratch if not existing
