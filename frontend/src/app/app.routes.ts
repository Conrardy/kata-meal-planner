import { Routes } from '@angular/router';
import { DailyDigestComponent } from './features/daily-digest/daily-digest.component';
import { RecipeDetailsComponent } from './features/recipe-details/recipe-details.component';
import { WeeklyPlanComponent } from './features/weekly-plan/weekly-plan.component';
import { RecipeBrowseComponent } from './features/recipe-browse/recipe-browse.component';
import { ShoppingListComponent } from './features/shopping-list/shopping-list.component';
import { PreferencesComponent } from './features/preferences/preferences.component';

export const routes: Routes = [
  { path: '', component: DailyDigestComponent },
  { path: 'weekly-plan', component: WeeklyPlanComponent },
  { path: 'recipes', component: RecipeBrowseComponent },
  { path: 'recipe/:recipeId', component: RecipeDetailsComponent },
  { path: 'shopping-list', component: ShoppingListComponent },
  { path: 'preferences', component: PreferencesComponent },
  { path: '**', redirectTo: '' },
];
