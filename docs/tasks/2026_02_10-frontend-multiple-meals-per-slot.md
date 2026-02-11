# Instruction: Frontend - Display & Manage Multiple Meals Per Slot

## Feature

- **Summary**: Update the Angular frontend to display and manage multiple meals per time slot. Weekly plan renders a vertical stack of meal cards per slot with a persistent "Add meal" button. Daily digest groups meals by type.
- **Stack**: `Angular 19`, `TypeScript 5.7`, `Tailwind CSS 3`, `Vitest 2`
- **Branch name**: `feature/multiple-meals-per-slot`
- **dependencies**: `backend-multiple-meals-per-slot` (Plan A must be completed first)

## Existing files

- @frontend/src/app/core/models/weekly-plan.model.ts
- @frontend/src/app/core/services/weekly-plan.service.ts
- @frontend/src/app/features/weekly-plan/weekly-plan.component.ts
- @frontend/src/app/features/weekly-plan/weekly-plan.component.html
- @frontend/src/app/features/daily-digest/daily-digest.component.ts
- @frontend/src/app/features/daily-digest/daily-digest.component.html

## Test files to update or create

- @frontend/src/app/features/weekly-plan/weekly-plan.component.spec.ts
- @frontend/src/app/features/daily-digest/daily-digest.component.spec.ts

### New file to create

- none

## Test files to create

- none (update existing)

## Implementation phases

### Phase 1: Models

> Update the DayPlan model to use arrays instead of nullable single values per mealtype.

1. Update `weekly-plan.model.ts`
   1.1 Change `DayPlan` interface fields `breakfast`, `lunch`, `dinner` from `WeeklyMeal | null` to `WeeklyMeal[]` — commit: `refactor(model): update DayPlan to array-based meal slots`

### Phase 2: Weekly Plan Component

> Render a vertical stack of meal cards per slot. Always show "Add meal" button at bottom of each slot, whether empty or not.

> Rule: Standalone component, signals, Tailwind only. Mobile-first.

1. Update `weekly-plan.component.ts`
   1.1 Rename `getMeal(day, mealType)` to `getMeals(day, mealType)` returning `WeeklyMeal[]` instead of `WeeklyMeal | null`
   1.2 Update any references from single meal to array — commit: `refactor(weekly-plan): getMeals returns array`
2. Update `weekly-plan.component.html`
   2.1 Replace single meal card rendering with `@for` loop over `getMeals(day, mealType)`
   2.2 Each meal card keeps existing actions (swap, remove)
   2.3 Show "Add meal" button at the bottom of every slot (empty or not)
   2.4 Empty slot (0 meals): show "Add meal" placeholder as before — commit: `feat(weekly-plan): render multiple meals per slot with add button`

### Phase 3: Daily Digest Component

> Verify the daily digest handles multiple meals per type correctly.

> Rule: Daily digest already receives flat PlannedMeal[]. Verify grouping by mealType renders all meals.

1. Verify `daily-digest.component.ts` and `.html`
   1.1 Confirm the meals list is iterated without assuming 1 meal per type
   1.2 If the template groups by mealType, ensure each group renders all meals for that type
   1.3 Fix only if there is a single-meal assumption — commit: `fix(daily-digest): handle multiple meals per mealtype` (only if needed)

### Phase 4: Frontend Tests

> Update mock data and add multi-meal rendering assertions.

> Rule: Vitest. Test behavior from user perspective. No mocking of functional components.

1. Update `weekly-plan.component.spec.ts`
   1.1 Update mock `DayPlan` data to use arrays instead of nullable values
   1.2 Add test: slot with 2 meals renders 2 meal cards
   1.3 Add test: "Add meal" button is visible even when slot has meals
   1.4 Add test: empty slot shows "Add meal" placeholder — commit: `test(weekly-plan): update tests for multi-meal slots`
2. Update `daily-digest.component.spec.ts`
   2.1 Add test: 2 meals for Lunch both render in the digest — commit: `test(daily-digest): verify multi-meal rendering`

## Reviewed implementation

- [ ] Phase 1: Models
- [ ] Phase 2: Weekly Plan Component
- [ ] Phase 3: Daily Digest Component
- [ ] Phase 4: Frontend Tests

## Validation flow

1. Run `npm run build` from frontend/ — no errors
2. Run `npm run test` from frontend/ — all tests pass
3. Start full stack (backend + frontend)
4. Navigate to Weekly Plan, add 2 recipes to Monday Lunch
5. Verify both meal cards appear stacked in the Lunch slot
6. Verify "Add meal" button still visible below the 2 cards
7. Navigate to Daily Digest for Monday — verify both Lunch meals appear
8. Swap one meal — verify only that card changes
9. Remove one meal — verify the other remains and "Add meal" button persists

## Estimations

- Confidence: 9/10
  - High: model change is straightforward, template rendering is a simple @for loop, daily digest likely already handles lists
  - Risk: minor — daily digest template may have a single-meal assumption that needs fixing
- Time to implement for an AI: ~20 min
