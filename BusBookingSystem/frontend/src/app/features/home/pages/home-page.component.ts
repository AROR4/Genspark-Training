import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { BookingApiService, CitiesApiService, CityOption } from '../../../core/api';
import { SearchFlowService } from '../../../core/state/search-flow.service';
import { formatApiError } from '../../../core/utils/api-error.util';
import { ToastService } from '../../../core/utils/toast.service';
import { AppHeaderComponent } from '../../../shared/layout/app-header.component';

@Component({
  selector: 'app-home-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, AppHeaderComponent],
  template: `
    <app-header />
    <main class="page">
      <section class="shell">
        <section class="hero">
          <div class="hero-copy">
            <p class="eyebrow">Search → Seats → Hold → Pay</p>
            <h1>Plan the trip. Lock the seats. Finish the booking with confidence.</h1>
            <p class="subtitle">Live availability, guided seat selection, and a timed checkout flow built for bus reservations.</p>
          </div>

          <form class="search-form" [formGroup]="form" (ngSubmit)="search()">
            <label>
              <span>Source city</span>
              <input formControlName="from" list="city-list" placeholder="Delhi" />
            </label>

            <label>
              <span>Destination city</span>
              <input formControlName="to" list="city-list" placeholder="Mumbai" />
            </label>

            <label>
              <span>Travel date</span>
              <input type="date" formControlName="date" />
            </label>

            <button type="submit" [disabled]="busy()">Search Buses</button>
          </form>

          <datalist id="city-list">
            @for (city of cityNames(); track city) {
              <option [value]="city"></option>
            }
          </datalist>
        </section>

        <section class="cards">
          <article class="card">
            <strong>Fresh seat layout on entry</strong>
            <small>The seat screen reloads live status before selection so booked seats stay blocked.</small>
          </article>
          <article class="card">
            <strong>Timed payment window</strong>
            <small>Once checkout starts, the hold countdown stays visible until payment succeeds or fails.</small>
          </article>
          <article class="card">
            <strong>Smarter search list</strong>
            <small>Cheapest bus highlighting, low-seat nudges, and sort controls help users choose faster.</small>
          </article>
        </section>
      </section>
    </main>
  `,
  styles: [`
    :host { display: block; }
    .page {
      min-height: calc(100dvh - 69px);
      padding: 24px;
      color: #f8fafc;
      background:
        radial-gradient(circle at top left, rgba(34, 197, 94, 0.12), transparent 24%),
        radial-gradient(circle at top right, rgba(249, 115, 22, 0.12), transparent 24%),
        linear-gradient(180deg, #0f172a 0%, #111827 48%, #020617 100%);
      font-family: 'Manrope', 'Avenir Next', sans-serif;
    }
    .shell { max-width: 1180px; margin: 0 auto; display: grid; gap: 28px; }
    .hero {
      display: grid;
      gap: 22px;
      padding: 32px;
      border-radius: 28px;
      background: linear-gradient(160deg, rgba(15, 23, 42, 0.92), rgba(30, 41, 59, 0.82));
      border: 1px solid rgba(148,163,184,0.16);
      box-shadow: 0 28px 80px rgba(2, 6, 23, 0.38);
    }
    .hero-copy { max-width: 780px; }
    .eyebrow {
      margin: 0 0 10px;
      color: #86efac;
      text-transform: uppercase;
      letter-spacing: 0.12em;
      font-size: 0.82rem;
      font-weight: 800;
    }
    h1 {
      margin: 0;
      font: 800 clamp(2.2rem, 5vw, 4rem) 'Sora', 'Trebuchet MS', sans-serif;
      line-height: 1.05;
    }
    .subtitle {
      margin: 12px 0 0;
      color: #cbd5e1;
      font-size: 1.02rem;
      max-width: 640px;
    }
    .search-form {
      display: grid;
      grid-template-columns: repeat(4, minmax(0, 1fr));
      gap: 14px;
      align-items: end;
    }
    label { display: grid; gap: 8px; }
    label span { color: #cbd5e1; font-size: 0.9rem; }
    input, button {
      min-height: 54px;
      border-radius: 14px;
      border: 1px solid rgba(148,163,184,0.16);
      background: rgba(15, 23, 42, 0.72);
      color: #fff;
      padding: 0 14px;
      font: inherit;
    }
    button {
      border: 0;
      background: linear-gradient(135deg, #0ea5e9, #2563eb);
      font-weight: 800;
      text-transform: uppercase;
      letter-spacing: 0.05em;
      cursor: pointer;
    }
    button:disabled { opacity: 0.65; cursor: wait; }
    .cards {
      display: grid;
      gap: 14px;
      grid-template-columns: repeat(3, minmax(0, 1fr));
    }
    .card {
      padding: 18px;
      border-radius: 18px;
      border: 1px solid rgba(148,163,184,0.14);
      background: rgba(15, 23, 42, 0.62);
      display: grid;
      gap: 8px;
    }
    .card small { color: #cbd5e1; }
    @media (max-width: 960px) {
      .search-form, .cards { grid-template-columns: 1fr 1fr; }
    }
    @media (max-width: 640px) {
      .hero { padding: 22px; }
      .search-form, .cards { grid-template-columns: 1fr; }
    }
  `]
})
export class HomePageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly bookingApi = inject(BookingApiService);
  private readonly citiesApi = inject(CitiesApiService);
  private readonly state = inject(SearchFlowService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  readonly busy = signal(false);
  readonly cities = signal<CityOption[]>([]);
  readonly cityNames = computed(() => this.cities().map(city => city.name));

  readonly form = this.fb.nonNullable.group({
    from: ['Delhi', Validators.required],
    to: ['Mumbai', Validators.required],
    date: [new Date().toISOString().slice(0, 10), Validators.required]
  });

  ngOnInit(): void {
    this.citiesApi.listOptions().subscribe({
      next: cities => this.cities.set(cities),
      error: () => this.cities.set([])
    });
  }

  search(): void {
    if (this.form.invalid) {
      this.toast.error('Please choose source, destination, and travel date.');
      return;
    }

    const value = this.form.getRawValue();
    const source = this.resolveCity(value.from);
    const destination = this.resolveCity(value.to);

    if (!source || !destination) {
      this.toast.error('Please select source and destination from the available city list.');
      return;
    }

    if (source.id === destination.id) {
      this.toast.error('Source and destination must be different.');
      return;
    }

    this.busy.set(true);
    this.bookingApi.search(source.id, destination.id, value.date).subscribe({
      next: results => {
        this.busy.set(false);
        this.state.setSearchQuery({
          sourceCityId: source.id,
          destinationCityId: destination.id,
          sourceCityName: source.name,
          destinationCityName: destination.name,
          date: value.date
        });
        this.state.setResults(results);
        this.state.setSelectedSchedule(null);
        this.state.setSelectedSeatIds([]);
        this.state.setBookingDraft(null);
        this.state.setLatestCheckout(null);
        this.state.setLatestPayment(null);
        this.state.setLatestBooking(null);

        void this.router.navigate(['/search-results'], {
          queryParams: {
            sourceCityId: source.id,
            destinationCityId: destination.id,
            from: source.name,
            to: destination.name,
            date: value.date
          }
        });
      },
      error: error => {
        this.busy.set(false);
        this.toast.error(formatApiError(error, 'Unable to search buses right now.'));
      }
    });
  }

  private resolveCity(name: string): CityOption | null {
    const normalized = name.trim().toLowerCase();
    return this.cities().find(city => city.name.trim().toLowerCase() === normalized) ?? null;
  }
}
