import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthApiService } from '../api';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authApi = inject(AuthApiService);
  const router = inject(Router);
  const token = authApi.token();

  if (!token) {
    return next(req);
  }

  return next(req.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`
    }
  })).pipe(
    catchError(error => {
      const isAuthEndpoint = req.url.includes('/Auth/login') || req.url.includes('/Auth/signup');

      if (error?.status === 401 && !isAuthEndpoint) {
        authApi.logout();
        void router.navigate(['/login']);
      }

      return throwError(() => error);
    })
  );
};
