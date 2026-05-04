import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  BusResponse,
  CreateBusScheduleRequest,
  OperatorApiService,
  OperatorOfficeRequest,
  OperatorOfficeResponse,
  OperatorRouteResponse
} from '../../../core/api';
import { formatApiError } from '../../../core/utils/api-error.util';
import { ToastService } from '../../../core/utils/toast.service';

@Component({
  selector: 'app-operator-schedules-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <section class="page">
      <header class="topbar">
        <div>
          <span class="eyebrow">Schedules</span>
          <h1>Create Trips by Route</h1>
          <p>Route must have matching source and destination offices before schedule creation.</p>
        </div>
      </header>

      @if (loadingRoutes()) {
        <div class="card muted">Loading routes...</div>
      } @else {
        <div class="routes-grid">
          @for (route of routes(); track route.routeId) {
            <article class="route-card" [class.active]="selectedRouteId() === route.routeId">
              <div>
                <strong>{{ route.sourceCityName }} → {{ route.destinationCityName }}</strong>
                <small>Route #{{ route.routeId }}</small>
              </div>

              @if (route.canCreateSchedule) {
                <button class="primary" type="button" (click)="openScheduleForm(route)">Create Schedule</button>
              } @else {
                <div class="missing-wrap">
                  <span class="warning">Missing office in: {{ route.missingCityName || 'Required city' }}</span>
                  <button class="secondary" type="button" (click)="startAddOffice(route)">Add Office</button>
                </div>
              }

              @if (addOfficeRouteId() === route.routeId) {
                <form class="inline-form" [formGroup]="officeForm" (ngSubmit)="submitOffice()">
                  <label>
                    <span>City ID (pre-filled)</span>
                    <input type="number" formControlName="cityId" />
                  </label>
                  <label>
                    <span>Office Address</span>
                    <input type="text" formControlName="address" placeholder="Enter office address" />
                  </label>
                  <button class="primary" type="submit" [disabled]="addingOffice()">
                    {{ addingOffice() ? 'Adding...' : 'Save Office' }}
                  </button>
                </form>
              }
            </article>
          } @empty {
            <div class="card muted">No routes available for this operator.</div>
          }
        </div>
      }

      @if (selectedRoute()) {
        <section class="card schedule-card">
          <h2>Create Schedule for {{ selectedRoute()!.sourceCityName }} → {{ selectedRoute()!.destinationCityName }}</h2>

          <form class="schedule-form" [formGroup]="scheduleForm" (ngSubmit)="submitSchedule()">
            <label>
              <span>Bus</span>
              <select formControlName="busId">
                @for (bus of buses(); track bus.busId) {
                  <option [value]="bus.busId">Bus #{{ bus.busId }} ({{ bus.totalSeats }} seats)</option>
                }
              </select>
            </label>

            <label>
              <span>Source Office</span>
              <select formControlName="sourceOfficeId">
                <option [value]="0">Select source office</option>
                @for (office of sourceOffices(); track office.id) {
                  <option [value]="office.id">{{ office.cityName }} - {{ office.address }}</option>
                }
              </select>
            </label>

            <label>
              <span>Destination Office</span>
              <select formControlName="destinationOfficeId">
                <option [value]="0">Select destination office</option>
                @for (office of destinationOffices(); track office.id) {
                  <option [value]="office.id">{{ office.cityName }} - {{ office.address }}</option>
                }
              </select>
            </label>

            <label>
              <span>Travel Date</span>
              <input type="date" formControlName="travelDate" />
            </label>

            <label>
              <span>Departure Time</span>
              <input type="time" formControlName="departureTime" />
            </label>

            <label>
              <span>Duration (minutes)</span>
              <input type="number" min="1" formControlName="durationMinutes" />
            </label>

            <label>
              <span>Base Price</span>
              <input type="number" min="1" formControlName="basePrice" />
            </label>

            <button class="primary" type="submit" [disabled]="creatingSchedule()">
              {{ creatingSchedule() ? 'Creating...' : 'Create Schedule' }}
            </button>
          </form>
        </section>
      }
    </section>
  `,
  styles: [`
    .page { display: grid; gap: 20px; color: #e2e8f0; }

    .topbar {
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      gap: 16px;
    }

    .routes-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
      gap: 14px;
    }

    .route-card {
      padding: 16px;
      border-radius: 14px;
      background: #020617;
      border: 1px solid rgba(148,163,184,0.14);
      display: grid;
      gap: 12px;
    }

    .route-card.active {
      border-color: rgba(59,130,246,0.45);
      box-shadow: inset 0 0 0 1px rgba(59,130,246,0.25);
    }

    .route-card small {
      color: #94a3b8;
    }

    .primary {
      padding: 10px 16px;
      border-radius: 10px;
      background: #3b82f6;
      border: none;
      color: white;
      cursor: pointer;
    }

    .secondary {
      padding: 10px 16px;
      border-radius: 10px;
      border: 1px solid rgba(148,163,184,0.35);
      background: #0f172a;
      color: #cbd5e1;
      cursor: pointer;
    }

    .missing-wrap {
      display: grid;
      gap: 10px;
    }

    .warning {
      color: #fbbf24;
      font-size: 0.92rem;
    }

    .inline-form {
      display: grid;
      gap: 10px;
      padding: 12px;
      border-radius: 12px;
      border: 1px solid rgba(148,163,184,0.16);
      background: #0b1220;
    }

    .inline-form label,
    .schedule-form label {
      display: grid;
      gap: 6px;
    }

    .inline-form span,
    .schedule-form span {
      color: #94a3b8;
      font-size: 0.9rem;
    }

    .schedule-card h2 {
      margin: 0 0 12px;
      font-size: 1.05rem;
    }

    .schedule-form {
      display: grid;
      gap: 12px;
      grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
      align-items: end;
    }

    .card {
      padding: 20px;
      border-radius: 16px;
      background: #020617;
      border: 1px solid rgba(148,163,184,0.14);
    }

    .muted {
      color: #94a3b8;
    }

    input,
    select {
      min-height: 42px;
      border-radius: 10px;
      border: 1px solid rgba(148,163,184,0.2);
      background: #0f172a;
      color: #f8fafc;
      padding: 0 12px;
    }
  `]
})
export class OperatorSchedulesPageComponent {
  private readonly api = inject(OperatorApiService);
  private readonly toast = inject(ToastService);
  private readonly fb = inject(FormBuilder);

  readonly routes = signal<OperatorRouteResponse[]>([]);
  readonly buses = signal<BusResponse[]>([]);
  readonly offices = signal<OperatorOfficeResponse[]>([]);

  readonly loadingRoutes = signal(false);
  readonly addingOffice = signal(false);
  readonly creatingSchedule = signal(false);

  readonly selectedRouteId = signal<number | null>(null);
  readonly addOfficeRouteId = signal<number | null>(null);

  readonly officeForm = this.fb.nonNullable.group({
    cityId: [0, [Validators.required, Validators.min(1)]],
    address: ['', [Validators.required, Validators.maxLength(180)]]
  });

  readonly scheduleForm = this.fb.nonNullable.group({
    busId: [0, [Validators.required, Validators.min(1)]],
    sourceOfficeId: [0, [Validators.required, Validators.min(1)]],
    destinationOfficeId: [0, [Validators.required, Validators.min(1)]],
    travelDate: ['', Validators.required],
    departureTime: ['', Validators.required],
    durationMinutes: [480, [Validators.required, Validators.min(1)]],
    basePrice: [1200, [Validators.required, Validators.min(1)]]
  });

  readonly selectedRoute = computed(() => {
    const id = this.selectedRouteId();
    return this.routes().find(route => route.routeId === id) ?? null;
  });

  readonly sourceOffices = computed(() => {
    const route = this.selectedRoute();
    if (!route) return [];
    return this.filterOfficesForCity(route.sourceCityId ?? null, route.sourceCityName);
  });

  readonly destinationOffices = computed(() => {
    const route = this.selectedRoute();
    if (!route) return [];
    return this.filterOfficesForCity(route.destinationCityId ?? null, route.destinationCityName);
  });

  ngOnInit(): void {
    this.loadRoutes();
    this.loadBuses();
  }

  openScheduleForm(route: OperatorRouteResponse): void {
    this.selectedRouteId.set(route.routeId);
    this.addOfficeRouteId.set(null);
    this.scheduleForm.patchValue({
      sourceOfficeId: 0,
      destinationOfficeId: 0
    });
    this.loadOffices();
  }

  startAddOffice(route: OperatorRouteResponse): void {
    const cityId = route.missingCityId ?? 0;
    this.addOfficeRouteId.set(route.routeId);
    this.officeForm.patchValue({
      cityId,
      address: ''
    });

    if (!cityId) {
      this.toast.error('Missing cityId in route response. Cannot prefill office city.');
    }
  }

  submitOffice(): void {
    if (this.officeForm.invalid) {
      this.toast.error('City and address are required to add office.');
      return;
    }

    this.addingOffice.set(true);

    const payload: OperatorOfficeRequest = this.officeForm.getRawValue();

    this.api.addOffice(payload).subscribe({
      next: () => {
        this.addingOffice.set(false);
        this.addOfficeRouteId.set(null);
        this.officeForm.reset({ cityId: 0, address: '' });
        this.toast.success('Office added successfully.');
        this.loadRoutes();
        this.loadOffices();
      },
      error: err => {
        this.addingOffice.set(false);
        this.toast.error(formatApiError(err, 'Failed to add office.'));
      }
    });
  }

  submitSchedule(): void {
    const route = this.selectedRoute();
    if (!route) {
      this.toast.error('Select a route to create schedule.');
      return;
    }

    if (this.scheduleForm.invalid) {
      this.toast.error('Please fill all schedule fields.');
      return;
    }

    this.creatingSchedule.set(true);

    const raw = this.scheduleForm.getRawValue();
    const payload: CreateBusScheduleRequest = {
      busId: raw.busId,
      routeId: route.routeId,
      sourceOfficeId: raw.sourceOfficeId,
      destinationOfficeId: raw.destinationOfficeId,
      travelDate: raw.travelDate,
      departureTime: raw.departureTime,
      durationMinutes: raw.durationMinutes,
      basePrice: raw.basePrice
    };

    this.api.createSchedule(payload).subscribe({
      next: () => {
        this.creatingSchedule.set(false);
        this.toast.success('Schedule created successfully.');
        this.scheduleForm.patchValue({
          sourceOfficeId: 0,
          destinationOfficeId: 0,
          travelDate: '',
          departureTime: ''
        });
      },
      error: err => {
        this.creatingSchedule.set(false);
        this.toast.error(formatApiError(err, 'Failed to create schedule.'));
      }
    });
  }

  private loadRoutes(): void {
    this.loadingRoutes.set(true);
    this.api.listRoutes().subscribe({
      next: routes => {
        this.loadingRoutes.set(false);
        this.routes.set(routes);

        const selectedId = this.selectedRouteId();
        if (selectedId && !routes.some(route => route.routeId === selectedId && route.canCreateSchedule)) {
          this.selectedRouteId.set(null);
        }
      },
      error: err => {
        this.loadingRoutes.set(false);
        this.toast.error(formatApiError(err, 'Failed to load routes.'));
      }
    });
  }

  private loadOffices(): void {
    this.api.listOffices().subscribe({
      next: offices => {
        this.offices.set(offices);
      },
      error: err => {
        this.toast.error(formatApiError(err, 'Failed to load operator offices.'));
      }
    });
  }

  private loadBuses(): void {
    this.api.listBuses().subscribe({
      next: buses => {
        this.buses.set(buses);
        if (buses.length > 0) {
          this.scheduleForm.patchValue({ busId: buses[0].busId });
        }
      },
      error: err => {
        this.toast.error(formatApiError(err, 'Failed to load buses.'));
      }
    });
  }

  private filterOfficesForCity(cityId: number | null, cityName: string | null): OperatorOfficeResponse[] {
    const offices = this.offices();

    if (cityId && offices.some(office => office.cityId != null)) {
      return offices.filter(office => office.cityId === cityId);
    }

    const normalizedCityName = this.normalizeCityName(cityName);
    if (!normalizedCityName) {
      return [];
    }

    return offices.filter(office => this.normalizeCityName(office.cityName) === normalizedCityName);
  }

  private normalizeCityName(value: string | null | undefined): string {
    return (value ?? '').trim().toLowerCase();
  }
}
