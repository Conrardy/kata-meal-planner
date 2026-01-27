import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./features/daily-digest/daily-digest.component').then(
        (m) => m.DailyDigestComponent
      ),
    title: 'Daily Digest',
  },
  {
    path: 'weekly-plan',
    loadComponent: () =>
      import('./features/weekly-plan/weekly-plan.component').then(
        (m) => m.WeeklyPlanComponent
      ),
    title: 'Weekly Plan',
  },
  {
    path: 'recipes',
    loadComponent: () =>
      import('./features/recipe-browse/recipe-browse.component').then(
        (m) => m.RecipeBrowseComponent
      ),
    title: 'Browse Recipes',
  },
  {
    path: 'recipes/new',
    loadComponent: () =>
      import('./features/recipe-create/recipe-create.component').then(
        (m) => m.RecipeCreateComponent
      ),
    title: 'Create Recipe',
  },
  {
    path: 'recipe/:recipeId',
    loadComponent: () =>
      import('./features/recipe-details/recipe-details.component').then(
        (m) => m.RecipeDetailsComponent
      ),
    title: 'Recipe Details',
  },
  {
    path: 'shopping-list',
    loadComponent: () =>
      import('./features/shopping-list/shopping-list.component').then(
        (m) => m.ShoppingListComponent
      ),
    title: 'Shopping List',
  },
  {
    path: 'preferences',
    loadComponent: () =>
      import('./features/preferences/preferences.component').then(
        (m) => m.PreferencesComponent
      ),
    title: 'Preferences',
  },
  { path: '**', redirectTo: '' },
];
