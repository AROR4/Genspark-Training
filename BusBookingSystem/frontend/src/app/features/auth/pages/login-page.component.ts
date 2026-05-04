import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthApiService } from '../../../core/api';
import { formatApiError } from '../../../core/utils/api-error.util';
import { ToastService } from '../../../core/utils/toast.service';

type LoginMode = 'user' | 'operator';

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <main class="auth-page">
      <section class="auth-card">
        <span class="eyebrow">BusHub Access</span>
        <h1>Login as user or operator</h1>
        <p>Sign in and continue to your live booking workspace.</p>

        <div class="mode-switch">
          <button type="button" [class.active]="mode() === 'user'" (click)="mode.set('user')">User</button>
          <button type="button" [class.active]="mode() === 'operator'" (click)="mode.set('operator')">Operator</button>
        </div>

        <form [formGroup]="form" (ngSubmit)="submit()" class="form-grid">
          <label><span>Email</span><input formControlName="email" /></label>
          <label><span>Password</span><input type="password" formControlName="password" /></label>
          <button class="primary" type="submit" [disabled]="busy()">Continue</button>
        </form>

        <span class="status">{{ message() }}</span>

        <div class="links">
          <a routerLink="/signup">Create user account</a>
          <a routerLink="/operator/register">Register operator</a>
        </div>
      </section>
    </main>
  `,
  styles: [`
    .auth-page {
      min-height: 100dvh;
      display: grid;
      place-items: center;
      padding: 24px;
      background:
        radial-gradient(circle at top right, rgba(37,99,235,0.22), transparent 28%),
        radial-gradient(circle at bottom left, rgba(239,68,68,0.18), transparent 28%),
        #040506;
    }

    .auth-card {
      width: min(520px, 100%);
      display: grid;
      gap: 18px;
      padding: 30px;
      color: #f8fafc;
      border: 1px solid rgba(255,255,255,0.08);
      border-radius: 28px;
      background: rgba(255,255,255,0.06);
      box-shadow: 0 24px 80px rgba(0,0,0,0.35);
    }

    .eyebrow,
    p,
    .status,
    label span,
    .links a {
      color: #94a3b8;
    }

    h1,
    p {
      margin: 0;
    }

    .mode-switch,
    .links {
      display: flex;
      gap: 10px;
      flex-wrap: wrap;
    }

    .mode-switch button,
    .links a,
    .primary {
      min-height: 46px;
      padding: 0 16px;
      border: 0;
      border-radius: 999px;
      color: #fff;
      background: rgba(255,255,255,0.08);
      text-decoration: none;
      font: inherit;
    }

    .mode-switch .active,
    .primary {
      background: #2563eb;
    }

    .form-grid {
      display: grid;
      gap: 14px;
    }

    label {
      display: grid;
      gap: 8px;
    }

    input {
      min-height: 52px;
      padding: 0 16px;
      color: #fff;
      border: 1px solid rgba(255,255,255,0.08);
      border-radius: 14px;
      background: rgba(255,255,255,0.08);
    }
  `]
})
export class LoginPageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authApi = inject(AuthApiService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  readonly mode = signal<LoginMode>('user');
  readonly busy = signal(false);
  readonly message = signal('Choose how you want to sign in.');
  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required]
  });

  ngOnInit(): void {
    const role = this.authApi.role();
    if (!this.authApi.isAuthenticated() || !role) {
      return;
    }

    const target = role === 'admin'
      ? '/admin/approvals'
      : role === 'operator'
        ? '/operator/buses'
        : '/';

    void this.router.navigateByUrl(target);
  }

  submit(): void {
  if (this.form.invalid) {
    this.message.set('Enter email and password.');
    this.toast.error('Please enter both email and password.');
    return;
  }

  this.busy.set(true);

  this.authApi.login(this.form.getRawValue()).subscribe({
    next: response => {
      this.busy.set(false);

      // 🔥 DEBUG START
      console.log('================ LOGIN DEBUG ================');
      console.log('FULL RESPONSE:', response);
      console.log('Response.role:', response.role);
      console.log('Mode selected:', this.mode());
      console.log('Service role():', this.authApi.role());
      console.log('LocalStorage auth:', localStorage.getItem('bushub.auth'));
      console.log('Token:', this.authApi.token());
      console.log('============================================');
      // 🔥 DEBUG END

      // ⚠️ TEMP FIX: USE RESPONSE DIRECTLY
      const role = (response.role || '').toLowerCase();

      if (role === 'admin') {
        this.authApi.logout();
        this.toast.info('Admin login is available at /admin/login.');
        this.router.navigate(['/admin/login']);
        return;
      }

      if (role === 'operator') {
        this.toast.success('Operator login successful.');
        this.router.navigate(['/operator']);
        return;
      }

      this.toast.success('Login successful.');
      this.router.navigate(['/']);
    },

    error: error => {
      this.busy.set(false);
      const message = formatApiError(error, 'Login failed.');
      this.message.set(message);
      this.toast.error(message);
    }
  });
}
}
