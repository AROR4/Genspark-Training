import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { AdminApiService, OperatorRequestResponse } from '../../../core/api';
import { formatApiError } from '../../../core/utils/api-error.util';
import { ToastService } from '../../../core/utils/toast.service';
import { AppHeaderComponent } from '../../../shared/layout/app-header.component';

@Component({
  selector: 'app-admin-dashboard-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, AppHeaderComponent],
  template: `
    <app-header />
    <main class="page">
      <section class="hero">
        <span class="eyebrow">Admin dashboard</span>
        <h1>Operator approvals</h1>
        <p>{{ message() }}</p>
      </section>

      <form class="toolbar" [formGroup]="form">
        <label><span>Status</span><input formControlName="status" /></label>
        <label><span>Admin notes</span><input formControlName="adminNotes" /></label>
        <button type="button" class="primary" (click)="load()">Refresh</button>
      </form>

      <section class="list">
        @for (item of requests(); track item.operatorId) {
          <article class="request">
            <div>
              <strong>{{ item.companyName || item.legalName }}</strong>
              <span>{{ item.ownerName }} · {{ item.email }} · {{ item.phoneNumber }}</span>
              <small>{{ item.approvalStatus }} · {{ item.offices?.[0]?.cityName || 'No city' }}</small>
            </div>
            <div class="actions">
              <button type="button" class="approve" (click)="approve(item.operatorId)">Approve</button>
              <button type="button" class="reject" (click)="reject(item.operatorId)">Reject</button>
            </div>
          </article>
        } @empty {
          <article class="request">No operator requests found.</article>
        }
      </section>
    </main>
  `,
  styles: [`
    .page { min-height: 100dvh; padding: 28px; color: #f8fafc; background: #050607; }
    .hero, .toolbar, .request { border: 1px solid rgba(255,255,255,0.08); border-radius: 24px; background: rgba(255,255,255,0.06); }
    .hero, .toolbar, .request { padding: 24px; }
    .eyebrow, p, label span, .request span, .request small { color: #94a3b8; }
    h1, p { margin: 0; }
    .toolbar, .list, .actions { display: grid; gap: 14px; }
    .toolbar { grid-template-columns: 180px 1fr auto; align-items: end; margin-top: 24px; }
    label { display: grid; gap: 8px; }
    input { min-height: 48px; padding: 0 14px; color: #fff; border: 1px solid rgba(255,255,255,0.08); border-radius: 14px; background: rgba(255,255,255,0.08); }
    .primary, .approve, .reject { min-height: 48px; padding: 0 14px; border: 0; border-radius: 14px; color: #fff; }
    .primary, .approve { background: #2563eb; }
    .reject { background: #ef4444; }
    .list { margin-top: 24px; }
    .request { display: grid; grid-template-columns: 1fr auto; gap: 16px; align-items: center; }
    .request span, .request small { display: block; margin-top: 4px; }
    .actions { grid-template-columns: auto auto; }
    @media (max-width: 900px) { .toolbar, .request { grid-template-columns: 1fr; } .actions { grid-template-columns: 1fr 1fr; } }
  `]
})
export class AdminDashboardPageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly adminApi = inject(AdminApiService);
  private readonly toast = inject(ToastService);

  readonly requests = signal<OperatorRequestResponse[]>([]);
  readonly message = signal('Fetching operator requests.');
  readonly form = this.fb.nonNullable.group({
    status: ['PENDING'],
    adminNotes: ['Reviewed from admin dashboard.']
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.adminApi.listOperatorRequests(this.form.controls.status.value).subscribe({
      next: requests => {
        this.requests.set(requests);
        this.message.set(`Loaded ${requests.length} request(s).`);
      },
      error: error => {
        const message = formatApiError(error, 'Failed to fetch requests.');
        this.message.set(message);
        this.toast.error(message);
      }
    });
  }

  approve(operatorId: number): void {
    this.adminApi.approveOperator(operatorId, { adminNotes: this.form.controls.adminNotes.value }).subscribe({
      next: () => {
        this.message.set(`Approved operator ${operatorId}.`);
        this.toast.success(`Approved operator ${operatorId}.`);
        this.load();
      },
      error: error => {
        const message = formatApiError(error, 'Approval failed.');
        this.message.set(message);
        this.toast.error(message);
      }
    });
  }

  reject(operatorId: number): void {
    this.adminApi.rejectOperator(operatorId, { adminNotes: this.form.controls.adminNotes.value }).subscribe({
      next: () => {
        this.message.set(`Rejected operator ${operatorId}.`);
        this.toast.success(`Rejected operator ${operatorId}.`);
        this.load();
      },
      error: error => {
        const message = formatApiError(error, 'Rejection failed.');
        this.message.set(message);
        this.toast.error(message);
      }
    });
  }
}
