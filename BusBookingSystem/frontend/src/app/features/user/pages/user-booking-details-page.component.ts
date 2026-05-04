import { CommonModule } from '@angular/common';
import { Component, computed, effect, inject, signal } from '@angular/core';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { SearchFlowService } from '../../../core/state/search-flow.service';
import { ToastService } from '../../../core/utils/toast.service';
import { AppHeaderComponent } from '../../../shared/layout/app-header.component';

@Component({
  selector: 'app-user-booking-details-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, AppHeaderComponent],
  template: `
    <app-header />
    <main class="page">
      <section class="card">
        <h1>Passenger Details</h1>
        <p>Add passenger info for the selected seats before we start the payment hold window.</p>

        <form class="form" [formGroup]="form" (ngSubmit)="continue()">
          <label>
            <span>Contact email</span>
            <input formControlName="contactEmail" />
            @if (showError('contactEmail')) {
              <small class="error-text">Enter a valid email address.</small>
            }
          </label>

          <label>
            <span>Contact phone</span>
            <input formControlName="contactPhone" inputmode="tel" maxlength="14" placeholder="9876543210" />
            @if (showError('contactPhone')) {
              <small class="error-text">Enter a valid 10-digit phone number.</small>
            }
          </label>

          <div formArrayName="passengers" class="passenger-grid">
            @for (group of passengers.controls; track $index) {
              <article [formGroupName]="$index" class="passenger-card">
                <strong>Passenger {{ $index + 1 }} · Seat Ref {{ seatIds()[$index] }}</strong>
                <label>
                  <span>Name</span>
                  <input formControlName="name" />
                  @if (showPassengerError($index, 'name')) {
                    <small class="error-text">Passenger name is required.</small>
                  }
                </label>
                <label>
                  <span>Age</span>
                  <input type="number" formControlName="age" />
                  @if (showPassengerError($index, 'age')) {
                    <small class="error-text">Enter a valid age.</small>
                  }
                </label>
                <label>
                  <span>Gender</span>
                  <input formControlName="gender" />
                  @if (showPassengerError($index, 'gender')) {
                    <small class="error-text">Gender is required.</small>
                  }
                </label>
              </article>
            }
          </div>

          <button type="submit">Continue to Payment</button>
        </form>
      </section>
    </main>
  `,
  styles: [`
    .page { min-height: 100dvh; padding: 26px; color: #e2e8f0; background: #0f172a; }
    .card {
      max-width: 980px;
      margin: 0 auto;
      border: 1px solid rgba(148,163,184,0.24);
      border-radius: 16px;
      background: rgba(30,41,59,0.72);
      padding: 18px;
      display: grid;
      gap: 14px;
    }
    h1, p { margin: 0; }
    p, label span { color: #94a3b8; }
    .form, .passenger-grid { display: grid; gap: 12px; }
    .passenger-grid { grid-template-columns: repeat(2, minmax(0,1fr)); }
    .passenger-card {
      border: 1px solid rgba(148,163,184,0.24);
      border-radius: 12px;
      padding: 12px;
      display: grid;
      gap: 10px;
      background: rgba(15,23,42,0.9);
    }
    label { display: grid; gap: 8px; }
    input, button {
      min-height: 46px;
      border-radius: 10px;
      border: 1px solid rgba(148,163,184,0.24);
      background: rgba(15,23,42,0.9);
      color: #fff;
      padding: 0 12px;
      font: inherit;
    }
    .error-text {
      color: #fca5a5;
      font-size: 0.84rem;
    }
    button { border: 0; background: linear-gradient(135deg, #2563eb, #4f46e5); cursor: pointer; }
    @media (max-width: 760px) { .passenger-grid { grid-template-columns: 1fr; } }
  `]
})
export class UserBookingDetailsPageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly state = inject(SearchFlowService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  readonly submitted = signal(false);

  readonly seatIds = computed(() => this.state.selectedSeatIds());
  readonly form = this.fb.nonNullable.group({
    contactEmail: ['', [Validators.required, Validators.email]],
    contactPhone: ['', [Validators.required, Validators.pattern(/^(?:\+91)?[6-9]\d{9}$/)]],
    passengers: this.fb.array([])
  });

  get passengers(): FormArray {
    return this.form.controls.passengers;
  }

  constructor() {
    effect(() => {
      const seatIds = this.seatIds();
      while (this.passengers.length < seatIds.length) {
        this.passengers.push(this.fb.nonNullable.group({
          name: ['', Validators.required],
          age: [18, Validators.required],
          gender: ['Male', Validators.required]
        }));
      }

      while (this.passengers.length > seatIds.length) {
        this.passengers.removeAt(this.passengers.length - 1);
      }
    });
  }

  ngOnInit(): void {
    if (!this.seatIds().length || !this.state.selectedSchedule()) {
      this.toast.info('Please select seats first.');
      void this.router.navigate(['/search-results']);
    }
  }

  continue(): void {
    this.submitted.set(true);

    const normalizedPhone = this.normalizePhone(this.form.controls.contactPhone.getRawValue());
    this.form.controls.contactPhone.setValue(normalizedPhone);

    if (this.form.invalid) {
      this.toast.error('Please complete contact and passenger details.');
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.state.setBookingDraft({
      contactEmail: value.contactEmail,
      contactPhone: normalizedPhone,
      passengers: (value.passengers as Array<{ name: string; age: number; gender: string }>).map((item, index) => ({
        ...item,
        seatAvailabilityId: this.seatIds()[index]
      }))
    });

    void this.router.navigate(['/payment']);
  }

  showError(controlName: 'contactEmail' | 'contactPhone'): boolean {
    const control = this.form.controls[controlName];
    return control.invalid && (control.touched || this.submitted());
  }

  showPassengerError(index: number, controlName: 'name' | 'age' | 'gender'): boolean {
    const group = this.passengers.at(index);
    const control = group.get(controlName);
    return !!control?.invalid && (control.touched || this.submitted());
  }

  private normalizePhone(value: string): string {
    const digits = value.replace(/\D/g, '');

    if (digits.length === 12 && digits.startsWith('91')) {
      return digits.slice(2);
    }

    return digits;
  }
}
