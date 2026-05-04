import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CitiesApiService, OperatorApiService, type CityOption } from '../../../core/api';
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

          <section class="wide office-section" formArrayName="offices">
            <div class="office-head">
              <strong>Office Locations</strong>
              <button type="button" class="ghost" (click)="addOffice()">Add city</button>
            </div>

            @for (office of offices.controls; track $index) {
              <div class="office-card" [formGroupName]="$index">
                <label><span>City name</span><input formControlName="cityName" list="operator-register-city-list" /></label>
                <label><span>Address</span><input formControlName="address" /></label>
                <button type="button" class="ghost" [disabled]="offices.length === 1" (click)="removeOffice($index)">Remove</button>
              </div>
            }

            <datalist id="operator-register-city-list">
              @for (city of cityOptions(); track city.id + '-' + city.name) {
                <option [value]="city.name"></option>
              }
            </datalist>
          </section>

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

    .office-section {
      display: grid;
      gap: 12px;
    }

    .office-head {
      display: flex;
      justify-content: space-between;
      align-items: center;
      gap: 10px;
    }

    .office-card {
      display: grid;
      gap: 10px;
      padding: 14px;
      border: 1px solid rgba(255,255,255,0.08);
      border-radius: 14px;
      background: rgba(255,255,255,0.03);
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

    .ghost {
      min-height: 40px;
      padding: 0 14px;
      border: 1px solid rgba(255,255,255,0.2);
      border-radius: 10px;
      color: #fff;
      background: rgba(255,255,255,0.08);
      font: inherit;
      cursor: pointer;
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
  private readonly citiesApi = inject(CitiesApiService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  readonly busy = signal(false);
  readonly cityOptions = signal<CityOption[]>([]);
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
    offices: this.fb.array([
      this.createOfficeGroup()
    ])
  });

  get offices(): FormArray {
    return this.form.controls.offices;
  }

  ngOnInit(): void {
    this.citiesApi.listOptions().subscribe({
      next: cities => this.cityOptions.set(cities),
      error: () => this.cityOptions.set([])
    });
  }

  addOffice(): void {
    this.offices.push(this.createOfficeGroup());
  }

  removeOffice(index: number): void {
    if (this.offices.length === 1) {
      return;
    }

    this.offices.removeAt(index);
  }

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
    const offices = value.offices.map(office => {
      const cityId = this.cityIdFor(office.cityName);
      return {
        cityId,
        address: office.address
      };
    });

    if (offices.some(office => office.cityId == null)) {
      this.message.set('Select valid office cities from suggestions.');
      this.toast.error('Please choose valid office cities from suggestions.');
      return;
    }

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
      offices: offices.map(office => ({ cityId: office.cityId as number, address: office.address }))
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

  private createOfficeGroup() {
    return this.fb.nonNullable.group({
      cityName: ['', Validators.required],
      address: ['', Validators.required]
    });
  }

  private cityIdFor(cityName: string): number | null {
    const normalized = cityName.trim().toLowerCase();
    const match = this.cityOptions().find(item => item.name.toLowerCase() === normalized);

    if (!match || match.id < 0) {
      return null;
    }

    return match.id;
  }
}
