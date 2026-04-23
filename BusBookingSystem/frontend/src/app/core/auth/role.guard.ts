import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthApiService } from '../api';

export const roleGuard = (expectedRole: 'user' | 'operator' | 'admin', redirectUrl: string): CanActivateFn => () => {
  const authApi = inject(AuthApiService);
  const router = inject(Router);

  if (!authApi.isAuthenticated()) {
    return router.createUrlTree([redirectUrl]);
  }

  if (authApi.role() !== expectedRole) {
    return router.createUrlTree([redirectUrl]);
  }

  return true;
};
