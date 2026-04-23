import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { BookingApiService, ScheduleSearchResponse } from '../../../core/api';
import { SearchFlowService } from '../../../core/state/search-flow.service';
import { formatApiError } from '../../../core/utils/api-error.util';
import { ToastService } from '../../../core/utils/toast.service';
import { AppHeaderComponent } from '../../../shared/layout/app-header.component';

@Component({
  selector: 'app-user-search-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, AppHeaderComponent],
  template: `
    <app-header />
    <main class="page">
      <section class="search-hero">
        <div>
          <span class="eyebrow">Passenger search</span>
          <h1>Find buses in real time</h1>
          <p>Results come from <code>GET /api/Bookings/search</code>.</p>
        </div>

        <form class="search-form" [formGroup]="form" (ngSubmit)="search()">
          <label><span>From</span><input formControlName="from" /></label>
          <label><span>To</span><input formControlName="to" /></label>
          <label><span>Date</span><input type="date" formControlName="date" /></label>
          <button class="primary" type="submit" [disabled]="busy()">Search buses</button>
        </form>
      </section>

      <section class="results">
        <div class="results-head">
          <h2>{{ results().length }} result(s)</h2>
          <span>{{ message() }}</span>
        </div>

        @for (item of results(); track item.scheduleId) {
          <article class="result-card">
            <div class="main">
              <div>
                <strong>{{ item.operatorName || 'Bus operator' }}</strong>
                <span>{{ item.sourceCityName }} to {{ item.destinationCityName }}</span>
              </div>
              <div class="time-line">
                <strong>{{ item.departureTime }}</strong>
                <small>{{ duration(item.durationMinutes) }}</small>
                <strong>{{ item.arrivalTime }}</strong>
              </div>
            </div>

            <div class="meta">
              <span>{{ item.availableSeats }}/{{ item.totalSeats }} seats</span>
              <span>Bus {{ item.busId }}</span>
              <span>{{ item.travelDate }}</span>
            </div>

            <div class="action-row">
              <div>
                <strong>Rs {{ item.basePrice }}</strong>
                <small>Schedule #{{ item.scheduleId }}</small>
              </div>
              <button type="button" class="primary" (click)="book(item)">Select seats</button>
            </div>
          </article>
        } @empty {
          <article class="empty">No buses yet. Run a search or check backend data.</article>
        }
      </section>
    </main>
  `,
  styles: [`
    .page { min-height: 100dvh; padding: 28px; color: #f8fafc; background: #050607; }
    .search-hero, .result-card, .empty { border: 1px solid rgba(255,255,255,0.08); border-radius: 24px; background: rgba(255,255,255,0.06); }
    .search-hero { display: grid; grid-template-columns: minmax(280px,1fr) minmax(340px,480px); gap: 24px; padding: 24px; }
    .eyebrow, p, .results-head span, label span, .result-card span, .result-card small, .empty { color: #94a3b8; }
    h1, h2, p { margin: 0; }
    .search-form, .results { display: grid; gap: 14px; }
    label { display: grid; gap: 8px; }
    input, .primary { min-height: 52px; border-radius: 14px; font: inherit; }
    input { padding: 0 16px; color: #fff; border: 1px solid rgba(255,255,255,0.08); background: rgba(255,255,255,0.08); }
    .primary { padding: 0 16px; border: 0; color: #fff; background: #2563eb; }
    .results { margin-top: 24px; }
    .results-head, .main, .meta, .action-row { display: grid; gap: 12px; }
    .results-head { grid-template-columns: 1fr auto; align-items: center; }
    .result-card { padding: 20px; }
    .main { grid-template-columns: 1fr auto; align-items: center; }
    .time-line { display: grid; grid-template-columns: auto auto auto; gap: 14px; align-items: center; }
    .meta, .action-row { grid-template-columns: repeat(3, minmax(0,1fr)); align-items: center; }
    .action-row { grid-template-columns: 1fr auto; margin-top: 12px; }
    .empty { padding: 24px; }
    @media (max-width: 900px) { .search-hero, .main, .meta, .action-row, .results-head { grid-template-columns: 1fr; } }
  `]
})
export class UserSearchPageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly bookingApi = inject(BookingApiService);
  private readonly state = inject(SearchFlowService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  readonly busy = signal(false);
  readonly message = signal('Search live schedules from your backend.');
  readonly results = computed(() => this.state.searchResults());
  readonly form = this.fb.nonNullable.group({
    from: ['Bangalore', Validators.required],
    to: ['Chennai', Validators.required],
    date: [new Date().toISOString().slice(0, 10), Validators.required]
  });

  search(): void {
    if (this.form.invalid) {
      this.toast.error('Please choose source, destination, and date.');
      return;
    }

    const value = this.form.getRawValue();
    this.busy.set(true);
    this.bookingApi.search(value.from, value.to, value.date).subscribe({
      next: results => {
        this.state.setResults(results);
        this.message.set(`Fetched ${results.length} schedule(s).`);
        this.toast.success(`Found ${results.length} bus option(s).`);
        this.busy.set(false);
      },
      error: error => {
        const message = formatApiError(error, 'Search failed.');
        this.message.set(message);
        this.toast.error(message);
        this.busy.set(false);
      }
    });
  }

  book(item: ScheduleSearchResponse): void {
    this.state.setSelectedSchedule(item);
    void this.router.navigate(['/user/book', item.scheduleId], {
      queryParams: { busId: item.busId }
    });
  }

  duration(totalMinutes: number): string {
    const hours = Math.floor(totalMinutes / 60);
    const minutes = totalMinutes % 60;
    return `${hours}h ${minutes}m`;
  }
}
