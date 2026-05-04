import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthApiService } from '../api';

export function roleGuard(
  expectedRole: 'user' | 'operator' | 'admin',
  redirectTo: string
): CanActivateFn {
  return () => {
    const auth = inject(AuthApiService);
    const router = inject(Router);

    const isLoggedIn = auth.isAuthenticated();
    const role = auth.role();

    if (!isLoggedIn) {
      router.navigate([redirectTo]);
      return false;
    }

    if (role !== expectedRole) {
      // redirect based on actual role
      if (role === 'admin') router.navigate(['/admin']);
      else if (role === 'operator') router.navigate(['/operator']);
      else router.navigate(['/']);

      return false;
    }

    return true;
  };
}
