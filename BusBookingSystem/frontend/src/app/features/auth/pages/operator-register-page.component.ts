import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { OperatorApiService } from '../../../core/api';
import { formatApiError } from '../../../core/utils/api-error.util';
import { ToastService } from '../../../core/utils/toast.service';

@Component({
  selector: 'app-operator-register-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <main class="page">
      <section class="card">
        <span class="eyebrow">Operator Onboarding</span>
        <h1>Register a bus operator</h1>
        <p class="subcopy">Use a strong password and complete company details carefully before submitting for approval.</p>

        <form class="grid two" [formGroup]="form" (ngSubmit)="submit()">
          <label><span>Owner name</span><input formControlName="ownerName" /></label>
          <label><span>Email</span><input formControlName="email" /></label>
          <label><span>Phone</span><input formControlName="phoneNumber" /></label>
          <label><span>Password</span><input type="password" formControlName="password" /></label>
          <label><span>Confirm password</span><input type="password" formControlName="confirmPassword" /></label>
          <label><span>Company</span><input formControlName="companyName" /></label>
          <label><span>Legal name</span><input formControlName="legalName" /></label>
          <label><span>Contact email</span><input formControlName="contactEmail" /></label>
          <label><span>Contact phone</span><input formControlName="contactPhone" /></label>
          <label><span>Registration number</span><input formControlName="registrationNumber" /></label>
          <label><span>Tax number</span><input formControlName="taxNumber" /></label>
          <label><span>License number</span><input formControlName="licenseNumber" /></label>
          <label><span>Office city</span><input formControlName="officeCity" /></label>
          <label class="wide"><span>Office address</span><input formControlName="officeAddress" /></label>
          <button class="primary wide" type="submit" [disabled]="busy()">Submit request</button>
        </form>

        <span class="status">{{ message() }}</span>
        <a routerLink="/login">Back to login</a>
      </section>
    </main>
  `,
  styles: [`
    .page {
      min-height: 100dvh;
      padding: 24px;
      background: #050607;
    }

    .card {
      width: min(980px,100%);
      margin: 0 auto;
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

    .two {
      grid-template-columns: repeat(2, minmax(0,1fr));
    }

    .wide {
      grid-column: 1 / -1;
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
      background: #ef4444;
    }

    @media (max-width: 760px) {
      .two {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class OperatorRegisterPageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly operatorApi = inject(OperatorApiService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  readonly busy = signal(false);
  readonly message = signal('Submit your operator application.');
  readonly form = this.fb.nonNullable.group({
    ownerName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    phoneNumber: ['', Validators.required],
    password: ['', Validators.required],
    confirmPassword: ['', Validators.required],
    companyName: ['', Validators.required],
    legalName: ['', Validators.required],
    contactEmail: ['', [Validators.required, Validators.email]],
    contactPhone: ['', Validators.required],
    registrationNumber: [''],
    taxNumber: [''],
    licenseNumber: [''],
    officeCity: ['', Validators.required],
    officeAddress: ['', Validators.required]
  });

  submit(): void {
    if (this.form.invalid) {
      this.message.set('Please complete the operator form.');
      this.toast.error('Please complete the operator registration form.');
      return;
    }

    if (this.form.controls.password.value !== this.form.controls.confirmPassword.value) {
      this.message.set('Passwords do not match.');
      this.toast.error('Passwords do not match.');
      return;
    }

    const value = this.form.getRawValue();
    this.busy.set(true);
    this.operatorApi.register({
      ownerName: value.ownerName,
      email: value.email,
      phoneNumber: value.phoneNumber,
      password: value.password,
      companyName: value.companyName,
      legalName: value.legalName,
      contactEmail: value.contactEmail,
      contactPhone: value.contactPhone,
      registrationNumber: value.registrationNumber,
      taxNumber: value.taxNumber,
      licenseNumber: value.licenseNumber,
      offices: [{ cityName: value.officeCity, address: value.officeAddress }]
    }).subscribe({
      next: () => {
        this.busy.set(false);
        this.toast.success('Operator registration submitted.');
        void this.router.navigate(['/login']);
      },
      error: error => {
        this.busy.set(false);
        const message = formatApiError(error, 'Operator registration failed.');
        this.message.set(message);
        this.toast.error(message);
      }
    });
  }
}
