import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CitiesApiService, CityOption } from '../../../core/api/cities-api.service';
import { OperatorApiService, OperatorOfficeRequest, OperatorOfficeResponse } from '../../../core/api';
import { formatApiError } from '../../../core/utils/api-error.util';
import { ToastService } from '../../../core/utils/toast.service';

@Component({
  selector: 'app-operator-offices-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <section class="page">
      <header class="topbar">
        <div>
          <span class="eyebrow">Manage Offices</span>
          <h1>Your Office Locations</h1>
          <p>Add boarding and operating offices for your company.</p>
        </div>

        <button class="primary" type="button" (click)="toggleForm()">
          {{ showForm() ? 'Close' : '+ Add Office' }}
        </button>
      </header>

      @if (showForm()) {
        <section class="form-card">
          <h3>Add New Office</h3>

          <label>
            <span>City</span>
            <select [(ngModel)]="form.cityId">
              <option [ngValue]="0">Select city</option>
              @for (city of cities(); track city.id) {
                <option [ngValue]="city.id">{{ city.name }}</option>
              }
            </select>
          </label>

          <label class="address-field">
            <span>Address</span>
            <textarea [(ngModel)]="form.address" rows="4" placeholder="Enter office address"></textarea>
          </label>

          <button class="primary submit" type="button" (click)="addOffice()" [disabled]="saving() || loadingCities()">
            {{ saving() ? 'Saving...' : 'Add Office' }}
          </button>
        </section>
      }

      <section class="list">
        @if (loadingOffices()) {
          <div class="empty">Loading offices...</div>
        } @else {
          @for (office of offices(); track office.id) {
            <article class="card">
              <div class="card-head">
                <div class="identity">
                  <strong>{{ office.cityName || 'City not available' }}</strong>
                  <span>{{ office.address || 'Address not available' }}</span>
                </div>
                <span class="office-id">Office #{{ office.id }}</span>
              </div>
            </article>
          } @empty {
            <div class="empty">No offices added yet.</div>
          }
        }
      </section>
    </section>
  `,
  styles: [`
    .page { display: grid; gap: 20px; color: #e2e8f0; }
    .topbar {
      display: flex;
      justify-content: space-between;
      align-items: start;
      gap: 16px;
    }
    .eyebrow {
      display: inline-block;
      margin-bottom: 6px;
      color: #93c5fd;
      font-size: 0.82rem;
      font-weight: 800;
      text-transform: uppercase;
      letter-spacing: 0.08em;
    }
    h1, h3, p { margin: 0; }
    p { color: #94a3b8; }
    .primary {
      padding: 10px 16px;
      border-radius: 12px;
      border: none;
      background: linear-gradient(135deg, #3b82f6, #2563eb);
      color: white;
      cursor: pointer;
      font: inherit;
      font-weight: 700;
    }
    .primary:disabled {
      opacity: 0.7;
      cursor: wait;
    }
    .form-card, .card {
      padding: 20px;
      border-radius: 18px;
      background: #020617;
      border: 1px solid rgba(148, 163, 184, 0.14);
      display: grid;
      gap: 14px;
    }
    .form-card {
      grid-template-columns: repeat(2, minmax(0, 1fr));
      align-items: end;
    }
    .form-card h3,
    .address-field,
    .submit {
      grid-column: 1 / -1;
    }
    label { display: grid; gap: 8px; }
    label span { color: #cbd5e1; font-size: 0.92rem; }
    select, textarea {
      min-height: 44px;
      padding: 10px 12px;
      border-radius: 10px;
      border: 1px solid rgba(148, 163, 184, 0.16);
      background: #0f172a;
      color: white;
      font: inherit;
    }
    textarea {
      resize: vertical;
      min-height: 108px;
    }
    .list { display: grid; gap: 14px; }
    .card-head {
      display: flex;
      justify-content: space-between;
      align-items: start;
      gap: 12px;
    }
    .identity {
      display: grid;
      gap: 6px;
    }
    .identity span { color: #94a3b8; }
    .office-id {
      padding: 6px 12px;
      border-radius: 999px;
      background: rgba(59, 130, 246, 0.16);
      border: 1px solid rgba(96, 165, 250, 0.24);
      color: #bfdbfe;
      font-weight: 700;
      white-space: nowrap;
    }
    .empty {
      text-align: center;
      color: #94a3b8;
      padding: 20px;
      border-radius: 16px;
      background: #020617;
      border: 1px solid rgba(148, 163, 184, 0.14);
    }
    @media (max-width: 860px) {
      .topbar, .card-head { flex-direction: column; }
      .form-card { grid-template-columns: 1fr; }
      .form-card h3,
      .address-field,
      .submit {
        grid-column: auto;
      }
    }
  `]
})
export class OperatorOfficesPageComponent {
  private readonly operatorApi = inject(OperatorApiService);
  private readonly citiesApi = inject(CitiesApiService);
  private readonly toast = inject(ToastService);

  readonly offices = signal<OperatorOfficeResponse[]>([]);
  readonly cities = signal<CityOption[]>([]);
  readonly showForm = signal(false);
  readonly saving = signal(false);
  readonly loadingCities = signal(true);
  readonly loadingOffices = signal(true);

  form: OperatorOfficeRequest = this.createEmptyForm();

  ngOnInit(): void {
    this.loadCities();
    this.loadOffices();
  }

  toggleForm(): void {
    this.showForm.set(!this.showForm());
  }

  loadCities(): void {
    this.loadingCities.set(true);
    this.citiesApi.listOptions().subscribe({
      next: cities => {
        this.cities.set(cities.filter(city => city.id > 0));
        this.loadingCities.set(false);
      },
      error: err => {
        this.loadingCities.set(false);
        this.toast.error(formatApiError(err, 'Failed to load cities'));
      }
    });
  }

  loadOffices(): void {
    this.loadingOffices.set(true);
    this.operatorApi.listOffices().subscribe({
      next: offices => {
        this.offices.set(offices);
        this.loadingOffices.set(false);
      },
      error: err => {
        this.loadingOffices.set(false);
        this.toast.error(formatApiError(err, 'Failed to load offices'));
      }
    });
  }

  addOffice(): void {
    if (!this.form.cityId || this.form.cityId < 1) {
      this.toast.error('City is required');
      return;
    }

    if (!this.form.address?.trim()) {
      this.toast.error('Address is required');
      return;
    }

    this.saving.set(true);

    this.operatorApi.addOffice({
      cityId: this.form.cityId,
      address: this.form.address.trim()
    }).subscribe({
      next: () => {
        this.saving.set(false);
        this.showForm.set(false);
        this.form = this.createEmptyForm();
        this.toast.success('Office added successfully');
        this.loadOffices();
      },
      error: err => {
        this.saving.set(false);
        this.toast.error(formatApiError(err, 'Failed to add office'));
      }
    });
  }

  private createEmptyForm(): OperatorOfficeRequest {
    return {
      cityId: 0,
      address: ''
    };
  }
}
