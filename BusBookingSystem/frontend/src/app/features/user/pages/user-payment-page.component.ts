import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { BookingApiService, BookingResponse, CreateBookingRequest } from '../../../core/api';
import { SearchFlowService } from '../../../core/state/search-flow.service';
import { formatApiError } from '../../../core/utils/api-error.util';
import { ToastService } from '../../../core/utils/toast.service';
import { AppHeaderComponent } from '../../../shared/layout/app-header.component';
import { environment } from '../../../../environments/environment';

declare global {
  interface Window {
    Razorpay?: new (options: Record<string, unknown>) => {
      open: () => void;
      on: (eventName: string, handler: (response: unknown) => void) => void;
    };
  }
}

@Component({
  selector: 'app-user-payment-page',
  standalone: true,
  imports: [CommonModule, AppHeaderComponent],
  template: `
    <app-header />
    <main class="page">
      <section class="card">
        <div class="topline">
          <div>
            <p class="eyebrow">Step 3 of 3</p>
            <h1>Payment</h1>
            <p>Seats are held for a limited time after checkout starts.</p>
          </div>

          <div class="timer" [class.expired]="remainingSeconds() <= 0">
            <span>Hold Timer</span>
            <strong>{{ countdown() }}</strong>
          </div>
        </div>

        <div class="summary">
          <div><small>Route</small><strong>{{ routeLabel() }}</strong></div>
          <div><small>Seats</small><strong>{{ seatCount() }}</strong></div>
          <div><small>Amount</small><strong>&#8377;{{ formatAmount(amount()) }}</strong></div>
          <div><small>Order ID</small><strong>{{ orderId() || 'Generating...' }}</strong></div>
        </div>

        @if (fareSummary()) {
          <section class="fare-card">
            <div class="fare-header">
              <h2>Fare Summary</h2>
              <span>Backend pricing</span>
            </div>

            <div class="fare-row">
              <span>{{ baseFareLabel() }}</span>
              <strong>&#8377;{{ formatAmount(fareSummary()?.baseAmount || 0) }}</strong>
            </div>

            <div class="fare-row">
              <span>GST (5%)</span>
              <strong>&#8377;{{ formatAmount(fareSummary()?.gstAmount || 0) }}</strong>
            </div>

            <div class="fare-row">
              <span>Convenience Fee</span>
              <strong>&#8377;{{ formatAmount(fareSummary()?.convenienceFee || 0) }}</strong>
            </div>

            <div class="fare-row total">
              <span>Total Amount</span>
              <strong>&#8377;{{ formatAmount(fareSummary()?.totalAmount || 0) }}</strong>
            </div>
          </section>
        }

        @if (failureMessage()) {
          <p class="message error">{{ failureMessage() }}</p>
        }

        @if (infoMessage()) {
          <p class="message info">{{ infoMessage() }}</p>
        }

        <div class="actions">
          <button type="button" [disabled]="busy() || checkoutBusy() || remainingSeconds() <= 0" (click)="payNow()">
            {{ gatewayLabel() }}
          </button>
          <button type="button" class="ghost" [disabled]="busy() || checkoutBusy()" (click)="simulateFailure()">
            Fail Payment
          </button>
        </div>
      </section>
    </main>
  `,
  styles: [`
    .page {
      min-height: 100dvh;
      padding: 26px;
      color: #f8fafc;
      background:
        radial-gradient(circle at top right, rgba(14, 165, 233, 0.16), transparent 20%),
        linear-gradient(180deg, #0f172a 0%, #111827 55%, #020617 100%);
    }
    .card {
      max-width: 920px;
      margin: 0 auto;
      border: 1px solid rgba(148,163,184,0.18);
      border-radius: 24px;
      background: rgba(15,23,42,0.88);
      padding: 22px;
      display: grid;
      gap: 18px;
    }
    .topline {
      display: flex;
      justify-content: space-between;
      align-items: start;
      gap: 16px;
    }
    .eyebrow {
      margin: 0 0 6px;
      color: #38bdf8;
      text-transform: uppercase;
      letter-spacing: 0.12em;
      font-size: 0.78rem;
      font-weight: 800;
    }
    h1, p { margin: 0; }
    p, small { color: #94a3b8; }
    .timer {
      min-width: 160px;
      padding: 14px 16px;
      border-radius: 18px;
      background: rgba(8, 47, 73, 0.55);
      border: 1px solid rgba(56, 189, 248, 0.24);
      display: grid;
      gap: 6px;
      text-align: right;
    }
    .timer strong { font-size: 1.6rem; color: #f8fafc; }
    .timer.expired { background: rgba(127, 29, 29, 0.45); border-color: rgba(248, 113, 113, 0.28); }
    .summary {
      display: grid;
      gap: 14px;
      grid-template-columns: repeat(4, minmax(0, 1fr));
      padding: 16px;
      border-radius: 20px;
      background: rgba(30, 41, 59, 0.82);
    }
    .summary div { display: grid; gap: 6px; }
    .message {
      padding: 14px 16px;
      border-radius: 16px;
      font-weight: 600;
    }
    .message.error { color: #fecaca; background: rgba(127, 29, 29, 0.35); }
    .message.info { color: #bae6fd; background: rgba(8, 47, 73, 0.45); }
    .actions { display: flex; gap: 12px; flex-wrap: wrap; }
    .fare-card {
      display: grid;
      gap: 14px;
      padding: 18px;
      border-radius: 20px;
      border: 1px solid rgba(148,163,184,0.16);
      background: rgba(15, 23, 42, 0.7);
      box-shadow: 0 18px 44px rgba(2, 6, 23, 0.22);
    }
    .fare-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 12px;
    }
    .fare-header h2,
    .fare-header span {
      margin: 0;
    }
    .fare-header span {
      color: #7dd3fc;
      font-size: 0.85rem;
      font-weight: 700;
    }
    .fare-row {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 16px;
      padding-top: 14px;
      border-top: 1px solid rgba(148,163,184,0.14);
      color: #cbd5e1;
    }
    .fare-row strong {
      color: #f8fafc;
    }
    .fare-row.total {
      color: #f8fafc;
      font-size: 1.02rem;
      font-weight: 700;
    }
    .fare-row.total strong {
      font-size: 1.2rem;
    }
    button {
      min-height: 50px;
      padding: 0 18px;
      border-radius: 14px;
      border: 0;
      color: #fff;
      background: linear-gradient(135deg, #0ea5e9, #2563eb);
      font: inherit;
      font-weight: 700;
      cursor: pointer;
    }
    button:disabled { opacity: 0.6; cursor: wait; }
    .ghost { background: rgba(51, 65, 85, 0.9); }
    @media (max-width: 820px) {
      .topline { flex-direction: column; }
      .timer { text-align: left; }
      .summary { grid-template-columns: 1fr 1fr; }
    }
    @media (max-width: 520px) {
      .summary { grid-template-columns: 1fr; }
    }
  `]
})
export class UserPaymentPageComponent {
  private readonly bookingApi = inject(BookingApiService);
  private readonly state = inject(SearchFlowService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly toast = inject(ToastService);

  readonly busy = signal(false);
  readonly checkoutBusy = signal(false);
  readonly remainingSeconds = signal(0);
  readonly failureMessage = signal('');
  readonly infoMessage = signal('');
  readonly fareSummary = computed(() => {
    const checkoutBookingId = this.state.latestCheckout()?.bookingId;
    const latestBooking = this.state.latestBooking();

    if (!checkoutBookingId || !latestBooking || latestBooking.bookingId !== checkoutBookingId) {
      return null;
    }

    return latestBooking;
  });
  readonly amount = computed(() => this.fareSummary()?.totalAmount ?? this.state.latestCheckout()?.amount ?? 0);
  readonly seatCount = computed(() => {
    const summarySeats = this.fareSummary()?.passengers?.length ?? 0;
    return summarySeats || (this.state.bookingDraft()?.passengers.length ?? 0);
  });
  readonly orderId = computed(() => this.state.latestCheckout()?.gatewayOrderId ?? this.state.latestPayment()?.gatewayOrderId ?? '');
  readonly routeLabel = computed(() => {
    const summary = this.fareSummary();
    if (summary?.journey) {
      return `${summary.journey.sourceCityName || 'Source'} to ${summary.journey.destinationCityName || 'Destination'}`;
    }

    const schedule = this.state.selectedSchedule();
    return `${schedule?.sourceCityName || schedule?.sourceCity || 'Source'} to ${schedule?.destinationCityName || schedule?.destinationCity || 'Destination'}`;
  });
  readonly baseFareLabel = computed(() => {
    const summary = this.fareSummary();
    const seats = this.seatCount();
    const seatPrice = summary?.journey?.basePrice ?? (seats ? Number(summary?.baseAmount ?? 0) / seats : 0);

    if (!seats) {
      return 'Base Fare';
    }

    return `Base Fare (₹${this.formatAmount(seatPrice)} × ${seats})`;
  });
  readonly countdown = computed(() => {
    const total = this.remainingSeconds();
    const minutes = Math.floor(total / 60);
    const seconds = total % 60;
    return `${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`;
  });
  readonly gatewayLabel = computed(() => {
    return environment.razorpayKeyId ? 'Pay with Razorpay' : 'Complete Demo Payment';
  });

  private timerId: number | null = null;
  private hasMarkedExpired = false;

  ngOnInit(): void {
    const schedule = this.state.selectedSchedule();
    const draft = this.state.bookingDraft();
    const resumeBookingId = Number(this.route.snapshot.queryParamMap.get('bookingId'));

    if (!schedule || !draft) {
      if (resumeBookingId > 0) {
        this.initPendingPayment(resumeBookingId);
        return;
      }

      this.toast.info('Please complete booking details first.');
      void this.router.navigate(['/search-results']);
      return;
    }

    if (this.state.latestCheckout()) {
      this.loadFareSummary(this.state.latestCheckout()?.bookingId ?? 0);
      this.startCountdown(this.state.latestCheckout()?.expiresAtUtc || '');
      return;
    }

    const payload: CreateBookingRequest = {
      scheduleId: schedule.scheduleId,
      contactEmail: draft.contactEmail,
      contactPhone: draft.contactPhone,
      paymentMethod: 'RAZORPAY',
      paymentReference: null,
      passengers: draft.passengers
    };

    this.checkoutBusy.set(true);
    this.bookingApi.checkout(payload).subscribe({
      next: checkout => {
        this.checkoutBusy.set(false);
        this.state.setLatestCheckout(checkout);
        this.loadFareSummary(checkout.bookingId);
        this.infoMessage.set('Seats are now held for you. Complete payment before the timer runs out.');
        this.startCountdown(checkout.expiresAtUtc);
      },
      error: error => {
        this.checkoutBusy.set(false);
        const message = formatApiError(error, 'Unable to start checkout.');
        this.failureMessage.set(message);
        this.toast.error(message);
      }
    });
  }

  ngOnDestroy(): void {
    this.clearTimer();
  }

  payNow(): void {
    const checkout = this.state.latestCheckout();
    if (!checkout || this.remainingSeconds() <= 0) {
      this.failureMessage.set('The payment window has expired. Please select seats again.');
      return;
    }

    if (environment.razorpayKeyId) {
      void this.openRazorpayCheckout(checkout.paymentId, checkout.gatewayOrderId || '', checkout.amount);
      return;
    }

    void this.confirmPayment(checkout.paymentId, `DEMO-${Date.now()}`, checkout.gatewayOrderId || undefined);
  }

  simulateFailure(): void {
    const checkout = this.state.latestCheckout();
    if (!checkout) {
      return;
    }

    this.busy.set(true);
    this.failureMessage.set('');
    this.bookingApi.failPayment(checkout.paymentId, {
      failureReason: 'Payment failed by user.',
      paymentReference: checkout.gatewayOrderId || undefined
    }).subscribe({
      next: payment => {
        this.busy.set(false);
        this.state.setLatestPayment(payment);
        this.failureMessage.set(payment.failureReason || 'Payment failed. Seats have been released.');
        this.infoMessage.set('');
      },
      error: error => {
        this.busy.set(false);
        this.toast.error(formatApiError(error, 'Could not mark payment as failed.'));
      }
    });
  }

  private async openRazorpayCheckout(paymentId: number, orderId: string, amount: number): Promise<void> {
    const loaded = await this.loadRazorpayScript();
    if (!loaded || !window.Razorpay) {
      this.infoMessage.set('Razorpay script was not available, so demo payment mode will be used.');
      await this.confirmPayment(paymentId, `DEMO-${Date.now()}`, orderId);
      return;
    }

    const razorpay = new window.Razorpay({
      key: environment.razorpayKeyId,
      amount: Math.round(amount * 100),
      currency: 'INR',
      name: 'BusMate',
      description: 'Bus ticket payment',
      order_id: orderId,
      handler: (response: { razorpay_payment_id?: string }) => {
        void this.confirmPayment(paymentId, response.razorpay_payment_id || `RZP-${Date.now()}`, orderId);
      }
    });

    razorpay.on('payment.failed', () => {
      this.simulateFailure();
    });

    razorpay.open();
  }

  private async confirmPayment(paymentId: number, gatewayPaymentId: string, paymentReference?: string): Promise<void> {
    this.busy.set(true);
    this.failureMessage.set('');

    this.bookingApi.confirmPayment(paymentId, {
      gatewayPaymentId,
      paymentReference
    }).subscribe({
      next: booking => {
        this.handlePaymentSuccess(booking);
      },
      error: error => {
        this.busy.set(false);
        this.failureMessage.set(formatApiError(error, 'Payment confirmation failed.'));
      }
    });
  }

  private handlePaymentSuccess(booking: BookingResponse): void {
    this.busy.set(false);
    this.clearTimer();
    this.state.setLatestBooking(booking);
    this.toast.success('Booking confirmed.');
    void this.router.navigate(['/ticket', booking.bookingId]);
  }

  private startCountdown(expiresAtUtc: string): void {
    this.clearTimer();
    this.hasMarkedExpired = false;

    const tick = () => {
      const expiresAt = new Date(expiresAtUtc).getTime();
      const seconds = Math.max(0, Math.floor((expiresAt - Date.now()) / 1000));
      this.remainingSeconds.set(seconds);

      if (seconds === 0 && !this.hasMarkedExpired) {
        this.hasMarkedExpired = true;
        this.failureMessage.set('Payment window expired. Your held seats have been released.');
        this.infoMessage.set('');
        this.simulateFailure();
      }
    };

    tick();
    this.timerId = window.setInterval(tick, 1000);
  }

  private clearTimer(): void {
    if (this.timerId !== null) {
      clearInterval(this.timerId);
      this.timerId = null;
    }
  }

  private async loadRazorpayScript(): Promise<boolean> {
    if (window.Razorpay) {
      return true;
    }

    const existing = document.querySelector<HTMLScriptElement>('script[data-razorpay="true"]');
    if (existing) {
      return new Promise(resolve => {
        existing.addEventListener('load', () => resolve(true), { once: true });
        existing.addEventListener('error', () => resolve(false), { once: true });
      });
    }

    return new Promise(resolve => {
      const script = document.createElement('script');
      script.src = 'https://checkout.razorpay.com/v1/checkout.js';
      script.async = true;
      script.dataset['razorpay'] = 'true';
      script.onload = () => resolve(true);
      script.onerror = () => resolve(false);
      document.body.appendChild(script);
    });
  }

  private initPendingPayment(bookingId: number): void {
    this.checkoutBusy.set(true);
    this.bookingApi.initPayment({ bookingId }).subscribe({
      next: payment => {
        this.checkoutBusy.set(false);
        this.state.setLatestPayment(payment);
        this.state.setLatestCheckout({
          bookingId: payment.bookingId,
          paymentId: payment.paymentId,
          bookingStatus: 'PENDING',
          paymentStatus: payment.status,
          gatewayOrderId: payment.gatewayOrderId,
          amount: payment.amount,
          paymentMethod: payment.paymentMethod,
          expiresAtUtc: payment.expiresAtUtc
        });
        this.loadFareSummary(payment.bookingId);
        this.infoMessage.set('Pending booking loaded. Complete payment to confirm your ticket.');
        this.startCountdown(payment.expiresAtUtc);
      },
      error: error => {
        this.checkoutBusy.set(false);
        this.toast.error(formatApiError(error, 'Unable to continue pending payment.'));
        void this.router.navigate(['/ticket']);
      }
    });
  }

  formatAmount(value: number): string {
    return new Intl.NumberFormat('en-IN', {
      maximumFractionDigits: 0
    }).format(Number(value || 0));
  }

  private loadFareSummary(bookingId: number): void {
    if (!bookingId) {
      return;
    }

    const latestBooking = this.state.latestBooking();
    if (latestBooking?.bookingId === bookingId) {
      return;
    }

    this.bookingApi.ticket(bookingId).subscribe({
      next: booking => this.state.setLatestBooking(booking),
      error: () => {
        // The payment flow can continue even if the summary call is unavailable.
      }
    });
  }
}
