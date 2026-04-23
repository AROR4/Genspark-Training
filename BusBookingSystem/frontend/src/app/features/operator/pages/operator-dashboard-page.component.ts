import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { OperatorApiService } from '../../../core/api';
import { formatApiError } from '../../../core/utils/api-error.util';
import { ToastService } from '../../../core/utils/toast.service';
import { AppHeaderComponent } from '../../../shared/layout/app-header.component';

@Component({
  selector: 'app-operator-dashboard-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, AppHeaderComponent],
  template: `
    <app-header />
    <main class="page">
      <section class="hero">
        <span class="eyebrow">Operator dashboard</span>
        <h1>Manage buses and schedules</h1>
        <p>{{ message() }}</p>
      </section>

      <section class="grid">
        <form class="card" [formGroup]="busForm" (ngSubmit)="createBus()">
          <h2>Create bus</h2>
          <label><span>Total seats</span><input type="number" formControlName="totalSeats" /></label>
          <label><span>Layout JSON</span><input formControlName="layoutJson" /></label>
          <label><span>Is active</span><input formControlName="isActive" /></label>
          <button class="primary" type="submit">Save bus</button>
        </form>

        <form class="card" [formGroup]="scheduleForm" (ngSubmit)="createSchedule()">
          <h2>Create schedule</h2>
          <label><span>Bus ID</span><input type="number" formControlName="busId" /></label>
          <label><span>From</span><input formControlName="sourceCityName" /></label>
          <label><span>To</span><input formControlName="destinationCityName" /></label>
          <label><span>Date</span><input type="date" formControlName="travelDate" /></label>
          <label><span>Departure time</span><input type="time" formControlName="departureTime" /></label>
          <label><span>Duration minutes</span><input type="number" formControlName="durationMinutes" /></label>
          <label><span>Base price</span><input type="number" formControlName="basePrice" /></label>
          <button class="primary" type="submit">Save schedule</button>
        </form>
      </section>

      <section class="lists">
        <div class="card">
          <h2>Buses</h2>
          @for (bus of buses(); track bus.busId) {
            <article class="row">
              <strong>Bus {{ bus.busId }}</strong>
              <span>{{ bus.totalSeats }} seats · {{ bus.isActive ? 'Active' : 'Inactive' }}</span>
            </article>
          } @empty {
            <article class="row">No buses returned.</article>
          }
        </div>

        <div class="card">
          <h2>Schedules</h2>
          @for (schedule of schedules(); track schedule.scheduleId) {
            <article class="row">
              <strong>{{ schedule.sourceCityName }} to {{ schedule.destinationCityName }}</strong>
              <span>{{ schedule.travelDate }} · {{ schedule.departureTime }} · Rs {{ schedule.basePrice }}</span>
            </article>
          } @empty {
            <article class="row">No schedules returned.</article>
          }
        </div>
      </section>
    </main>
  `,
  styles: [`
    .page { min-height: 100dvh; padding: 28px; color: #f8fafc; background: #050607; }
    .hero, .card { border: 1px solid rgba(255,255,255,0.08); border-radius: 24px; background: rgba(255,255,255,0.06); }
    .hero, .card { padding: 24px; }
    .eyebrow, p, label span, .row span { color: #94a3b8; }
    h1, h2, p { margin: 0; }
    .grid, .lists { display: grid; gap: 18px; margin-top: 24px; }
    .grid { grid-template-columns: repeat(2, minmax(0,1fr)); }
    .lists { grid-template-columns: repeat(2, minmax(0,1fr)); }
    form { display: grid; gap: 12px; }
    label { display: grid; gap: 8px; }
    input { min-height: 48px; padding: 0 14px; color: #fff; border: 1px solid rgba(255,255,255,0.08); border-radius: 14px; background: rgba(255,255,255,0.08); }
    .primary { min-height: 50px; border: 0; border-radius: 14px; color: #fff; background: #ef4444; }
    .row { display: grid; gap: 4px; padding: 14px 0; border-top: 1px solid rgba(255,255,255,0.06); }
    @media (max-width: 900px) { .grid, .lists { grid-template-columns: 1fr; } }
  `]
})
export class OperatorDashboardPageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly operatorApi = inject(OperatorApiService);
  private readonly toast = inject(ToastService);

  readonly message = signal('Fetching operator buses and schedules.');
  readonly buses = signal<any[]>([]);
  readonly schedules = signal<any[]>([]);

  readonly busForm = this.fb.nonNullable.group({
    totalSeats: [36, Validators.required],
    layoutJson: [''],
    isActive: ['true', Validators.required]
  });

  readonly scheduleForm = this.fb.nonNullable.group({
    busId: [1, Validators.required],
    sourceCityName: ['Bangalore', Validators.required],
    destinationCityName: ['Chennai', Validators.required],
    travelDate: [new Date().toISOString().slice(0, 10), Validators.required],
    departureTime: ['21:30', Validators.required],
    durationMinutes: [390, Validators.required],
    basePrice: [1200, Validators.required]
  });

  ngOnInit(): void {
    this.refresh();
  }

  refresh(): void {
    this.operatorApi.listBuses().subscribe({
      next: buses => this.buses.set(buses),
      error: error => {
        const message = formatApiError(error, 'Failed to load buses.');
        this.message.set(message);
        this.toast.error(message);
      }
    });
    this.operatorApi.listSchedules().subscribe({
      next: schedules => this.schedules.set(schedules),
      error: error => {
        const message = formatApiError(error, 'Failed to load schedules.');
        this.message.set(message);
        this.toast.error(message);
      }
    });
  }

  createBus(): void {
    const value = this.busForm.getRawValue();
    this.operatorApi.createBus({
      totalSeats: Number(value.totalSeats),
      layoutJson: value.layoutJson || null,
      isActive: String(value.isActive) === 'true'
    }).subscribe({
      next: () => {
        this.message.set('Bus created successfully.');
        this.toast.success('Bus created successfully.');
        this.refresh();
      },
      error: error => {
        const message = formatApiError(error, 'Bus creation failed.');
        this.message.set(message);
        this.toast.error(message);
      }
    });
  }

  createSchedule(): void {
    const value = this.scheduleForm.getRawValue();
    this.operatorApi.createSchedule({
      busId: Number(value.busId),
      sourceCityName: value.sourceCityName,
      destinationCityName: value.destinationCityName,
      travelDate: value.travelDate,
      departureTime: value.departureTime,
      durationMinutes: Number(value.durationMinutes),
      basePrice: Number(value.basePrice)
    }).subscribe({
      next: () => {
        this.message.set('Schedule created successfully.');
        this.toast.success('Schedule created successfully.');
        this.refresh();
      },
      error: error => {
        const message = formatApiError(error, 'Schedule creation failed.');
        this.message.set(message);
        this.toast.error(message);
      }
    });
  }
}
