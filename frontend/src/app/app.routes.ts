import { Routes } from '@angular/router';
import { DailyDigestComponent } from './features/daily-digest/daily-digest.component';
import { RecipeDetailsComponent } from './features/recipe-details/recipe-details.component';
import { WeeklyPlanComponent } from './features/weekly-plan/weekly-plan.component';
import { RecipeBrowseComponent } from './features/recipe-browse/recipe-browse.component';

export const routes: Routes = [
  { path: '', component: DailyDigestComponent },
  { path: 'weekly-plan', component: WeeklyPlanComponent },
  { path: 'recipes', component: RecipeBrowseComponent },
  { path: 'recipe/:recipeId', component: RecipeDetailsComponent },
  { path: '**', redirectTo: '' },
];
