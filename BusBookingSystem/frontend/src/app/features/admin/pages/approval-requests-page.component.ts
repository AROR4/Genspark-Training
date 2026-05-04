import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { AdminApiService, OperatorRequestResponse } from '../../../core/api';
import { formatApiError } from '../../../core/utils/api-error.util';
import { formatOperatorOffices } from '../../../core/utils/operator-office.util';
import { ToastService } from '../../../core/utils/toast.service';

@Component({
  selector: 'app-approval-requests-page',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="page">
      <header class="topbar">
        <div>
          <span class="eyebrow">Approval Requests</span>
          <h1>Pending operators</h1>
          <p>Review and approve operator registrations before they go live.</p>
        </div>
      </header>

      <div class="table" aria-live="polite">
        @for (item of requests(); track item.operatorId) {
          <article class="row" (click)="openDetails(item.operatorId)">
            <div>
              <strong>{{ item.ownerName || item.companyName }}</strong>
              <span>{{ item.companyName || item.legalName }}</span>
            </div>
            <div class="details">
              <span>{{ item.email }}</span>
              <small>{{ formatOffice(item) }}</small>
            </div>
            <div class="actions">
              <button
                type="button"
                class="view"
                [disabled]="loadingDetailId() === item.operatorId"
                (click)="$event.stopPropagation(); openDetails(item.operatorId)">
                View
              </button>
              <button
                type="button"
                class="approve"
                [disabled]="busyId() === item.operatorId"
                (click)="$event.stopPropagation(); approve(item)">
                Approve
              </button>
              <button
                type="button"
                class="reject"
                [disabled]="busyId() === item.operatorId"
                (click)="$event.stopPropagation(); reject(item)">
                Reject
              </button>
            </div>
          </article>
        } @empty {
          <article class="empty">No pending approvals right now.</article>
        }
      </div>

      @if (selectedOperator()) {
        <div class="modal-backdrop" (click)="closeDetails()">
          <section class="modal" (click)="$event.stopPropagation()">
            <h2>Operator Request Details</h2>

            <div class="detail-grid">
              <div><span>Owner</span><strong>{{ selectedOperator()?.ownerName }}</strong></div>
              <div><span>Company</span><strong>{{ selectedOperator()?.companyName || selectedOperator()?.legalName }}</strong></div>
              <div><span>Email</span><strong>{{ selectedOperator()?.email }}</strong></div>
              <div><span>Phone</span><strong>{{ selectedOperator()?.phoneNumber }}</strong></div>
              <div><span>Contact Email</span><strong>{{ selectedOperator()?.contactEmail || '-' }}</strong></div>
              <div><span>Contact Phone</span><strong>{{ selectedOperator()?.contactPhone || '-' }}</strong></div>
              <div><span>Registration Number</span><strong>{{ selectedOperator()?.registrationNumber || '-' }}</strong></div>
              <div><span>Tax Number</span><strong>{{ selectedOperator()?.taxNumber || '-' }}</strong></div>
              <div><span>License Number</span><strong>{{ selectedOperator()?.licenseNumber || '-' }}</strong></div>
              <div><span>Status</span><strong>{{ selectedOperator()?.approvalStatus || '-' }}</strong></div>
              <div><span>Notes</span><strong>{{ selectedOperator()?.adminNotes || '-' }}</strong></div>
              <div><span>Offices</span><strong>{{ formatOffice(selectedOperator()!) }}</strong></div>
            </div>

            <div class="modal-actions">
              <button type="button" class="close" (click)="closeDetails()">Close</button>
            </div>
          </section>
        </div>
      }
    </section>
  `,
  styles: [`
    .page { display: grid; gap: 18px; color: #e2e8f0; }
    .topbar { display: flex; align-items: end; justify-content: space-between; gap: 12px; }
    .eyebrow { display: inline-block; margin-bottom: 8px; color: #60a5fa; text-transform: uppercase; letter-spacing: 0.18em; font-size: 0.75rem; }
    h1, p { margin: 0; }
    p, span, small { color: #94a3b8; }
    .table { display: grid; gap: 12px; }
    .row, .empty {
      display: grid;
      grid-template-columns: 1.2fr 1fr auto;
      gap: 14px;
      align-items: center;
      padding: 18px;
      border-radius: 18px;
      background: rgba(15,23,42,0.8);
      border: 1px solid rgba(148,163,184,0.12);
    }
    .row {
      cursor: pointer;
      transition: border-color 180ms ease, transform 180ms ease;
    }
    .row:hover {
      border-color: rgba(96,165,250,0.35);
      transform: translateY(-1px);
    }
    .details { display: grid; gap: 4px; }
    .actions { display: flex; gap: 10px; justify-content: end; }
    button {
      min-height: 42px;
      padding: 0 14px;
      border: 0;
      border-radius: 12px;
      color: #fff;
      cursor: pointer;
      transition: transform 180ms ease, opacity 180ms ease, background 180ms ease;
    }
    button:hover:not(:disabled) { transform: translateY(-1px); }
    button:disabled { opacity: 0.65; cursor: wait; }
    .view { background: rgba(148,163,184,0.28); }
    .approve { background: linear-gradient(135deg, #2563eb, #4f46e5); }
    .reject { background: rgba(239,68,68,0.9); }
    .empty { color: #94a3b8; justify-content: center; }

    .modal-backdrop {
      position: fixed;
      inset: 0;
      background: rgba(2,6,23,0.72);
      display: grid;
      place-items: center;
      padding: 16px;
      z-index: 30;
    }

    .modal {
      width: min(860px, 100%);
      max-height: 86dvh;
      overflow: auto;
      padding: 18px;
      border-radius: 18px;
      background: rgba(15,23,42,0.96);
      border: 1px solid rgba(148,163,184,0.2);
      display: grid;
      gap: 14px;
    }

    .detail-grid {
      display: grid;
      gap: 10px;
      grid-template-columns: repeat(2, minmax(0, 1fr));
    }

    .detail-grid div {
      display: grid;
      gap: 4px;
      padding: 10px;
      border-radius: 12px;
      background: rgba(30,41,59,0.72);
      border: 1px solid rgba(148,163,184,0.12);
    }

    .detail-grid span {
      color: #93c5fd;
      font-size: 0.78rem;
      letter-spacing: 0.06em;
      text-transform: uppercase;
    }
    .detail-grid strong { color: #f8fafc; }
    .modal-actions { display: flex; justify-content: end; }
    .close { background: rgba(148,163,184,0.22); }

    @media (max-width: 980px) {
      .row, .empty { grid-template-columns: 1fr; }
      .actions { justify-content: start; flex-wrap: wrap; }
      .detail-grid { grid-template-columns: 1fr; }
    }
  `]
})
export class ApprovalRequestsPageComponent {
  private readonly adminApi = inject(AdminApiService);
  private readonly toast = inject(ToastService);

  readonly requests = signal<OperatorRequestResponse[]>([]);
  readonly busyId = signal<number | null>(null);
  readonly loadingDetailId = signal<number | null>(null);
  readonly selectedOperator = signal<OperatorRequestResponse | null>(null);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.adminApi.listPendingOperators().subscribe({
      next: requests => this.requests.set(requests),
      error: error => this.toast.error(formatApiError(error, 'Unable to load pending operators.'))
    });
  }

  approve(item: OperatorRequestResponse): void {
    const adminNotes = window.prompt('Enter notes for approval:', 'not good') || '';

    if (!adminNotes.trim()) {
      this.toast.error('Approval note is required.');
      return;
    }

    this.busyId.set(item.operatorId);
    this.adminApi.approveOperator(item.operatorId, { adminNotes }).subscribe({
      next: () => {
        this.toast.success(`${item.companyName || item.ownerName} approved.`);
        this.busyId.set(null);
        this.closeDetails();
        this.load();
      },
      error: error => {
        this.busyId.set(null);
        this.toast.error(formatApiError(error, 'Unable to approve operator.'));
      }
    });
  }

  reject(item: OperatorRequestResponse): void {
    const adminNotes = window.prompt('Enter notes for rejection:', 'string') || '';

    if (!adminNotes.trim()) {
      this.toast.error('Rejection note is required.');
      return;
    }

    this.busyId.set(item.operatorId);
    this.adminApi.rejectOperator(item.operatorId, { adminNotes }).subscribe({
      next: () => {
        this.toast.success(`${item.companyName || item.ownerName} rejected.`);
        this.busyId.set(null);
        this.closeDetails();
        this.load();
      },
      error: error => {
        this.busyId.set(null);
        this.toast.error(formatApiError(error, 'Unable to reject operator.'));
      }
    });
  }

  openDetails(operatorId: number): void {
    this.loadingDetailId.set(operatorId);
    this.adminApi.getOperatorRequest(operatorId).subscribe({
      next: details => {
        this.loadingDetailId.set(null);
        this.selectedOperator.set(details);
      },
      error: error => {
        this.loadingDetailId.set(null);
        this.toast.error(formatApiError(error, 'Unable to load operator details.'));
      }
    });
  }

  closeDetails(): void {
    this.selectedOperator.set(null);
  }

  formatOffice(item: OperatorRequestResponse): string {
    return formatOperatorOffices(item.offices);
  }

}
