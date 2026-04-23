import { CommonModule } from '@angular/common';
import { Component, computed, effect, inject, signal } from '@angular/core';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { BookingApiService, CreateBookingRequest, SeatAvailabilityResponse } from '../../../core/api';
import { SearchFlowService } from '../../../core/state/search-flow.service';
import { formatApiError } from '../../../core/utils/api-error.util';
import { ToastService } from '../../../core/utils/toast.service';
import { AppHeaderComponent } from '../../../shared/layout/app-header.component';

@Component({
  selector: 'app-user-booking-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, AppHeaderComponent],
  template: `
    <app-header />
    <main class="page">
      <section class="wrap">
        <div class="hero">
          <div>
            <span class="eyebrow">Seat selection</span>
            <h1>{{ schedule()?.sourceCityName }} to {{ schedule()?.destinationCityName }}</h1>
            <p>Schedule #{{ schedule()?.scheduleId }} · Bus {{ schedule()?.busId }}</p>
          </div>
          <a routerLink="/user/search">Back to search</a>
        </div>

        <section class="grid">
          <div class="seat-panel">
            <div class="panel-head">
              <h2>Available seats</h2>
              <span>{{ message() }}</span>
            </div>

            <div class="seat-grid">
              @for (seat of seats(); track seat.seatId) {
                <button
                  type="button"
                  [class.booked]="seat.isBooked"
                  [class.selected]="selectedSeatIds().includes(seat.seatId)"
                  [disabled]="seat.isBooked"
                  (click)="toggleSeat(seat)"
                >
                  {{ seat.seatNumber || seat.seatId }}
                </button>
              }
            </div>
          </div>

          <form class="booking-form" [formGroup]="form" (ngSubmit)="submit()">
            <label><span>Contact email</span><input formControlName="contactEmail" /></label>
            <label><span>Contact phone</span><input formControlName="contactPhone" /></label>
            <label><span>Payment method</span><input formControlName="paymentMethod" /></label>
            <label><span>Payment reference</span><input formControlName="paymentReference" /></label>

            <div formArrayName="passengers" class="passenger-grid">
              @for (group of passengers.controls; track $index) {
                <div [formGroupName]="$index" class="passenger-card">
                  <strong>Passenger {{ $index + 1 }}</strong>
                  <label><span>Name</span><input formControlName="name" /></label>
                  <label><span>Age</span><input type="number" formControlName="age" /></label>
                  <label><span>Gender</span><input formControlName="gender" /></label>
                </div>
              }
            </div>

            <button class="primary" type="submit" [disabled]="busy() || selectedSeatIds().length === 0">Book ticket</button>
          </form>
        </section>
      </section>
    </main>
  `,
  styles: [`
    .page { min-height: 100dvh; padding: 28px; color: #f8fafc; background: #050607; }
    .wrap, .seat-panel, .booking-form { display: grid; gap: 18px; }
    .hero, .seat-panel, .booking-form, .passenger-card { border: 1px solid rgba(255,255,255,0.08); border-radius: 24px; background: rgba(255,255,255,0.06); }
    .hero, .seat-panel, .booking-form { padding: 24px; }
    .hero, .grid, .panel-head { display: grid; gap: 18px; }
    .hero { grid-template-columns: 1fr auto; align-items: center; }
    .grid { grid-template-columns: minmax(0,1fr) 420px; }
    .eyebrow, p, a, .panel-head span, label span { color: #94a3b8; }
    h1, h2, p { margin: 0; }
    a { text-decoration: none; }
    .seat-grid { display: grid; grid-template-columns: repeat(4, minmax(58px,1fr)); gap: 12px; }
    .seat-grid button { aspect-ratio: 1.1; border: 0; border-radius: 14px; color: #fff; background: rgba(255,255,255,0.08); }
    .seat-grid .booked { opacity: 0.3; }
    .seat-grid .selected { background: #2563eb; }
    .booking-form, .passenger-grid { display: grid; gap: 14px; }
    label { display: grid; gap: 8px; }
    input { min-height: 48px; padding: 0 14px; color: #fff; border: 1px solid rgba(255,255,255,0.08); border-radius: 14px; background: rgba(255,255,255,0.08); }
    .passenger-card { padding: 16px; display: grid; gap: 10px; }
    .primary { min-height: 52px; border: 0; border-radius: 14px; color: #fff; background: #2563eb; }
    @media (max-width: 980px) { .grid, .hero { grid-template-columns: 1fr; } }
  `]
})
export class UserBookingPageComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly fb = inject(FormBuilder);
  private readonly bookingApi = inject(BookingApiService);
  private readonly state = inject(SearchFlowService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  readonly busy = signal(false);
  readonly message = signal('Loading seats...');
  readonly seats = signal<SeatAvailabilityResponse[]>([]);
  readonly selectedSeatIds = signal<number[]>([]);
  readonly schedule = computed(() => this.state.selectedSchedule());
  readonly form = this.fb.nonNullable.group({
    contactEmail: ['', [Validators.required, Validators.email]],
    contactPhone: ['', Validators.required],
    paymentMethod: ['UPI', Validators.required],
    paymentReference: ['demo-payment-ref', Validators.required],
    passengers: this.fb.array([])
  });

  get passengers(): FormArray {
    return this.form.controls.passengers;
  }

  constructor() {
    effect(() => {
      const ids = this.selectedSeatIds();
      while (this.passengers.length < ids.length) {
        this.passengers.push(this.fb.nonNullable.group({
          name: ['', Validators.required],
          age: [18, Validators.required],
          gender: ['Male', Validators.required]
        }));
      }

      while (this.passengers.length > ids.length) {
        this.passengers.removeAt(this.passengers.length - 1);
      }
    });
  }

  ngOnInit(): void {
    const scheduleId = Number(this.route.snapshot.paramMap.get('scheduleId'));
    this.bookingApi.seats(scheduleId).subscribe({
      next: seats => {
        this.seats.set(seats);
        this.message.set(`Fetched ${seats.length} seats.`);
        this.toast.success(`Loaded ${seats.length} seat(s).`);
      },
      error: error => {
        const message = formatApiError(error, 'Failed to load seats.');
        this.message.set(message);
        this.toast.error(message);
      }
    });
  }

  toggleSeat(seat: SeatAvailabilityResponse): void {
    const selected = this.selectedSeatIds();
    this.selectedSeatIds.set(selected.includes(seat.seatId)
      ? selected.filter(id => id !== seat.seatId)
      : [...selected, seat.seatId]);
  }

  submit(): void {
    if (this.form.invalid || !this.schedule()) {
      this.message.set('Fill contact and passenger details.');
      this.toast.error('Fill contact and passenger details before booking.');
      return;
    }

    const value = this.form.getRawValue();
    const passengers = value.passengers as Array<{ name: string; age: number; gender: string }>;
    const payload: CreateBookingRequest = {
      scheduleId: this.schedule()!.scheduleId,
      contactEmail: value.contactEmail,
      contactPhone: value.contactPhone,
      paymentMethod: value.paymentMethod,
      paymentReference: value.paymentReference,
      passengers: passengers.map((passenger, index) => ({
        name: passenger.name,
        age: Number(passenger.age),
        gender: passenger.gender,
        seatId: this.selectedSeatIds()[index]
      }))
    };

    this.busy.set(true);
    this.bookingApi.create(payload).subscribe({
      next: booking => {
        this.state.setLatestBooking(booking);
        this.busy.set(false);
        this.toast.success('Booking created successfully.');
        void this.router.navigate(['/user/tickets', booking.bookingId]);
      },
      error: error => {
        this.busy.set(false);
        const message = formatApiError(error, 'Booking failed.');
        this.message.set(message);
        this.toast.error(message);
      }
    });
  }
}
