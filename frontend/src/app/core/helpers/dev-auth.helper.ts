import { environment } from '../../../environments/environment';

/**
 * Development helper - previously used for auto-login
 * Now authentication requires manual login via /login page
 * Seeded users: Emmanuel, Gabrielle (password configured in backend)
 */
export function autoLoginForDevelopment(): void {
  if (!environment.production) {
    const hasToken = localStorage.getItem('mealplanner_access_token');

    if (!hasToken) {
      console.log('[DEV] No auth token found. Please login at /login');
      console.log('[DEV] Available users: Emmanuel, Gabrielle');
      console.log('[DEV] Password: MealPlanner123!');
    }
  }
}
