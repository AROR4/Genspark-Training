import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { BookingApiService, type ScheduleSearchResponse } from '../../../core/api';
import { formatApiError } from '../../../core/utils/api-error.util';
import { ToastService } from '../../../core/utils/toast.service';

@Component({
  selector: 'app-home-page',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <main class="landing-page">
      <section class="atmosphere" aria-hidden="true">
        <span class="mist mist-a"></span>
        <span class="mist mist-b"></span>
        <span class="mist mist-c"></span>
      </section>

      <section class="hero-shell">
        <header class="hero-topbar">
          <a class="brand" routerLink="/" aria-label="RouteMate home">
            <span class="brand-mark" aria-hidden="true"></span>
            <span class="brand-copy">
              <strong>RouteMate</strong>
              <small>Bus booking platform</small>
            </span>
          </a>

          <nav class="quick-links" aria-label="Quick actions">
            <a routerLink="/login">Log in</a>
            <a routerLink="/signup">Sign up</a>
          </nav>
        </header>

        <div class="hero-grid">
          <section class="hero-copy">
            <span class="eyebrow">Search before login</span>
            <h1>Your Journey Begins in One Search</h1>
            <p>
              Discover live schedules instantly. Anyone can check routes and timings,
              then continue to booking after login.
            </p>

            <div class="hero-badges" aria-label="Service stats">
              <article>
                <strong>500+</strong>
                <span>daily departures</span>
              </article>
              <article>
                <strong>99%</strong>
                <span>on-time network</span>
              </article>
              <article>
                <strong>4.8/5</strong>
                <span>passenger rating</span>
              </article>
            </div>
          </section>

          <section class="search-panel" aria-label="Search buses panel">
            <div class="panel-head">
              <h2>Find your bus now</h2>
              <small>{{ message() }}</small>
            </div>

            <form class="search-form" [formGroup]="form" (ngSubmit)="search()">
              <label>
                <span>From</span>
                <input formControlName="from" type="text" placeholder="Bangalore" autocomplete="off" />
              </label>
              <label>
                <span>To</span>
                <input formControlName="to" type="text" placeholder="Chennai" autocomplete="off" />
              </label>
              <label>
                <span>Travel date</span>
                <input formControlName="date" type="date" />
              </label>

              <button class="search-btn" type="submit" [disabled]="busy()">Search buses</button>
            </form>

            <p class="booking-note">Seat selection requires login, but searching is open for everyone.</p>
          </section>
        </div>

        <section class="results-wrap" aria-live="polite">
          <header class="results-head">
            <h3>Live routes</h3>
            <small>{{ results().length }} result(s)</small>
          </header>

          <div class="results-list">
            @for (item of results(); track item.scheduleId) {
              <article class="result-card">
                <div class="route-line">
                  <strong>{{ item.sourceCityName }} to {{ item.destinationCityName }}</strong>
                  <span>{{ item.operatorName || 'Operator' }}</span>
                </div>

                <div class="time-line">
                  <span>{{ item.departureTime }}</span>
                  <small>{{ duration(item.durationMinutes) }}</small>
                  <span>{{ item.arrivalTime }}</span>
                </div>

                <div class="meta-line">
                  <small>{{ item.availableSeats }}/{{ item.totalSeats }} seats</small>
                  <strong>Rs {{ item.basePrice }}</strong>
                  <a routerLink="/login">Log in to book</a>
                </div>
              </article>
            } @empty {
              <article class="empty-state">
                Search a route above to see available buses, timings, and fares.
              </article>
            }
          </div>
        </section>
      </section>
    </main>
  `,
  styles: [`
    :host {
      display: block;
      color: #ecf1ff;
      --bg-1: #0a1124;
      --bg-2: #111b37;
      --bg-3: #040712;
      --line: rgba(255, 255, 255, 0.15);
      --panel: rgba(11, 18, 40, 0.65);
      --muted: #b4bfd6;
      --accent: #7b4dff;
      --accent-2: #37c3ff;
      --accent-3: #f472b6;
      font-family: 'Manrope', 'Avenir Next', 'Segoe UI', sans-serif;
    }

    .landing-page {
      min-height: 100dvh;
      position: relative;
      padding: 24px;
      background:
        radial-gradient(circle at 20% 8%, rgba(123, 77, 255, 0.32), transparent 20%),
        radial-gradient(circle at 82% 14%, rgba(55, 195, 255, 0.25), transparent 22%),
        radial-gradient(circle at 84% 86%, rgba(244, 114, 182, 0.2), transparent 18%),
        linear-gradient(135deg, var(--bg-1) 0%, var(--bg-2) 58%, var(--bg-3) 100%);
      overflow: hidden;
    }

    .atmosphere {
      position: absolute;
      inset: 0;
      pointer-events: none;
      z-index: 0;
    }

    .mist {
      position: absolute;
      border-radius: 50%;
      filter: blur(26px);
      opacity: 0.42;
      animation: drift 10s ease-in-out infinite alternate;
    }

    .mist-a {
      width: 220px;
      height: 220px;
      left: -70px;
      top: 8%;
      background: rgba(123, 77, 255, 0.8);
    }

    .mist-b {
      width: 280px;
      height: 280px;
      right: -90px;
      top: 20%;
      background: rgba(55, 195, 255, 0.65);
      animation-delay: 1.2s;
    }

    .mist-c {
      width: 250px;
      height: 250px;
      left: 40%;
      bottom: -110px;
      background: rgba(244, 114, 182, 0.58);
      animation-delay: 2.2s;
    }

    .hero-shell {
      position: relative;
      max-width: 1320px;
      margin: 0 auto;
      z-index: 1;
      padding: 28px;
      border: 1px solid rgba(255, 255, 255, 0.2);
      border-radius: 36px;
      background:
        linear-gradient(180deg, rgba(255, 255, 255, 0.09), rgba(255, 255, 255, 0.03)),
        rgba(5, 10, 25, 0.74);
      box-shadow: 0 38px 90px rgba(0, 0, 0, 0.36);
      backdrop-filter: blur(20px);
      animation: reveal 560ms ease;
    }

    .hero-topbar,
    .brand,
    .quick-links,
    .hero-badges {
      display: flex;
      align-items: center;
    }

    .hero-topbar {
      justify-content: space-between;
      gap: 16px;
      margin-bottom: 32px;
    }

    .brand,
    .quick-links a {
      text-decoration: none;
      color: #f2f6ff;
    }

    .brand {
      gap: 14px;
    }

    .brand-mark {
      width: 52px;
      height: 52px;
      border-radius: 18px;
      background:
        radial-gradient(circle at 70% 28%, rgba(255, 255, 255, 0.35), transparent 46%),
        linear-gradient(135deg, var(--accent), var(--accent-2));
      box-shadow: inset 0 1px 0 rgba(255, 255, 255, 0.4), 0 14px 24px rgba(0, 0, 0, 0.32);
    }

    .brand-copy {
      display: grid;
      gap: 3px;
    }

    .brand-copy strong {
      font-size: 1.08rem;
      letter-spacing: 0.06em;
      text-transform: uppercase;
    }

    .brand-copy small,
    .eyebrow,
    .hero-copy p,
    .hero-badges span,
    .panel-head small,
    .booking-note,
    .result-card small,
    .empty-state {
      color: var(--muted);
    }

    .quick-links {
      gap: 10px;
      flex-wrap: wrap;
    }

    .quick-links a {
      min-height: 42px;
      padding: 0 16px;
      display: inline-flex;
      align-items: center;
      border: 1px solid rgba(255, 255, 255, 0.16);
      border-radius: 999px;
      background: rgba(255, 255, 255, 0.06);
      transition: transform 180ms ease, background 180ms ease;
    }

    .quick-links a:hover {
      transform: translateY(-2px);
      background: rgba(255, 255, 255, 0.1);
    }

    .hero-grid {
      display: grid;
      grid-template-columns: minmax(0, 1.06fr) minmax(380px, 0.94fr);
      gap: 26px;
      align-items: start;
    }

    .hero-copy {
      display: grid;
      gap: 20px;
      padding: 20px 8px 20px 0;
    }

    .eyebrow {
      letter-spacing: 0.24em;
      text-transform: uppercase;
      font-size: 0.78rem;
    }

    .hero-copy h1,
    .panel-head h2,
    .results-head h3,
    .result-card strong {
      margin: 0;
      font-family: 'Sora', 'Trebuchet MS', sans-serif;
    }

    .hero-copy h1 {
      font-size: clamp(2.8rem, 5.6vw, 4.9rem);
      line-height: 0.97;
      max-width: 10ch;
    }

    .hero-copy p {
      margin: 0;
      max-width: 58ch;
      font-size: 1.02rem;
      line-height: 1.7;
    }

    .hero-badges {
      gap: 14px;
      flex-wrap: wrap;
    }

    .hero-badges article {
      min-width: 150px;
      padding: 16px 18px;
      border: 1px solid rgba(255, 255, 255, 0.12);
      border-radius: 22px;
      background: var(--panel);
      display: grid;
      gap: 4px;
    }

    .search-panel {
      border-radius: 28px;
      border: 1px solid rgba(255, 255, 255, 0.18);
      background:
        radial-gradient(circle at 88% 0%, rgba(123, 77, 255, 0.18), transparent 36%),
        radial-gradient(circle at 14% 100%, rgba(55, 195, 255, 0.18), transparent 38%),
        rgba(8, 14, 32, 0.8);
      padding: 22px;
      display: grid;
      gap: 16px;
      animation: reveal 780ms ease;
    }

    .panel-head {
      display: grid;
      gap: 6px;
    }

    .panel-head h2 {
      font-size: 1.45rem;
    }

    .search-form {
      display: grid;
      gap: 12px;
    }

    label {
      display: grid;
      gap: 6px;
    }

    input {
      min-height: 52px;
      border-radius: 14px;
      border: 1px solid rgba(255, 255, 255, 0.16);
      background: rgba(255, 255, 255, 0.08);
      color: #f8fbff;
      font: inherit;
      padding: 0 14px;
    }

    input:focus-visible,
    .search-btn:focus-visible,
    .quick-links a:focus-visible,
    .meta-line a:focus-visible {
      outline: 3px solid #93c5fd;
      outline-offset: 2px;
    }

    .search-btn {
      min-height: 52px;
      border: 0;
      border-radius: 14px;
      background: linear-gradient(135deg, var(--accent), var(--accent-3));
      color: #fff;
      font: inherit;
      font-weight: 700;
      cursor: pointer;
      transition: transform 180ms ease, filter 180ms ease;
    }

    .search-btn:hover {
      transform: translateY(-2px);
      filter: saturate(1.08);
    }

    .search-btn:disabled {
      cursor: not-allowed;
      opacity: 0.72;
      transform: none;
    }

    .booking-note {
      margin: 0;
      font-size: 0.95rem;
    }

    .results-wrap {
      margin-top: 26px;
      display: grid;
      gap: 14px;
    }

    .results-head {
      display: flex;
      justify-content: space-between;
      align-items: center;
      gap: 12px;
    }

    .results-head h3 {
      font-size: 1.3rem;
    }

    .results-list {
      display: grid;
      gap: 12px;
    }

    .result-card {
      border: 1px solid var(--line);
      border-radius: 20px;
      background: rgba(255, 255, 255, 0.04);
      padding: 16px 18px;
      display: grid;
      gap: 12px;
      animation: reveal 420ms ease;
    }

    .route-line,
    .time-line,
    .meta-line {
      display: flex;
      flex-wrap: wrap;
      align-items: center;
      gap: 10px;
      justify-content: space-between;
    }

    .time-line span {
      font-weight: 700;
    }

    .meta-line a {
      color: #e6ecff;
      text-decoration: none;
      border-radius: 999px;
      border: 1px solid rgba(255, 255, 255, 0.2);
      min-height: 34px;
      padding: 0 12px;
      display: inline-flex;
      align-items: center;
      background: rgba(255, 255, 255, 0.06);
    }

    .empty-state {
      border: 1px dashed rgba(255, 255, 255, 0.24);
      border-radius: 20px;
      padding: 20px;
      background: rgba(255, 255, 255, 0.03);
      text-align: center;
    }

    @media (max-width: 1100px) {
      .hero-grid {
        grid-template-columns: 1fr;
      }

      .hero-copy h1 {
        max-width: 12ch;
      }

      .search-panel {
        max-width: 720px;
      }
    }

    @media (max-width: 760px) {
      .landing-page {
        padding: 14px;
      }

      .hero-shell {
        padding: 18px;
        border-radius: 28px;
      }

      .hero-topbar,
      .route-line,
      .time-line,
      .meta-line,
      .results-head {
        flex-direction: column;
        align-items: stretch;
      }

      .hero-topbar {
        margin-bottom: 20px;
      }

      .quick-links {
        width: 100%;
      }

      .hero-copy {
        padding-right: 0;
      }

      .hero-copy h1 {
        font-size: clamp(2.7rem, 13vw, 4.2rem);
      }
    }

    @media (max-width: 560px) {
      .hero-badges {
        display: grid;
        grid-template-columns: 1fr;
      }

      .quick-links a,
      .search-btn {
        width: 100%;
        justify-content: center;
      }
    }

    @keyframes reveal {
      from {
        opacity: 0;
        transform: translateY(12px);
      }
      to {
        opacity: 1;
        transform: translateY(0);
      }
    }

    @keyframes drift {
      from {
        transform: translate3d(0, 0, 0);
      }
      to {
        transform: translate3d(18px, -14px, 0);
      }
    }
  `]
})
export class HomePageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly bookingApi = inject(BookingApiService);
  private readonly toast = inject(ToastService);

  readonly busy = signal(false);
  readonly message = signal('Search routes without login.');
  readonly searchResults = signal<ScheduleSearchResponse[]>([]);
  readonly results = computed(() => this.searchResults());

  readonly form = this.fb.nonNullable.group({
    from: ['Bangalore', Validators.required],
    to: ['Chennai', Validators.required],
    date: [new Date().toISOString().slice(0, 10), Validators.required]
  });

  search(): void {
    if (this.form.invalid) {
      this.toast.error('Please enter source, destination, and travel date.');
      return;
    }

    const value = this.form.getRawValue();
    this.busy.set(true);
    this.bookingApi.search(value.from, value.to, value.date).subscribe({
      next: results => {
        this.searchResults.set(results);
        this.message.set(`Fetched ${results.length} route(s).`);
        this.toast.success(`Found ${results.length} bus option(s).`);
        this.busy.set(false);
      },
      error: error => {
        const message = formatApiError(error, 'Unable to fetch routes right now.');
        this.message.set(message);
        this.toast.error(message);
        this.busy.set(false);
      }
    });
  }

  duration(totalMinutes: number): string {
    const hours = Math.floor(totalMinutes / 60);
    const minutes = totalMinutes % 60;
    return `${hours}h ${minutes}m`;
  }
}
