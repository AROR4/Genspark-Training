import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthApiService, BookingApiService, ScheduleSearchResponse } from '../../../core/api';
import { SearchFlowService } from '../../../core/state/search-flow.service';
import { formatApiError } from '../../../core/utils/api-error.util';
import { ToastService } from '../../../core/utils/toast.service';
import { AppHeaderComponent } from '../../../shared/layout/app-header.component';

@Component({
  selector: 'app-user-search-page',
  standalone: true,
  imports: [CommonModule, AppHeaderComponent],
  template: `
    <app-header />
    <main class="page">
      <section class="head">
        <div>
          <p class="eyebrow">Search Results</p>
          <h1>{{ from() }} to {{ to() }}</h1>
          <p class="sub">{{ date() }} · {{ results().length }} bus option(s)</p>
        </div>

        <div class="actions">
          <label>
            <span>Sort by</span>
            <select [value]="sortBy()" (change)="sortBy.set(($any($event.target).value))">
              <option value="departure">Departure time</option>
              <option value="price">Price</option>
              <option value="availability">Seats left</option>
            </select>
          </label>
          <button type="button" (click)="goHome()">Modify Search</button>
        </div>
      </section>

      <section class="results">
        @for (item of sortedResults(); track item.scheduleId) {
          <article class="card" [class.cheapest]="item.scheduleId === cheapestScheduleId()">
            <div class="topline">
              <div>
                <div class="title-row">
                  <h2>{{ item.operatorName || 'Bus Operator' }}</h2>
                  @if (item.scheduleId === cheapestScheduleId()) {
                    <span class="badge badge-gold">Cheapest</span>
                  }
                </div>
                <p>{{ cityName(item.sourceCityName, item.sourceCity) }} to {{ cityName(item.destinationCityName, item.destinationCity) }}</p>
              </div>

              <div class="price">Rs {{ displayPrice(item) }}</div>
            </div>

            <div class="grid">
              <div>
                <small>Bus Type</small>
                <strong>{{ item.busType || 'AC / Sleeper' }}</strong>
              </div>
              <div>
                <small>Departure</small>
                <strong>{{ formatTime(item.departureTime) }}</strong>
              </div>
              <div>
                <small>Arrival</small>
                <strong>{{ formatTime(item.arrivalTime) }}</strong>
              </div>
              <div>
                <small>Duration</small>
                <strong>{{ duration(item) }}</strong>
              </div>
              <div>
                <small>Available Seats</small>
                <strong>{{ item.availableSeats }}</strong>
              </div>
            </div>

            <div class="footer">
              <p [class.alert]="item.availableSeats <= 5">
                {{ seatsMessage(item.availableSeats) }}
              </p>
              <button type="button" (click)="selectSeats(item)">View Seats</button>
            </div>
          </article>
        } @empty {
          <article class="empty">No buses found for this route and date.</article>
        }
      </section>
    </main>
  `,
  styles: [`
    .page {
      min-height: 100dvh;
      padding: 26px;
      color: #f8fafc;
      background:
        radial-gradient(circle at top right, rgba(245, 158, 11, 0.12), transparent 20%),
        linear-gradient(180deg, #fff7ed 0%, #fffbeb 18%, #f8fafc 100%);
    }
    .head, .card, .empty {
      border: 1px solid rgba(15, 23, 42, 0.08);
      background: rgba(255,255,255,0.92);
      border-radius: 24px;
      box-shadow: 0 18px 40px rgba(148, 163, 184, 0.18);
    }
    .head {
      padding: 20px;
      display: flex;
      justify-content: space-between;
      align-items: end;
      gap: 18px;
      margin-bottom: 18px;
    }
    .eyebrow {
      margin: 0 0 6px;
      color: #c2410c;
      text-transform: uppercase;
      letter-spacing: 0.12em;
      font-size: 0.78rem;
      font-weight: 800;
    }
    h1, h2, p { margin: 0; color: #0f172a; }
    .sub, small { color: #64748b; }
    .actions {
      display: flex;
      align-items: end;
      gap: 12px;
      flex-wrap: wrap;
    }
    label { display: grid; gap: 8px; }
    select, button {
      min-height: 46px;
      border-radius: 14px;
      border: 1px solid rgba(15, 23, 42, 0.12);
      padding: 0 14px;
      font: inherit;
    }
    select { background: #fff; color: #0f172a; }
    button {
      border: 0;
      color: #fff;
      background: linear-gradient(135deg, #f97316, #ea580c);
      cursor: pointer;
      font-weight: 700;
    }
    .results { display: grid; gap: 14px; }
    .card {
      padding: 20px;
      display: grid;
      gap: 16px;
      border-left: 6px solid transparent;
    }
    .card.cheapest { border-left-color: #f59e0b; }
    .topline, .footer {
      display: flex;
      justify-content: space-between;
      align-items: center;
      gap: 12px;
    }
    .title-row { display: flex; align-items: center; gap: 10px; flex-wrap: wrap; }
    .badge {
      padding: 5px 10px;
      border-radius: 999px;
      font-size: 0.75rem;
      font-weight: 800;
      text-transform: uppercase;
      letter-spacing: 0.05em;
    }
    .badge-gold { color: #92400e; background: #fef3c7; }
    .price { font-size: 1.35rem; font-weight: 800; color: #0f172a; }
    .grid {
      display: grid;
      grid-template-columns: repeat(5, minmax(0, 1fr));
      gap: 12px;
      padding: 14px;
      border-radius: 18px;
      background: linear-gradient(160deg, #e2e8f0, #f8fafc);
      border: 1px solid rgba(15, 23, 42, 0.08);
    }
    .grid div { display: grid; gap: 4px; }
    .grid strong { color: #0f172a; }
    .grid small { color: #475569; }
    .footer p { color: #475569; font-weight: 600; }
    .footer .alert { color: #b45309; }
    .empty { padding: 20px; }
    @media (max-width: 900px) {
      .head, .topline, .footer { align-items: start; flex-direction: column; }
      .grid { grid-template-columns: 1fr 1fr; }
    }
    @media (max-width: 540px) {
      .grid { grid-template-columns: 1fr; }
    }
  `]
})
export class UserSearchPageComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly bookingApi = inject(BookingApiService);
  private readonly state = inject(SearchFlowService);
  private readonly authApi = inject(AuthApiService);
  private readonly toast = inject(ToastService);

  readonly from = signal('');
  readonly to = signal('');
  readonly date = signal('');
  readonly sortBy = signal<'departure' | 'price' | 'availability'>('departure');
  readonly results = computed(() => this.state.searchResults());
  readonly cheapestScheduleId = computed(() => {
    const items = this.results();
    if (!items.length) {
      return null;
    }

    return [...items].sort((a, b) => this.displayPrice(a) - this.displayPrice(b))[0]?.scheduleId ?? null;
  });
  readonly sortedResults = computed(() => {
    const items = [...this.results()];
    const sortBy = this.sortBy();

    if (sortBy === 'price') {
      return items.sort((a, b) => this.displayPrice(a) - this.displayPrice(b));
    }

    if (sortBy === 'availability') {
      return items.sort((a, b) => b.availableSeats - a.availableSeats);
    }

    return items.sort((a, b) => this.timeValue(a.departureTime) - this.timeValue(b.departureTime));
  });

  ngOnInit(): void {
    const sourceCityId = Number(this.route.snapshot.queryParamMap.get('sourceCityId'));
    const destinationCityId = Number(this.route.snapshot.queryParamMap.get('destinationCityId'));
    const from = this.route.snapshot.queryParamMap.get('from') || this.state.searchQuery()?.sourceCityName || '';
    const to = this.route.snapshot.queryParamMap.get('to') || this.state.searchQuery()?.destinationCityName || '';
    const date = this.route.snapshot.queryParamMap.get('date') || this.state.searchQuery()?.date || '';

    this.from.set(from);
    this.to.set(to);
    this.date.set(date);

    if (!this.results().length && sourceCityId > 0 && destinationCityId > 0 && date) {
      this.bookingApi.search(sourceCityId, destinationCityId, date).subscribe({
        next: buses => {
          this.state.setSearchQuery({
            sourceCityId,
            destinationCityId,
            sourceCityName: from,
            destinationCityName: to,
            date
          });
          this.state.setResults(buses);
        },
        error: error => this.toast.error(formatApiError(error, 'Unable to load buses right now.'))
      });
    }
  }

  selectSeats(schedule: ScheduleSearchResponse): void {
    if (!this.authApi.isAuthenticated()) {
      this.toast.info('Please login to continue booking.');
      void this.router.navigate(['/login']);
      return;
    }

    this.state.setSelectedSchedule(schedule);
    this.state.setSelectedSeatIds([]);
    this.state.setBookingDraft(null);
    this.state.setLatestCheckout(null);
    this.state.setLatestPayment(null);
    void this.router.navigate(['/seat-selection', schedule.scheduleId]);
  }

  goHome(): void {
    void this.router.navigate(['/']);
  }

  duration(item: ScheduleSearchResponse): string {
    const totalMinutes = item.durationMinutes || item.duration || 0;
    const hours = Math.floor(totalMinutes / 60);
    const minutes = totalMinutes % 60;
    return `${hours}h ${minutes}m`;
  }

  displayPrice(item: ScheduleSearchResponse): number {
    return item.basePrice ?? item.price ?? 0;
  }

  formatTime(value: string): string {
    return String(value || '').slice(0, 5);
  }

  seatsMessage(availableSeats: number): string {
    if (availableSeats <= 5) {
      return `Only ${availableSeats} seats left`;
    }

    return `${availableSeats} seats available`;
  }

  cityName(primary?: string | null, fallback?: string | null): string {
    return primary || fallback || 'Unknown city';
  }

  private timeValue(value: string): number {
    const [hours = '0', minutes = '0'] = String(value || '').split(':');
    return Number(hours) * 60 + Number(minutes);
  }
}
