import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { AdminApiService } from '../../../core/api';
import { formatApiError } from '../../../core/utils/api-error.util';
import { ToastService } from '../../../core/utils/toast.service';

@Component({
  selector: 'app-admin-revenue-page',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="page">
      <header class="topbar">
        <div>
          <span class="eyebrow">Revenue</span>
          <h1>Platform Revenue</h1>
          <p>Track admin share from confirmed bookings.</p>
        </div>
      </header>

      @if (loading()) {
        <div class="card muted">Loading revenue...</div>
      } @else {
        <section class="stats-grid">
          <article class="card">
            <span>Platform Revenue</span>
            <strong>{{ formatCurrency(adminRevenue()) }}</strong>
          </article>
          <article class="card">
            <span>Total Bookings</span>
            <strong>{{ totalBookings() }}</strong>
          </article>
          <article class="card">
            <span>Total Cancelled</span>
            <strong>{{ totalCancelled() }}</strong>
          </article>
        </section>
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
export class AdminRevenuePageComponent {
  private readonly adminApi = inject(AdminApiService);
  private readonly toast = inject(ToastService);

  readonly loading = signal(false);
  readonly adminRevenue = signal(0);
  readonly totalBookings = signal(0);
  readonly totalCancelled = signal(0);

  ngOnInit(): void {
    this.loading.set(true);
    this.adminApi.revenue().subscribe({
      next: data => {
        this.loading.set(false);
        this.adminRevenue.set(data.adminRevenue ?? 0);
        this.totalBookings.set(data.totalBookings ?? 0);
        this.totalCancelled.set(data.totalCancelled ?? 0);
      },
      error: error => {
        this.loading.set(false);
        this.toast.error(formatApiError(error, 'Failed to load admin revenue.'));
      }
    });
  }

  formatCurrency(value: number): string {
    return `₹${value.toLocaleString('en-IN')}`;
  }
}
