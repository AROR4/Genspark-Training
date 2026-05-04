import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { OperatorApiService } from '../../../core/api';
import { formatApiError } from '../../../core/utils/api-error.util';
import { ToastService } from '../../../core/utils/toast.service';

@Component({
  selector: 'app-operator-revenue-page',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="page">
      <header class="topbar">
        <div>
          <span class="eyebrow">Revenue</span>
          <h1>Revenue Overview</h1>
          <p>Track earnings and performance.</p>
        </div>
      </header>

      @if (loading()) {
        <div class="card muted">Loading revenue...</div>
      } @else {
        <div class="stats-grid">
          <article class="card">
            <span>Total Revenue</span>
            <strong>{{ formatCurrency(totalRevenue()) }}</strong>
          </article>

          <article class="card">
            <span>Total Tickets Sold</span>
            <strong>{{ totalTickets() }}</strong>
          </article>

          <article class="card">
            <span>Total Bookings</span>
            <strong>{{ totalBookings() }}</strong>
          </article>

          <article class="card">
            <span>Total Cancelled</span>
            <strong>{{ totalCancelled() }}</strong>
          </article>
        </div>
      }
    </section>
  `,
  styles: [`
    .page { display: grid; gap: 20px; color: #e2e8f0; }
    .stats-grid {
      display: grid;
      gap: 14px;
      grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
    }

    .card {
      padding: 20px;
      border-radius: 16px;
      background: #020617;
      border: 1px solid rgba(148,163,184,0.14);
      display: grid;
      gap: 8px;
    }
    .card span {
      color: #94a3b8;
      font-size: 0.92rem;
    }
    .card strong {
      color: #f8fafc;
      font-size: 1.3rem;
    }
    .muted {
      color: #94a3b8;
    }
  `]
})
export class OperatorRevenuePageComponent {
  private readonly api = inject(OperatorApiService);
  private readonly toast = inject(ToastService);

  readonly loading = signal(false);
  readonly totalRevenue = signal(0);
  readonly totalTickets = signal(0);
  readonly totalBookings = signal(0);
  readonly totalCancelled = signal(0);

  ngOnInit(): void {
    this.loading.set(true);
    this.api.revenue().subscribe({
      next: data => {
        this.loading.set(false);
        this.totalRevenue.set(data.totalRevenue ?? 0);
        this.totalTickets.set(data.totalTickets ?? 0);
        this.totalBookings.set(data.totalBookings ?? 0);
        this.totalCancelled.set(data.totalCancelled ?? 0);
      },
      error: error => {
        this.loading.set(false);
        this.toast.error(formatApiError(error, 'Failed to load operator revenue.'));
      }
    });
  }

  formatCurrency(value: number): string {
    return `₹${value.toLocaleString('en-IN')}`;
  }
}