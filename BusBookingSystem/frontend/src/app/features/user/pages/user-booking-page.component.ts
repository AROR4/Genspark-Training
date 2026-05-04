import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { BookingApiService, SeatAvailabilityResponse } from '../../../core/api';
import { SearchFlowService } from '../../../core/state/search-flow.service';
import { formatApiError } from '../../../core/utils/api-error.util';
import { ToastService } from '../../../core/utils/toast.service';
import { AppHeaderComponent } from '../../../shared/layout/app-header.component';

@Component({
  selector: 'app-user-booking-page',
  standalone: true,
  imports: [CommonModule, AppHeaderComponent],
  template: `
    <app-header />
    <main class="page">
      <section class="hero">
        <div>
          <p class="eyebrow">Step 1 of 3</p>
          <h1>Select Seats</h1>
          <p>{{ routeLabel() }}</p>
        </div>

        <div class="hero-actions">
          <button type="button" class="secondary" [disabled]="busy()" (click)="refreshSeats()">Refresh Layout</button>
          <button type="button" class="secondary" (click)="goBack()">Back</button>
        </div>
      </section>

      <section class="layout">
        <div class="seat-panel">
          <div class="legend">
            <span><i class="available"></i> Available</span>
            <span><i class="held"></i> Held</span>
            <span><i class="booked"></i> Booked</span>
            <span><i class="selected"></i> Selected</span>
          </div>

          <div class="seat-grid">
            @for (seat of seats(); track seatKey(seat)) {
              <button
                type="button"
                class="seat"
                [disabled]="!canSelect(seat)"
                [class.available]="statusOf(seat) === 'Available'"
                [class.held]="statusOf(seat) === 'Held'"
                [class.booked]="statusOf(seat) === 'Booked'"
                [class.selected]="selectedSeatIds().includes(seatKey(seat))"
                (click)="toggleSeat(seat)">
                <span>{{ seat.seatNumber || labelFor($index) }}</span>
                <small>{{ statusOf(seat) }}</small>
              </button>
            }
          </div>
        </div>

        <aside class="summary">
          <h2>Selection Summary</h2>
          <p>{{ selectedSeatIds().length }} seat(s) selected</p>
          <p>Booked seats cannot be selected.</p>
          <p>Held seats are temporarily unavailable.</p>
          <button type="button" [disabled]="busy() || !selectedSeatIds().length" (click)="continue()">Continue</button>
        </aside>
      </section>
    </main>
  `,
  styles: [`
    .page {
      min-height: 100dvh;
      padding: 26px;
      color: #0f172a;
      background:
        radial-gradient(circle at top left, rgba(34, 197, 94, 0.12), transparent 18%),
        linear-gradient(180deg, #f0fdf4 0%, #f8fafc 22%, #eef2ff 100%);
    }
    .hero, .seat-panel, .summary {
      border: 1px solid rgba(15, 23, 42, 0.08);
      background: rgba(255,255,255,0.94);
      border-radius: 24px;
      box-shadow: 0 18px 40px rgba(148, 163, 184, 0.14);
      padding: 20px;
    }
    .hero {
      display: flex;
      align-items: start;
      justify-content: space-between;
      gap: 12px;
    }
    .eyebrow {
      margin: 0 0 6px;
      color: #0f766e;
      text-transform: uppercase;
      letter-spacing: 0.12em;
      font-size: 0.78rem;
      font-weight: 800;
    }
    h1, h2, p { margin: 0; }
    p { color: #64748b; }
    .hero-actions { display: flex; gap: 10px; flex-wrap: wrap; }
    .layout { display: grid; gap: 16px; margin-top: 16px; grid-template-columns: minmax(0, 1fr) 320px; }
    .legend {
      display: flex;
      gap: 16px;
      flex-wrap: wrap;
      margin-bottom: 16px;
      color: #475569;
      font-weight: 600;
    }
    .legend span { display: inline-flex; align-items: center; gap: 8px; }
    i {
      width: 14px;
      height: 14px;
      border-radius: 4px;
      display: inline-block;
    }
    .available { background: #22c55e; }
    .held { background: #facc15; }
    .booked { background: #ef4444; }
    .selected { background: #0f172a; }
    .seat-grid {
      display: grid;
      grid-template-columns: repeat(4, minmax(70px, 1fr));
      gap: 12px;
    }
    .seat, .summary button, .secondary {
      min-height: 50px;
      border: 0;
      border-radius: 16px;
      font: inherit;
    }
    .seat {
      padding: 12px 10px;
      cursor: pointer;
      display: grid;
      gap: 6px;
      text-align: left;
      color: #0f172a;
      border: 1px solid transparent;
    }
    .seat small { color: inherit; opacity: 0.8; }
    .seat.available { background: #dcfce7; border-color: #22c55e; }
    .seat.held { background: #fef9c3; color: #854d0e; cursor: not-allowed; border-color: #eab308; }
    .seat.booked { background: #fee2e2; color: #991b1b; cursor: not-allowed; border-color: #ef4444; }
    .seat.selected { background: #0f172a; color: #fff; border-color: #0f172a; }
    .summary {
      display: grid;
      gap: 12px;
      align-content: start;
    }
    .summary button {
      color: #fff;
      background: linear-gradient(135deg, #0f766e, #0f172a);
      cursor: pointer;
    }
    .secondary {
      padding: 0 14px;
      cursor: pointer;
      color: #0f172a;
      background: #e2e8f0;
    }
    @media (max-width: 900px) { .layout { grid-template-columns: 1fr; } }
    @media (max-width: 620px) { .seat-grid { grid-template-columns: repeat(2, minmax(0, 1fr)); } }
  `]
})
export class UserBookingPageComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly bookingApi = inject(BookingApiService);
  private readonly state = inject(SearchFlowService);
  private readonly toast = inject(ToastService);

  readonly busy = signal(false);
  readonly seats = signal<SeatAvailabilityResponse[]>([]);
  readonly selectedSeatIds = signal<number[]>([]);
  readonly schedule = computed(() => this.state.selectedSchedule());
  readonly routeLabel = computed(() => {
    const schedule = this.schedule();
    return `${schedule?.sourceCityName || schedule?.sourceCity || 'Source'} to ${schedule?.destinationCityName || schedule?.destinationCity || 'Destination'}`;
  });

  ngOnInit(): void {
    const scheduleId = Number(this.route.snapshot.paramMap.get('scheduleId'));
    const selected = this.schedule();

    if (!selected || selected.scheduleId !== scheduleId) {
      const fromResults = this.state.searchResults().find(item => item.scheduleId === scheduleId) || null;
      this.state.setSelectedSchedule(fromResults);
    }

    this.refreshSeats();
  }

  refreshSeats(): void {
    const scheduleId = Number(this.route.snapshot.paramMap.get('scheduleId'));
    this.busy.set(true);
    this.bookingApi.seats(scheduleId).subscribe({
      next: seats => {
        this.busy.set(false);
        this.seats.set(seats);
        this.selectedSeatIds.set([]);
        this.state.setSelectedSeatIds([]);
      },
      error: error => {
        this.busy.set(false);
        this.toast.error(formatApiError(error, 'Unable to load seats right now.'));
      }
    });
  }

  toggleSeat(seat: SeatAvailabilityResponse): void {
    const id = this.seatKey(seat);
    const current = this.selectedSeatIds();

    if (!this.canSelect(seat)) {
      return;
    }

    if (current.includes(id)) {
      this.selectedSeatIds.set(current.filter(value => value !== id));
      return;
    }

    if (current.length >= 6) {
      this.toast.info('Maximum 6 seats can be selected.');
      return;
    }

    this.selectedSeatIds.set([...current, id]);
  }

  continue(): void {
    if (!this.selectedSeatIds().length) {
      return;
    }

    this.state.setSelectedSeatIds(this.selectedSeatIds());
    this.state.setBookingDraft(null);
    this.state.setLatestCheckout(null);
    this.state.setLatestPayment(null);
    void this.router.navigate(['/booking']);
  }

  goBack(): void {
    void this.router.navigate(['/search-results']);
  }

  seatKey(seat: SeatAvailabilityResponse): number {
    return seat.seatAvailabilityId ?? seat.seatId;
  }

  statusOf(seat: SeatAvailabilityResponse): string {
    return seat.status || 'Available';
  }

  canSelect(seat: SeatAvailabilityResponse): boolean {
    return this.statusOf(seat) === 'Available';
  }

  labelFor(index: number): string {
    const row = String.fromCharCode(65 + Math.floor(index / 4));
    const col = (index % 4) + 1;
    return `${row}${col}`;
  }
}
