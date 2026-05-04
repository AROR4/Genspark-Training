import { HttpClient } from '@angular/common/http';
import { Injectable, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { API_ENDPOINTS } from './api-endpoints';
import { AuthResponse, LoginRequest, SignUpRequest } from './api.models';

const AUTH_STORAGE_KEY = 'bushub.auth';
const TOKEN_STORAGE_KEY = 'token';

@Injectable({ providedIn: 'root' })
export class AuthApiService {
  readonly currentUser = signal<AuthResponse | null>(this.readStoredUser());

  constructor(private readonly http: HttpClient) {}

  login(payload: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(this.url(API_ENDPOINTS.auth.login), payload).pipe(
      tap(res => this.storeSession(res))
    );
  }

  signup(payload: SignUpRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(this.url(API_ENDPOINTS.auth.signup), payload).pipe(
      tap(res => this.storeSession(res))
    );
  }

  logout(): void {
    localStorage.removeItem(AUTH_STORAGE_KEY);
    localStorage.removeItem(TOKEN_STORAGE_KEY);
    localStorage.removeItem('role');
    this.currentUser.set(null);
  }

  token(): string | null {
    return this.currentUser()?.token ?? localStorage.getItem(TOKEN_STORAGE_KEY);
  }

  isAuthenticated(): boolean {
    return !!this.token();
  }

  // 🔥 FIXED ROLE METHOD
  role(): 'user' | 'operator' | 'admin' | null {
    const currentRole = this.currentUser()?.role;
    if (currentRole) return this.normalizeRole(currentRole);

    const stored = this.readStoredUser();
    if (stored?.role) return this.normalizeRole(stored.role);

    const legacyStoredRole = localStorage.getItem('role');
    if (legacyStoredRole) return this.normalizeRole(legacyStoredRole);

    return this.decodeRoleFromToken(this.token());
  }

  private normalizeRole(role: unknown): 'user' | 'operator' | 'admin' | null {
    if (typeof role !== 'string') return null;

    const r = role.toUpperCase();

    if (r.includes('ADMIN')) return 'admin';
    if (r.includes('OPERATOR')) return 'operator';
    if (r.includes('USER')) return 'user';

    return null;
  }

  private url(endpoint: string): string {
    return `${environment.apiBaseUrl}${endpoint}`;
  }

  private storeSession(res: AuthResponse): void {
    localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(res));

    if (res.token) {
      localStorage.setItem(TOKEN_STORAGE_KEY, res.token);
    }

    this.currentUser.set(res);
  }

  private decodeRoleFromToken(token: string | null): 'user' | 'operator' | 'admin' | null {
    if (!token) return null;

    try {
      const parts = token.split('.');
      if (parts.length < 2) {
        return null;
      }

      const base64Url = parts[1];
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      const padded = base64.padEnd(base64.length + (4 - (base64.length % 4 || 4)) % 4, '=');
      const payload = JSON.parse(atob(padded)) as Record<string, unknown>;

      const role =
        payload['role'] ??
        payload['Role'] ??
        payload['roles'] ??
        payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ??
        payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role'];

      if (Array.isArray(role) && role.length > 0) {
        return this.normalizeRole(role[0]);
      }

      return this.normalizeRole(role);
    } catch {
      return null;
    }
  }

  private readStoredUser(): AuthResponse | null {
    const raw = localStorage.getItem(AUTH_STORAGE_KEY);
    return raw ? JSON.parse(raw) : null;
  }

  validateToken(token: string): Observable<void> {
    return this.http.post<void>(this.url(API_ENDPOINTS.auth.validateToken), { token });
  }
}