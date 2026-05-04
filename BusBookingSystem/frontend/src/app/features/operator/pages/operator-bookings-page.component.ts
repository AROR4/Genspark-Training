import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { OperatorApiService, OperatorTripBookingResponse, OperatorTripResponse } from '../../../core/api';
import { formatApiError } from '../../../core/utils/api-error.util';
import { ToastService } from '../../../core/utils/toast.service';

@Component({
  selector: 'app-operator-bookings-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <section class="page">
      <header class="topbar">
        <div>
          <span class="eyebrow">Bookings</span>
          <h1>Manage Trips</h1>
          <p>Review all trips created by this operator and cancel a specific trip when needed.</p>
        </div>
        <button class="secondary" type="button" [disabled]="loading()" (click)="loadTrips()">Refresh</button>
      </header>

      @if (loading()) {
        <div class="card muted">Loading trips...</div>
      } @else {
        <section class="summary-grid">
          <article class="stat-card">
            <strong>{{ trips().length }}</strong>
            <span>Total Trips</span>
          </article>
          <article class="stat-card">
            <strong>{{ activeTrips().length }}</strong>
            <span>Active Trips</span>
          </article>
          <article class="stat-card">
            <strong>{{ cancelledTrips().length }}</strong>
            <span>Cancelled Trips</span>
          </article>
        </section>

        <section class="list">
          @for (trip of trips(); track trip.scheduleId) {
            <article class="card trip-card" [class.cancelled]="trip.isCancelled">
              <div class="trip-head">
                <div>
                  <div class="title-line">
                    <strong>{{ trip.sourceCityName }} → {{ trip.destinationCityName }}</strong>
                    <span class="badge" [class.cancelled-badge]="trip.isCancelled">
                      {{ trip.isCancelled ? 'Cancelled' : 'Active' }}
                    </span>
                  </div>
                  <small>
                    Schedule #{{ trip.scheduleId }} · Bus #{{ trip.busId }} ·
                    {{ trip.company || trip.registrationNumber || (trip.type || 'Operator Bus') }}
                  </small>
                </div>

                @if (!trip.isCancelled) {
                  <button
                    class="danger"
                    type="button"
                    [disabled]="cancellingScheduleId() === trip.scheduleId"
                    (click)="cancelTrip(trip)"
                  >
                    {{ cancellingScheduleId() === trip.scheduleId ? 'Cancelling...' : 'Cancel Trip' }}
                  </button>
                }
              </div>

              <div class="trip-metrics">
                <div><span>Date</span><strong>{{ trip.travelDate }}</strong></div>
                <div><span>Time</span><strong>{{ formatTime(trip.departureTime) }} - {{ formatTime(trip.arrivalTime) }}</strong></div>
                <div><span>Duration</span><strong>{{ duration(trip.durationMinutes) }}</strong></div>
                <div><span>Price</span><strong>Rs {{ trip.basePrice }}</strong></div>
                <div><span>Total Seats</span><strong>{{ trip.totalSeats }}</strong></div>
                <div><span>Booked</span><strong>{{ trip.bookedSeats }}</strong></div>
                <div><span>Held</span><strong>{{ trip.onHoldSeats }}</strong></div>
                <div><span>Available</span><strong>{{ trip.availableSeats }}</strong></div>
                <div><span>Active Bookings</span><strong>{{ trip.activeBookingCount }}</strong></div>
              </div>

              <div class="bookings-block">
                <h3>Current Bookings</h3>

                @for (booking of trip.currentBookings; track booking.bookingId) {
                  <article class="booking-row">
                    <div>
                      <strong>Booking #{{ booking.bookingId }}</strong>
                      <small>
                        {{ booking.customerName || booking.contactEmail || booking.customerPhone || 'Customer' }}
                        · {{ booking.passengerCount }} passenger(s)
                      </small>
                    </div>

                    <div class="booking-meta">
                      <span>{{ booking.status }}</span>
                      <strong>Rs {{ booking.totalAmount }}</strong>
                    </div>

                    <div class="passenger-line">
                      {{ passengerSummary(booking) }}
                    </div>
                  </article>
                } @empty {
                  <div class="muted booking-empty">No active bookings for this trip.</div>
                }
              </div>
            </article>
          } @empty {
            <div class="card muted">No trips created yet.</div>
          }
        </section>
      }
    </section>
  `,
  styles: [`
    .page { display: grid; gap: 20px; color: #e2e8f0; }
    .topbar {
      display: flex;
      justify-content: space-between;
      align-items: start;
      gap: 16px;
    }
    .eyebrow {
      display: inline-block;
      margin-bottom: 6px;
      color: #93c5fd;
      font-size: 0.82rem;
      font-weight: 800;
      text-transform: uppercase;
      letter-spacing: 0.08em;
    }
    h1, h3, p { margin: 0; }
    .summary-grid {
      display: grid;
      grid-template-columns: repeat(3, minmax(0, 1fr));
      gap: 14px;
    }
    .stat-card, .card {
      padding: 18px;
      border-radius: 16px;
      background: #020617;
      border: 1px solid rgba(148,163,184,0.14);
    }
    .stat-card {
      display: grid;
      gap: 6px;
    }
    .stat-card strong { font-size: 1.8rem; color: #f8fafc; }
    .stat-card span, p, small, .muted { color: #94a3b8; }
    .list { display: grid; gap: 14px; }
    .trip-card {
      display: grid;
      gap: 16px;
    }
    .trip-card.cancelled {
      border-color: rgba(239,68,68,0.28);
      opacity: 0.9;
    }
    .trip-head {
      display: flex;
      justify-content: space-between;
      align-items: start;
      gap: 14px;
    }
    .title-line {
      display: flex;
      gap: 10px;
      align-items: center;
      flex-wrap: wrap;
      margin-bottom: 4px;
    }
    .badge {
      padding: 4px 10px;
      border-radius: 999px;
      background: rgba(34,197,94,0.18);
      color: #86efac;
      font-size: 0.76rem;
      font-weight: 800;
      text-transform: uppercase;
      letter-spacing: 0.05em;
    }
    .cancelled-badge {
      background: rgba(239,68,68,0.18);
      color: #fca5a5;
    }
    .trip-metrics {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(120px, 1fr));
      gap: 12px;
      padding: 14px;
      border-radius: 14px;
      background: #0b1220;
    }
    .trip-metrics div { display: grid; gap: 4px; }
    .trip-metrics span { color: #94a3b8; font-size: 0.84rem; }
    .trip-metrics strong { color: #f8fafc; }
    .bookings-block {
      display: grid;
      gap: 10px;
    }
    .booking-row {
      display: grid;
      gap: 8px;
      padding: 14px;
      border-radius: 14px;
      background: #0b1220;
      border: 1px solid rgba(148,163,184,0.1);
    }
    .booking-meta {
      display: flex;
      justify-content: space-between;
      gap: 10px;
      color: #cbd5e1;
    }
    .passenger-line {
      color: #94a3b8;
      font-size: 0.92rem;
    }
    .booking-empty {
      padding: 12px 0;
    }
    .secondary, .danger {
      min-height: 42px;
      padding: 0 14px;
      border-radius: 12px;
      border: none;
      color: white;
      cursor: pointer;
      font: inherit;
      font-weight: 700;
    }
    .secondary { background: #334155; }
    .danger { background: rgba(239,68,68,0.9); }
    @media (max-width: 900px) {
      .summary-grid { grid-template-columns: 1fr; }
      .topbar, .trip-head { flex-direction: column; }
      .booking-meta { flex-direction: column; }
    }
  `]
})
export class OperatorBookingsPageComponent {
  private readonly api = inject(OperatorApiService);
  private readonly toast = inject(ToastService);

  readonly trips = signal<OperatorTripResponse[]>([]);
  readonly loading = signal(false);
  readonly cancellingScheduleId = signal<number | null>(null);

  readonly activeTrips = computed(() => this.trips().filter(trip => !trip.isCancelled));
  readonly cancelledTrips = computed(() => this.trips().filter(trip => trip.isCancelled));

  ngOnInit(): void {
    this.loadTrips();
  }

  loadTrips(): void {
    this.loading.set(true);
    this.api.listTrips().subscribe({
      next: trips => {
        this.loading.set(false);
        this.trips.set(trips);
      },
      error: error => {
        this.loading.set(false);
        this.toast.error(formatApiError(error, 'Failed to load operator trips.'));
      }
    });
  }

  cancelTrip(trip: OperatorTripResponse): void {
    const reason = window.prompt('Enter cancellation reason', 'Trip cancelled by operator')?.trim();
    if (reason === undefined) {
      return;
    }

    this.cancellingScheduleId.set(trip.scheduleId);
    this.api.cancelTrip(trip.scheduleId, { reason: reason || 'Trip cancelled by operator' }).subscribe({
      next: response => {
        this.cancellingScheduleId.set(null);
        this.toast.success(response.message || 'Trip cancelled successfully.');
        this.trips.update(items => items.map(item => {
          if (item.scheduleId !== trip.scheduleId) {
            return item;
          }

          return { ...item, isCancelled: true };
        }));
        this.loadTrips();
      },
      error: error => {
        this.cancellingScheduleId.set(null);
        this.toast.error(formatApiError(error, 'Failed to cancel trip.'));
      }
    });
  }

  formatTime(value: string): string {
    return String(value || '').slice(0, 5);
  }

  duration(totalMinutes: number): string {
    const hours = Math.floor(totalMinutes / 60);
    const minutes = totalMinutes % 60;
    return `${hours}h ${minutes}m`;
  }

  passengerSummary(booking: OperatorTripBookingResponse): string {
    return booking.passengers.map(passenger => `${passenger.name} (${passenger.seatNumber})`).join(', ');
  }
}
