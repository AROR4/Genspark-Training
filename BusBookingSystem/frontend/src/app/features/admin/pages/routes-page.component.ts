import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AdminApiService, AdminRouteResponse, CitiesApiService, type CityOption } from '../../../core/api';
import { formatApiError } from '../../../core/utils/api-error.util';
import { ToastService } from '../../../core/utils/toast.service';

@Component({
  selector: 'app-routes-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <section class="page">
      <header class="topbar">
        <div>
          <span class="eyebrow">Routes</span>
          <h1>Route library</h1>
          <p>Manage your route catalog and add new city pairs.</p>
        </div>
        <button type="button" class="primary" (click)="openModal()">+ Add Route</button>
      </header>

      <div class="cards">
        @for (route of routes(); track route.routeId) {
          <article class="card">
            <strong>{{ route.sourceCityName }} → {{ route.destinationCityName }}</strong>
            <small>Route ID {{ route.routeId }}</small>
          </article>
        } @empty {
          <article class="empty">No routes loaded yet.</article>
        }
      </div>

      @if (showModal()) {
        <div class="modal-backdrop" (click)="closeModal()">
          <section class="modal" (click)="$event.stopPropagation()">
            <h2>Add Route</h2>
            <form [formGroup]="form" (ngSubmit)="submit()" class="form">
              <label><span>Source City</span><input formControlName="sourceCityName" list="admin-route-city-list" /></label>
              <label><span>Destination City</span><input formControlName="destinationCityName" list="admin-route-city-list" /></label>

              <datalist id="admin-route-city-list">
                @for (city of cityOptions(); track city.id + '-' + city.name) {
                  <option [value]="city.name"></option>
                }
              </datalist>

              <div class="modal-actions">
                <button type="button" class="ghost" (click)="closeModal()">Cancel</button>
                <button type="submit" class="primary" [disabled]="busy()">Save Route</button>
              </div>
            </form>
          </section>
        </div>
      }
    </section>
  `,
  styles: [`
    .page { display: grid; gap: 18px; color: #e2e8f0; }
    .topbar { display: flex; align-items: center; justify-content: space-between; gap: 12px; }
    .eyebrow { display: inline-block; margin-bottom: 8px; color: #60a5fa; text-transform: uppercase; letter-spacing: 0.18em; font-size: 0.75rem; }
    h1, p { margin: 0; }
    p, small, span { color: #94a3b8; }
    .primary, .ghost {
      min-height: 42px;
      padding: 0 14px;
      border: 0;
      border-radius: 12px;
      color: #fff;
      cursor: pointer;
    }
    .primary { background: linear-gradient(135deg, #2563eb, #4f46e5); }
    .ghost { background: rgba(148,163,184,0.16); }
    .cards { display: grid; gap: 12px; grid-template-columns: repeat(3, minmax(0,1fr)); }
    .card, .empty, .modal {
      padding: 18px;
      border-radius: 18px;
      background: rgba(15,23,42,0.8);
      border: 1px solid rgba(148,163,184,0.12);
    }
    .card { display: grid; gap: 6px; }
    .empty { color: #94a3b8; }
    .modal-backdrop {
      position: fixed;
      inset: 0;
      background: rgba(2,6,23,0.72);
      display: grid;
      place-items: center;
      padding: 16px;
      z-index: 20;
    }
    .modal { width: min(520px, 100%); display: grid; gap: 16px; }
    .form { display: grid; gap: 12px; }
    label { display: grid; gap: 8px; }
    input {
      min-height: 44px;
      padding: 0 12px;
      border-radius: 12px;
      border: 1px solid rgba(148,163,184,0.18);
      background: rgba(2,6,23,0.6);
      color: #fff;
      font: inherit;
    }
    .modal-actions { display: flex; justify-content: end; gap: 10px; }
    @media (max-width: 980px) { .cards { grid-template-columns: 1fr; } .topbar { align-items: start; flex-direction: column; } }
  `]
})
export class RoutesPageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly adminApi = inject(AdminApiService);
  private readonly citiesApi = inject(CitiesApiService);
  private readonly toast = inject(ToastService);

  readonly routes = signal<AdminRouteResponse[]>([]);
  readonly cityOptions = signal<CityOption[]>([]);
  readonly showModal = signal(false);
  readonly busy = signal(false);
  readonly form = this.fb.nonNullable.group({
    sourceCityName: ['', Validators.required],
    destinationCityName: ['', Validators.required]
  });

  ngOnInit(): void {
    this.citiesApi.listOptions().subscribe({
      next: cities => this.cityOptions.set(cities),
      error: () => this.cityOptions.set([])
    });

    this.load();
  }

  load(): void {
    this.adminApi.listRoutes().subscribe({
      next: routes => this.routes.set(routes),
      error: error => this.toast.error(formatApiError(error, 'Unable to load routes.'))
    });
  }

  openModal(): void {
    this.showModal.set(true);
  }

  closeModal(): void {
    this.showModal.set(false);
  }

  submit(): void {
    if (this.form.invalid) {
      this.toast.error('Please enter both source and destination city.');
      return;
    }

    const value = this.form.getRawValue();
    const sourceCityId = this.cityIdFor(value.sourceCityName);
    const destinationCityId = this.cityIdFor(value.destinationCityName);

    if (sourceCityId == null || destinationCityId == null) {
      this.toast.error('Please select source and destination from city suggestions.');
      return;
    }

    this.busy.set(true);
    this.adminApi.addRoute({ sourceCityId, destinationCityId }).subscribe({
      next: () => {
        this.busy.set(false);
        this.toast.success('Route added successfully.');
        this.form.reset({ sourceCityName: '', destinationCityName: '' });
        this.showModal.set(false);
        this.load();
      },
      error: error => {
        this.busy.set(false);
        this.toast.error(formatApiError(error, 'Unable to add route.'));
      }
    });
  }

  private cityIdFor(cityName: string): number | null {
    const normalized = cityName.trim().toLowerCase();
    const match = this.cityOptions().find(item => item.name.toLowerCase() === normalized);

    if (!match || match.id < 0) {
      return null;
    }

    return match.id;
  }
}
