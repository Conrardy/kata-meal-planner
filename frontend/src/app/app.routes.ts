import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./features/login/login.component').then(
        (m) => m.LoginComponent
      ),
    title: 'Login',
  },
  {
    path: '',
    loadComponent: () =>
      import('./features/daily-digest/daily-digest.component').then(
        (m) => m.DailyDigestComponent
      ),
    title: 'Daily Digest',
    canActivate: [authGuard],
  },
  {
    path: 'weekly-plan',
    loadComponent: () =>
      import('./features/weekly-plan/weekly-plan.component').then(
        (m) => m.WeeklyPlanComponent
      ),
    title: 'Weekly Plan',
    canActivate: [authGuard],
  },
  {
    path: 'recipes',
    loadComponent: () =>
      import('./features/recipe-browse/recipe-browse.component').then(
        (m) => m.RecipeBrowseComponent
      ),
    title: 'Browse Recipes',
    canActivate: [authGuard],
  },
  {
    path: 'recipes/new',
    loadComponent: () =>
      import('./features/recipe-create/recipe-create.component').then(
        (m) => m.RecipeCreateComponent
      ),
    title: 'Create Recipe',
    canActivate: [authGuard],
  },
  {
    path: 'recipe/:recipeId',
    loadComponent: () =>
      import('./features/recipe-details/recipe-details.component').then(
        (m) => m.RecipeDetailsComponent
      ),
    title: 'Recipe Details',
    canActivate: [authGuard],
  },
  {
    path: 'shopping-list',
    loadComponent: () =>
      import('./features/shopping-list/shopping-list.component').then(
        (m) => m.ShoppingListComponent
      ),
    title: 'Shopping List',
    canActivate: [authGuard],
  },
  {
    path: 'preferences',
    loadComponent: () =>
      import('./features/preferences/preferences.component').then(
        (m) => m.PreferencesComponent
      ),
    title: 'Preferences',
    canActivate: [authGuard],
  },
  { path: '**', redirectTo: '' },
];
