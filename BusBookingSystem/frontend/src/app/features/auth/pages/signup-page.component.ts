import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthApiService } from '../../../core/api';
import { formatApiError } from '../../../core/utils/api-error.util';
import { ToastService } from '../../../core/utils/toast.service';

@Component({
  selector: 'app-signup-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <main class="page">
      <section class="card">
        <span class="eyebrow">New User</span>
        <h1>Create your account</h1>
        <p class="subcopy">Use a strong password with uppercase, lowercase, number, and special character.</p>

        <form class="grid" [formGroup]="form" (ngSubmit)="submit()">
          <label><span>Name</span><input formControlName="name" /></label>
          <label><span>Email</span><input formControlName="email" /></label>
          <label><span>Phone</span><input formControlName="phoneNumber" /></label>
          <label><span>Password</span><input type="password" formControlName="password" /></label>
          <label><span>Confirm password</span><input type="password" formControlName="confirmPassword" /></label>
          <button class="primary" type="submit" [disabled]="busy()">Sign up</button>
        </form>

        <span class="status">{{ message() }}</span>
        <a routerLink="/login">Back to login</a>
      </section>
    </main>
  `,
  styles: [`
    .page {
      min-height: 100dvh;
      display: grid;
      place-items: center;
      padding: 24px;
      background: #050607;
    }

    .card {
      width: min(520px,100%);
      display: grid;
      gap: 18px;
      padding: 30px;
      color: #fff;
      border: 1px solid rgba(255,255,255,0.08);
      border-radius: 28px;
      background: rgba(255,255,255,0.06);
      box-shadow: 0 24px 80px rgba(0,0,0,0.35);
    }

    .eyebrow,
    .status,
    .subcopy,
    label span,
    a {
      color: #94a3b8;
    }

    h1,
    .subcopy {
      margin: 0;
    }

    .grid {
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

    .primary {
      min-height: 50px;
      border: 0;
      border-radius: 14px;
      color: #fff;
      background: #2563eb;
    }
  `]
})
export class SignupPageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authApi = inject(AuthApiService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  readonly busy = signal(false);
  readonly message = signal('Create your passenger account.');
  readonly form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    phoneNumber: ['', Validators.required],
    password: ['', Validators.required],
    confirmPassword: ['', Validators.required]
  });

  submit(): void {
    if (this.form.invalid) {
      this.message.set('Please fill all fields.');
      this.toast.error('Please fill all signup fields.');
      return;
    }

    if (this.form.controls.password.value !== this.form.controls.confirmPassword.value) {
      this.message.set('Passwords do not match.');
      this.toast.error('Passwords do not match.');
      return;
    }

    this.busy.set(true);
    const { name, email, phoneNumber, password } = this.form.getRawValue();
    this.authApi.signup({ name, email, phoneNumber, password }).subscribe({
      next: () => {
        this.busy.set(false);
        this.toast.success('Account created successfully.');
        void this.router.navigate(['/user/search']);
      },
      error: error => {
        this.busy.set(false);
        const message = formatApiError(error, 'Signup failed.');
        this.message.set(message);
        this.toast.error(message);
      }
    });
  }
}
