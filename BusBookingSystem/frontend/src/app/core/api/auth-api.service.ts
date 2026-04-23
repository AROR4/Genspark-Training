import { HttpClient } from '@angular/common/http';
import { Injectable, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { API_ENDPOINTS } from './api-endpoints';
import { AuthResponse, LoginRequest, SignUpRequest } from './api.models';

const AUTH_STORAGE_KEY = 'bushub.auth';

@Injectable({ providedIn: 'root' })
export class AuthApiService {
  readonly currentUser = signal<AuthResponse | null>(this.readStoredUser());

  constructor(private readonly http: HttpClient) {}

  login(payload: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(this.url(API_ENDPOINTS.auth.login), payload).pipe(
      tap(response => this.storeSession(response))
    );
  }

  signup(payload: SignUpRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(this.url(API_ENDPOINTS.auth.signup), payload).pipe(
      tap(response => this.storeSession(response))
    );
  }

  logout(): void {
    localStorage.removeItem(AUTH_STORAGE_KEY);
    this.currentUser.set(null);
  }

  token(): string | null {
    return this.currentUser()?.token ?? null;
  }

  role(): 'user' | 'operator' | 'admin' | null {
    const role = (this.currentUser()?.role || '').toUpperCase();

    if (role.includes('ADMIN')) {
      return 'admin';
    }

    if (role.includes('OPERATOR')) {
      return 'operator';
    }

    if (role.includes('USER')) {
      return 'user';
    }

    return null;
  }

  isAuthenticated(): boolean {
    return !!this.token();
  }

  private url(endpoint: string): string {
    return `${environment.apiBaseUrl}${endpoint}`;
  }

  private storeSession(response: AuthResponse): void {
    localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(response));
    this.currentUser.set(response);
  }

  private readStoredUser(): AuthResponse | null {
    const raw = localStorage.getItem(AUTH_STORAGE_KEY);
    return raw ? JSON.parse(raw) as AuthResponse : null;
  }
}
