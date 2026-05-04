import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { AdminApiService, AdminOperatorResponse } from '../../../core/api';
import { formatApiError } from '../../../core/utils/api-error.util';
import { normalizeOperatorOffices } from '../../../core/utils/operator-office.util';
import { ToastService } from '../../../core/utils/toast.service';

@Component({
  selector: 'app-manage-operators-page',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="page">
      <header class="topbar">
        <div>
          <span class="eyebrow">Manage Operators</span>
          <h1>Operator control</h1>
          <p>View details and manage approved operators.</p>
        </div>
      </header>

      <div class="list">
        @for (operator of operators(); track operator.id) {
          <article class="row">
            <div>
              <strong>{{ operator.companyName || operator.ownerName }}</strong>
              <span>{{ operator.ownerName }}</span>
            </div>

            <div>
              <span class="status"
                [class.active]="isApproved(operator)"
                [class.pending]="!isApproved(operator)">
                {{ isApproved(operator) ? 'Approved' : 'Pending' }}
              </span>
            </div>

            <div class="actions">
              <button class="view" (click)="view(operator)">View</button>

              <button
                class="danger"
                [disabled]="busyId() === operator.id || !isApproved(operator)"
                (click)="disable(operator)">
                Disable
              </button>
            </div>
          </article>
        } @empty {
          <article class="empty">No operators available.</article>
        }
      </div>

      <!-- VIEW MODAL -->
      @if (selectedOperator()) {
        <div class="modal-backdrop" (click)="close()">
          <div class="modal" (click)="$event.stopPropagation()">
            <h2>{{ selectedOperator()?.companyName }}</h2>

            <p><strong>Owner:</strong> {{ selectedOperator()?.ownerName }}</p>
            <p><strong>Email:</strong> {{ selectedOperator()?.contactEmail }}</p>
            <p><strong>Phone:</strong> {{ selectedOperator()?.contactPhone }}</p>
            <p><strong>Legal Name:</strong> {{ selectedOperator()?.legalName }}</p>
            <p><strong>License:</strong> {{ selectedOperator()?.licenseNumber }}</p>

            <div class="offices">
            <h3>Offices</h3>

            @if (operatorOffices().length) {
              @for (office of operatorOffices(); track office.id) {
                <div class="office">
                  <strong>{{ office.cityName }}</strong>
                  <span>{{ office.address }}</span>
                </div>
              }
            } @else {
              <p class="no-office">No offices available</p>
            }
          </div>

            <button class="close-btn" (click)="close()">Close</button>
          </div>
        </div>
      }
    </section>
  `,
  styles: [`
    .page { display: grid; gap: 18px; color: #e2e8f0; }
    .eyebrow { color: #60a5fa; font-size: 0.75rem; letter-spacing: 0.15em; }
    .list { display: grid; gap: 12px; }

    .row, .empty {
      display: grid;
      grid-template-columns: 1.2fr auto auto;
      gap: 14px;
      align-items: center;
      padding: 18px;
      border-radius: 18px;
      background: #0f172a;
      border: 1px solid rgba(148,163,184,0.12);
    }

    .status { padding: 6px 12px; border-radius: 999px; font-size: 0.85rem; }
    .status.active { background: rgba(34,197,94,0.15); color: #86efac; }
    .status.pending { background: rgba(251,191,36,0.15); color: #fde68a; }

    .actions { display: flex; gap: 10px; }
    button {
      padding: 8px 14px;
      border-radius: 10px;
      border: none;
      cursor: pointer;
      color: white;
    }

    .view { background: #3b82f6; }
    .danger { background: #ef4444; }

    .modal-backdrop {
      position: fixed;
      inset: 0;
      background: rgba(0,0,0,0.6);
      display: flex;
      justify-content: center;
      align-items: center;
    }

    .modal {
      background: #020617;
      padding: 24px;
      border-radius: 16px;
      width: 400px;
      color: white;
    }

    .office { margin-top: 10px; }

    .close-btn {
      margin-top: 20px;
      background: #334155;
    }
  `]
})
export class ManageOperatorsPageComponent {
  private readonly adminApi = inject(AdminApiService);
  private readonly toast = inject(ToastService);

  readonly operators = signal<AdminOperatorResponse[]>([]);
  readonly busyId = signal<number | null>(null);
  readonly selectedOperator = signal<AdminOperatorResponse | null>(null);
  readonly operatorOffices = signal<AdminOperatorResponse['offices']>([]);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.adminApi.listOperators().subscribe({
      next: operators => this.operators.set(operators),
      error: err => this.toast.error(formatApiError(err, 'Failed to load operators'))
    });
  }

  view(operator: AdminOperatorResponse): void {
    this.selectedOperator.set(operator);
    this.operatorOffices.set(normalizeOperatorOffices(operator.offices));
  }

  close(): void {
    this.selectedOperator.set(null);
    this.operatorOffices.set([]);
  }

  disable(operator: AdminOperatorResponse): void {
    if (!this.isApproved(operator)) {
      this.toast.error('Only approved operators can be disabled');
      return;
    }

    if (!window.confirm('Disable this operator?')) return;

    this.busyId.set(operator.id);

    this.adminApi.disableOperator(operator.id, 'Disabled by admin').subscribe({
      next: () => {
        this.toast.success('Operator disabled');
        this.busyId.set(null);
        this.load();
      },
      error: err => {
        this.busyId.set(null);
        this.toast.error(formatApiError(err, 'Disable failed'));
      }
    });
  }

  isApproved(operator: AdminOperatorResponse): boolean {
    return operator?.user?.isApproved === true;
  }
}
