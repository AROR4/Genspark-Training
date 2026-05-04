import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthApiService } from '../../../core/api';
import { formatApiError } from '../../../core/utils/api-error.util';
import { ToastService } from '../../../core/utils/toast.service';

@Component({
  selector: 'app-admin-login-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <main class="page">
      <section class="card">
        <span class="eyebrow">Admin only</span>
        <h1>Admin login</h1>
        <form class="grid" [formGroup]="form" (ngSubmit)="submit()">
          <label><span>Email</span><input formControlName="email" /></label>
          <label><span>Password</span><input type="password" formControlName="password" /></label>
          <button class="primary" type="submit" [disabled]="busy()">Enter dashboard</button>
        </form>
        <span class="status">{{ message() }}</span>
      </section>
    </main>
  `,
  styles: [`
    .page { min-height: 100dvh; display: grid; place-items: center; padding: 24px; background: #030406; }
    .card { width: min(500px,100%); display: grid; gap: 18px; padding: 30px; color: #fff; border: 1px solid rgba(255,255,255,0.08); border-radius: 28px; background: rgba(255,255,255,0.06); box-shadow: 0 24px 80px rgba(0,0,0,0.35); }
    .eyebrow, .status, label span { color: #94a3b8; }
    h1 { margin: 0; }
    .grid { display: grid; gap: 14px; }
    label { display: grid; gap: 8px; }
    input { min-height: 52px; padding: 0 16px; color: #fff; border: 1px solid rgba(255,255,255,0.08); border-radius: 14px; background: rgba(255,255,255,0.08); }
    .primary { min-height: 50px; border: 0; border-radius: 14px; color: #fff; background: #ef4444; }
  `]
})
export class AdminLoginPageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authApi = inject(AuthApiService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  readonly busy = signal(false);
  readonly message = signal('Use your admin credentials.');
  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required]
  });

  submit(): void {
  if (this.form.invalid) {
    this.toast.error('Enter email and password');
    return;
  }

  this.busy.set(true);

  this.authApi.login(this.form.getRawValue()).subscribe({
    next: (res) => {
      this.busy.set(false);

      // 🔥 DEBUG
      console.log('LOGIN RESPONSE:', res);

      const role = res.role?.toLowerCase();

      // ✅ STORE ROLE (CRITICAL)
      localStorage.setItem('token', res.token || '');
      localStorage.setItem('role', role || '');

      console.log('ROLE:', role);

      if (role === 'admin') {
        this.router.navigate(['/admin']);
      } else if (role === 'operator') {
        this.router.navigate(['/operator']);
      } else {
        this.router.navigate(['/search-results']);
      }

      this.toast.success('Login successful');
    },
    error: err => {
      this.busy.set(false);
      this.toast.error('Login failed');
    }
  });
}
}
