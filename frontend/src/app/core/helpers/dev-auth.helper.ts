import { environment } from '../../../environments/environment';

/**
 * Development helper to auto-login for testing
 * Only works in development mode
 */
export function autoLoginForDevelopment(): void {
  if (!environment.production) {
    const hasToken = localStorage.getItem('mealplanner_access_token');
    
    if (!hasToken) {
      console.log('[DEV] No auth token found. Setting up development credentials...');
      
      // Auto-login with test account
      fetch(`${environment.apiUrl}/api/v1/auth/login`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          email: 'test@example.com',
          password: 'Test123!',
        }),
      })
        .then((response) => response.json())
        .then((data) => {
          localStorage.setItem('mealplanner_access_token', data.accessToken);
          localStorage.setItem('mealplanner_refresh_token', data.refreshToken);
          localStorage.setItem(
            'mealplanner_user',
            JSON.stringify({ userId: data.userId, email: data.email })
          );
          console.log('[DEV] Auto-login successful. Reloading...');
          window.location.reload();
        })
        .catch((error) => {
          console.error('[DEV] Auto-login failed:', error);
          console.log('[DEV] You may need to register first or login manually.');
        });
    }
  }
}
