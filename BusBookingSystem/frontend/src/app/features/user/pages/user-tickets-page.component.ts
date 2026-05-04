import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthApiService, BookingApiService, BookingResponse, UserApiService, UserBookingSummary, UserCancelBookingResponse, UserProfileResponse } from '../../../core/api';
import { formatApiError } from '../../../core/utils/api-error.util';
import { ToastService } from '../../../core/utils/toast.service';
import { AppHeaderComponent } from '../../../shared/layout/app-header.component';

type BookingTab = 'future' | 'past' | 'cancelled' | 'pending';

@Component({
  selector: 'app-user-tickets-page',
  standalone: true,
  imports: [CommonModule, AppHeaderComponent],
  template: `
    <app-header />
    <main class="page">
      <section class="layout">
        <aside class="sidebar card">
          <button type="button" class="side-link" (click)="goHome()">
            ← Home
          </button>
        </aside>

        <section class="main-column">
          <section class="card profile-card">
            <div class="avatar">{{ initials() }}</div>
            <div class="profile-copy">
              <p class="eyebrow">My Account</p>
              <h1>{{ profile()?.name || 'Passenger' }}</h1>
              <p>{{ profile()?.email || 'No email available' }}</p>
              <p>{{ profile()?.phone || 'No phone available' }}</p>
            </div>
          </section>

          <section class="card bookings-card">
            <div class="section-head">
              <div>
                <p class="eyebrow">Trips</p>
                <h2>Manage your journeys</h2>
              </div>
            </div>

            <div class="tabs" role="tablist" aria-label="Booking status tabs">
              @for (tab of tabs; track tab.key) {
                <button
                  type="button"
                  class="tab"
                  [class.active]="activeTab() === tab.key"
                  [class.future]="tab.key === 'future'"
                  [class.past]="tab.key === 'past'"
                  [class.cancelled]="tab.key === 'cancelled'"
                  [class.pending]="tab.key === 'pending'"
                  (click)="activeTab.set(tab.key)"
                >
                  {{ tab.label }}
                </button>
              }
            </div>

            <div class="cards-list">
              @if (loading()) {
                <article class="empty-state">Loading bookings...</article>
              } @else if (loadError()) {
                <article class="empty-state">{{ loadError() }}</article>
              } @else {
                @for (booking of activeBookings(); track booking.bookingId) {
                  <article class="booking-card" [class.highlight]="booking.bookingId === highlightedBookingId()">
                    <div class="booking-topline">
                      <div>
                        <h3>{{ booking.sourceCity || 'Source' }} → {{ booking.destinationCity || 'Destination' }}</h3>
                        <p>{{ formatTravelDate(booking.travelDate) }} | {{ formatTravelTime(booking.departureTime) }}</p>
                      </div>
                      <span class="status-badge" [ngClass]="badgeClass(booking)">
                        {{ badgeLabel(booking) }}
                      </span>
                    </div>

                    <div class="booking-meta">
                      <span>Status: {{ badgeLabel(booking) }} @if (isCancelled(booking)) { ❌ }</span>
                      <span>Travels: {{ booking.operatorName || 'Not available' }}</span>
                      <span>Bus No: {{ busLabel(booking) }}</span>
                      <span>Contact: {{ booking.contactPhone || 'Not available' }}</span>
                      <span>Seats: {{ booking.seats.join(', ') || 'Not assigned' }}</span>
                      <span>Amount: &#8377;{{ formatAmount(booking.totalAmount) }}</span>
                      <span>Booking: {{ booking.ticketNumber || booking.bookingCode || ('#' + booking.bookingId) }}</span>
                    </div>

                    @if (showFareBreakdown(booking)) {
                      <div class="fare-breakdown">
                        <span>Base Fare: &#8377;{{ formatAmount(booking.baseAmount) }}</span>
                        <span>GST: &#8377;{{ formatAmount(booking.gstAmount) }}</span>
                        <span>Convenience Fee: &#8377;{{ formatAmount(booking.convenienceFee) }}</span>
                      </div>
                    }

                    @if (isCancelled(booking)) {
                      <div class="refund-panel">
                        <div class="refund-head">
                          <strong>Refund Details</strong>
                          <span class="refund-badge">Refund Initiated</span>
                        </div>
                        <span>Refund Amount: &#8377;{{ formatAmount(refundAmount(booking)) }}</span>
                        <span>Refund Status: {{ refundStatusLabel(booking) }}</span>
                        @if (booking.operatorLoss != null) {
                          <span>Operator Loss: &#8377;{{ formatAmount(booking.operatorLoss || 0) }}</span>
                        }
                        @if (booking.adminRevenue != null) {
                          <span>Admin Revenue: &#8377;{{ formatAmount(booking.adminRevenue || 0) }}</span>
                        }
                        <p>{{ cancellationMessage(booking) }}</p>
                      </div>
                    }

                    <div class="actions">
                      <button type="button" class="ghost" (click)="viewDetails(booking.bookingId)">View Details</button>

                      @if (isPending(booking)) {
                        <button type="button" (click)="resumePayment(booking.bookingId)">Pending Payment</button>
                      } @else {
                        <button type="button" class="action-link" (click)="downloadTicket(booking.bookingId)">
                          Download Ticket
                        </button>
                      }

                      @if (canCancelTicket(booking)) {
                        <button
                          type="button"
                          class="danger-action"
                          [disabled]="cancellingBookingId() === booking.bookingId"
                          (click)="openCancelModal(booking)">
                          {{ cancellingBookingId() === booking.bookingId ? 'Cancelling...' : 'Cancel Ticket' }}
                        </button>
                      }
                    </div>
                  </article>
                } @empty {
                  <article class="empty-state">No trips found</article>
                }
              }
            </div>
          </section>
        </section>
      </section>

      @if (detailOpen()) {
        <div class="modal-backdrop" (click)="closeDetails()">
          <section class="ticket-modal" (click)="$event.stopPropagation()">
            <div class="modal-head">
              <div>
                <p class="eyebrow">Ticket Details</p>
                <h2>{{ ticketDetail()?.journey?.sourceCityName || 'Source' }} → {{ ticketDetail()?.journey?.destinationCityName || 'Destination' }}</h2>
              </div>
              <button type="button" class="modal-close" (click)="closeDetails()">Close</button>
            </div>

            @if (detailLoading()) {
              <article class="modal-state">Loading ticket details...</article>
            } @else if (detailError()) {
              <article class="modal-state">{{ detailError() }}</article>
            } @else if (ticketDetail()) {
              <div class="modal-grid">
                <div class="modal-card">
                  <span class="modal-label">Booking</span>
                  <strong>{{ ticketDetail()?.ticketNumber || ticketDetail()?.bookingCode || ('#' + ticketDetail()?.bookingId) }}</strong>
                </div>
                <div class="modal-card">
                  <span class="modal-label">Travel</span>
                  <strong>{{ formatTravelDate(ticketDetail()?.journey?.travelDate || '') }} | {{ formatTravelTime(ticketDetail()?.journey?.departureTime || '') }}</strong>
                </div>
                <div class="modal-card">
                  <span class="modal-label">Travels</span>
                  <strong>{{ ticketDetail()?.journey?.operatorName || 'Not available' }}</strong>
                </div>
                <div class="modal-card">
                  <span class="modal-label">Bus No</span>
                  <strong>{{ ticketBusLabel() }}</strong>
                </div>
                <div class="modal-card">
                  <span class="modal-label">Status</span>
                  <strong>{{ ticketDetail()?.status || 'NA' }}</strong>
                </div>
                <div class="modal-card">
                  <span class="modal-label">Total Amount</span>
                  <strong>&#8377;{{ formatAmount(ticketDetail()?.totalAmount || 0) }}</strong>
                </div>
              </div>

              <div class="modal-section">
                <h3>Contact</h3>
                <p>Email: {{ ticketDetail()?.contactEmail || 'Not available' }}</p>
                <p>Phone: {{ ticketDetail()?.contactPhone || 'Not available' }}</p>
              </div>

              <div class="modal-section">
                <h3>Passengers</h3>
                @for (passenger of ticketDetail()?.passengers || []; track $index) {
                  <article class="passenger-row">
                    <span>{{ passenger.name }}</span>
                    <span>{{ passenger.gender }} · {{ passenger.age }}</span>
                    <strong>Seat {{ passenger.seatNumber }}</strong>
                  </article>
                } @empty {
                  <article class="passenger-row">No passenger details available.</article>
                }
              </div>

              <div class="modal-section">
                <h3>Fare Summary</h3>
                <div class="fare-modal-row">
                  <span>Base Fare</span>
                  <strong>&#8377;{{ formatAmount(ticketDetail()?.baseAmount || 0) }}</strong>
                </div>
                <div class="fare-modal-row">
                  <span>GST</span>
                  <strong>&#8377;{{ formatAmount(ticketDetail()?.gstAmount || 0) }}</strong>
                </div>
                <div class="fare-modal-row">
                  <span>Convenience Fee</span>
                  <strong>&#8377;{{ formatAmount(ticketDetail()?.convenienceFee || 0) }}</strong>
                </div>
                <div class="fare-modal-row total">
                  <span>Total</span>
                  <strong>&#8377;{{ formatAmount(ticketDetail()?.totalAmount || 0) }}</strong>
                </div>
              </div>
            }
          </section>
        </div>
      }

      @if (cancelModalBooking()) {
        <div class="modal-backdrop" (click)="closeCancelModal()">
          <section class="ticket-modal cancel-modal" (click)="$event.stopPropagation()">
            <div class="modal-head">
              <div>
                <p class="eyebrow">Cancel Ticket</p>
                <h2>{{ cancelModalBooking()?.sourceCity || 'Source' }} → {{ cancelModalBooking()?.destinationCity || 'Destination' }}</h2>
              </div>
              <button type="button" class="modal-close" (click)="closeCancelModal()">Close</button>
            </div>

            <div class="modal-section">
              <h3>Cancellation Rules</h3>
              <p>24 hours or more before departure: 100% refund</p>
              <p>12 hours to less than 24 hours: 50% refund</p>
              <p>4 hours to less than 12 hours: 25% refund</p>
              <p>Less than 4 hours: 0% refund</p>
            </div>

            <div class="modal-section">
              <h3>Trip Details</h3>
              <p>Travel Date: {{ formatTravelDate(cancelModalBooking()?.travelDate || '') }}</p>
              <p>Departure Time: {{ formatTravelTime(cancelModalBooking()?.departureTime || '') }}</p>
              <p>Travels: {{ cancelModalBooking()?.operatorName || 'Not available' }}</p>
              <p>Bus No: {{ cancelModalBooking() ? busLabel(cancelModalBooking()!) : 'Not available' }}</p>
            </div>

            <div class="modal-actions">
              <button type="button" class="ghost modal-action" (click)="closeCancelModal()">No</button>
              <button
                type="button"
                class="danger-action modal-action"
                [disabled]="cancellingBookingId() === cancelModalBooking()?.bookingId"
                (click)="confirmCancelTicket()">
                {{ cancellingBookingId() === cancelModalBooking()?.bookingId ? 'Cancelling...' : 'Yes, Cancel Ticket' }}
              </button>
            </div>
          </section>
        </div>
      }
    </main>
  `,
  styles: [`
    .page {
      min-height: 100dvh;
      padding: 26px;
      color: #e2e8f0;
      background:
        radial-gradient(circle at top left, rgba(34, 197, 94, 0.12), transparent 24%),
        radial-gradient(circle at top right, rgba(249, 115, 22, 0.12), transparent 24%),
        linear-gradient(180deg, #0f172a 0%, #111827 48%, #020617 100%);
    }
    .layout {
      max-width: 1220px;
      margin: 0 auto;
      display: grid;
      grid-template-columns: 260px minmax(0, 1fr);
      gap: 22px;
      align-items: start;
    }
    .card {
      border: 1px solid rgba(148,163,184,0.18);
      border-radius: 24px;
      background: rgba(15, 23, 42, 0.86);
      box-shadow: 0 24px 60px rgba(2, 6, 23, 0.3);
    }
    .sidebar {
      position: sticky;
      top: 92px;
      padding: 18px;
      display: grid;
      gap: 12px;
    }
    .side-link,
    .actions button,
    .action-link,
    .tab {
      min-height: 44px;
      border-radius: 14px;
      border: 1px solid rgba(148,163,184,0.2);
      background: rgba(30, 41, 59, 0.82);
      color: #e2e8f0;
      font: inherit;
      text-decoration: none;
      cursor: pointer;
      transition: 160ms ease;
    }
    .side-link {
      padding: 0 16px;
      text-align: left;
      font-weight: 700;
    }
    .side-link.active,
    .side-link:hover,
    .tab.active {
      border-color: rgba(125, 211, 252, 0.38);
      background: rgba(8, 47, 73, 0.8);
      color: #f8fafc;
    }
    .side-link.danger {
      border-color: rgba(248, 113, 113, 0.26);
      color: #fecaca;
      background: rgba(127, 29, 29, 0.24);
    }
    .main-column {
      display: grid;
      gap: 18px;
    }
    .profile-card {
      padding: 24px;
      display: grid;
      grid-template-columns: auto 1fr;
      gap: 18px;
      align-items: center;
    }
    .avatar {
      width: 78px;
      height: 78px;
      border-radius: 50%;
      display: grid;
      place-items: center;
      font-size: 1.5rem;
      font-weight: 800;
      color: #f8fafc;
      background: linear-gradient(135deg, #16a34a, #0ea5e9);
    }
    .profile-copy,
    .section-head {
      display: grid;
      gap: 6px;
    }
    .eyebrow {
      margin: 0;
      color: #86efac;
      text-transform: uppercase;
      letter-spacing: 0.1em;
      font-size: 0.78rem;
      font-weight: 800;
    }
    h1,
    h2,
    h3,
    p {
      margin: 0;
    }
    .profile-copy p:last-child,
    .section-head p:last-child,
    .booking-topline p,
    .booking-meta span,
    .fare-breakdown span {
      color: #94a3b8;
    }
    .bookings-card {
      display: grid;
    }
    .section-head {
      padding: 22px 22px 0;
    }
    .tabs {
      display: flex;
      gap: 10px;
      flex-wrap: wrap;
      padding: 18px 22px 0;
    }
    .tab {
      padding: 0 16px;
      font-weight: 700;
    }
    .tab.future.active { border-color: rgba(74, 222, 128, 0.36); background: rgba(20, 83, 45, 0.8); }
    .tab.past.active { border-color: rgba(148, 163, 184, 0.32); background: rgba(51, 65, 85, 0.82); }
    .tab.cancelled.active { border-color: rgba(248, 113, 113, 0.32); background: rgba(127, 29, 29, 0.62); }
    .tab.pending.active { border-color: rgba(251, 146, 60, 0.36); background: rgba(124, 45, 18, 0.74); }
    .cards-list {
      display: grid;
      gap: 16px;
      padding: 22px;
    }
    .booking-card,
    .empty-state {
      border: 1px solid rgba(148,163,184,0.16);
      border-radius: 22px;
      background: rgba(30, 41, 59, 0.72);
      padding: 18px;
    }
    .booking-card {
      display: grid;
      gap: 14px;
    }
    .booking-card.highlight {
      border-color: rgba(125, 211, 252, 0.38);
      box-shadow: 0 0 0 1px rgba(125, 211, 252, 0.18);
    }
    .booking-topline {
      display: flex;
      justify-content: space-between;
      gap: 16px;
      align-items: start;
    }
    .status-badge {
      padding: 8px 12px;
      border-radius: 999px;
      font-size: 0.82rem;
      font-weight: 800;
      letter-spacing: 0.04em;
    }
    .status-future { color: #bbf7d0; background: rgba(20, 83, 45, 0.9); }
    .status-past { color: #e2e8f0; background: rgba(51, 65, 85, 0.9); }
    .status-cancelled { color: #fecaca; background: rgba(127, 29, 29, 0.88); }
    .status-pending { color: #fed7aa; background: rgba(124, 45, 18, 0.88); }
    .booking-meta,
    .fare-breakdown {
      display: flex;
      gap: 18px;
      flex-wrap: wrap;
    }
    .refund-panel {
      display: grid;
      gap: 8px;
      padding: 14px 16px;
      border-radius: 18px;
      border: 1px solid rgba(248,113,113,0.2);
      background: rgba(127, 29, 29, 0.18);
    }
    .refund-head {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 12px;
    }
    .refund-badge {
      padding: 6px 10px;
      border-radius: 999px;
      background: rgba(245, 158, 11, 0.18);
      color: #fcd34d;
      font-size: 0.78rem;
      font-weight: 800;
      letter-spacing: 0.04em;
      text-transform: uppercase;
    }
    .refund-panel strong,
    .refund-panel span:first-child {
      color: #fecaca;
    }
    .refund-panel span,
    .refund-panel p {
      margin: 0;
      color: #e2e8f0;
    }
    .actions {
      display: flex;
      gap: 12px;
      flex-wrap: wrap;
    }
    .actions button,
    .action-link {
      padding: 0 16px;
      display: inline-grid;
      place-items: center;
      font-weight: 700;
    }
    .actions button:not(.ghost) {
      border: 0;
      background: linear-gradient(135deg, #0ea5e9, #2563eb);
      color: #fff;
    }
    .actions .danger-action {
      border: 1px solid rgba(248, 113, 113, 0.24);
      background: rgba(127, 29, 29, 0.92);
      color: #fecaca;
    }
    .ghost {
      background: rgba(15, 23, 42, 0.92);
    }
    .empty-state {
      color: #94a3b8;
      text-align: center;
    }
    .modal-backdrop {
      position: fixed;
      inset: 0;
      background: rgba(2, 6, 23, 0.72);
      backdrop-filter: blur(8px);
      display: grid;
      place-items: center;
      padding: 20px;
      z-index: 40;
    }
    .ticket-modal {
      width: min(880px, 100%);
      max-height: min(88dvh, 920px);
      overflow: auto;
      border-radius: 28px;
      border: 1px solid rgba(148,163,184,0.2);
      background: linear-gradient(180deg, rgba(15, 23, 42, 0.98), rgba(30, 41, 59, 0.98));
      box-shadow: 0 30px 80px rgba(2, 6, 23, 0.45);
      padding: 24px;
      display: grid;
      gap: 18px;
    }
    .modal-head {
      display: flex;
      justify-content: space-between;
      align-items: start;
      gap: 16px;
    }
    .modal-close {
      min-height: 42px;
      padding: 0 16px;
      border-radius: 14px;
      border: 1px solid rgba(148,163,184,0.2);
      background: rgba(15, 23, 42, 0.92);
      color: #e2e8f0;
      font: inherit;
      cursor: pointer;
    }
    .modal-grid {
      display: grid;
      grid-template-columns: repeat(4, minmax(0, 1fr));
      gap: 12px;
    }
    .modal-card,
    .modal-section,
    .modal-state {
      border-radius: 20px;
      border: 1px solid rgba(148,163,184,0.14);
      background: rgba(15, 23, 42, 0.72);
      padding: 16px;
    }
    .modal-card {
      display: grid;
      gap: 6px;
    }
    .modal-label {
      color: #7dd3fc;
      font-size: 0.78rem;
      font-weight: 800;
      letter-spacing: 0.08em;
      text-transform: uppercase;
    }
    .modal-section {
      display: grid;
      gap: 10px;
    }
    .modal-section h3 {
      margin: 0;
      color: #f8fafc;
    }
    .modal-section p {
      color: #cbd5e1;
    }
    .modal-actions {
      display: flex;
      justify-content: end;
      gap: 12px;
      flex-wrap: wrap;
    }
    .modal-action {
      min-height: 44px;
      padding: 0 16px;
      border-radius: 14px;
      font: inherit;
      font-weight: 700;
      cursor: pointer;
    }
    .passenger-row,
    .fare-modal-row {
      display: flex;
      justify-content: space-between;
      gap: 12px;
      align-items: center;
      padding-top: 10px;
      border-top: 1px solid rgba(148,163,184,0.1);
      color: #cbd5e1;
    }
    .passenger-row:first-of-type,
    .fare-modal-row:first-of-type {
      border-top: 0;
      padding-top: 0;
    }
    .fare-modal-row.total {
      color: #f8fafc;
      font-weight: 800;
    }
    @media (max-width: 980px) {
      .layout {
        grid-template-columns: 1fr;
      }
      .sidebar {
        position: static;
        grid-template-columns: repeat(3, minmax(0, 1fr));
      }
    }
    @media (max-width: 640px) {
      .page {
        padding: 18px;
      }
      .profile-card {
        grid-template-columns: 1fr;
      }
      .sidebar {
        grid-template-columns: 1fr;
      }
      .booking-topline {
        flex-direction: column;
      }
      .modal-grid {
        grid-template-columns: 1fr 1fr;
      }
    }
    @media (max-width: 560px) {
      .modal-grid {
        grid-template-columns: 1fr;
      }
      .passenger-row,
      .fare-modal-row,
      .modal-head,
      .modal-actions {
        flex-direction: column;
        align-items: start;
      }
    }
  `]
})
export class UserTicketsPageComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly bookingApi = inject(BookingApiService);
  private readonly userApi = inject(UserApiService);
  private readonly authApi = inject(AuthApiService);
  private readonly toast = inject(ToastService);

  readonly tabs: Array<{ key: BookingTab; label: string }> = [
    { key: 'future', label: 'Future Trips' },
    { key: 'past', label: 'Past Trips' },
    { key: 'cancelled', label: 'Cancelled' },
    { key: 'pending', label: 'Pending Payment' }
  ];
  readonly activeSection = signal<'profile' | 'bookings'>('bookings');
  readonly activeTab = signal<BookingTab>('future');
  readonly loading = signal(true);
  readonly loadError = signal('');
  readonly profile = signal<UserProfileResponse | null>(null);
  readonly bookings = signal<UserBookingSummary[]>([]);
  readonly highlightedBookingId = signal<number | null>(null);
  readonly detailOpen = signal(false);
  readonly detailLoading = signal(false);
  readonly detailError = signal('');
  readonly ticketDetail = signal<BookingResponse | null>(null);
  readonly cancellingBookingId = signal<number | null>(null);
  readonly cancelModalBooking = signal<UserBookingSummary | null>(null);
  readonly today = signal(this.toLocalIsoDate(new Date()));
  readonly futureTrips = computed(() => this.bookings().filter(booking => this.isFutureTrip(booking)));
  readonly pastTrips = computed(() => this.bookings().filter(booking => this.isPastTrip(booking)));
  readonly cancelledTrips = computed(() => this.bookings().filter(booking => this.isCancelled(booking)));
  readonly pendingTrips = computed(() => this.bookings().filter(booking => this.isPending(booking)));
  readonly activeBookings = computed(() => {
    switch (this.activeTab()) {
      case 'past':
        return this.pastTrips();
      case 'cancelled':
        return this.cancelledTrips();
      case 'pending':
        return this.pendingTrips();
      case 'future':
      default:
        return this.futureTrips();
    }
  });

  ngOnInit(): void {
    this.syncRouteState();

    this.userApi.profile().subscribe({
      next: profile => this.profile.set(profile),
      error: error => {
        this.profile.set({
          name: this.authApi.currentUser()?.name ?? 'Passenger',
          email: this.authApi.currentUser()?.email ?? '',
          phone: this.authApi.currentUser()?.phoneNumber ?? ''
        });
        this.toast.error(formatApiError(error, 'Unable to load profile details.'));
      }
    });

    this.userApi.bookings().subscribe({
      next: bookings => {
        this.loading.set(false);
        this.bookings.set(bookings);
        this.selectInitialTab(this.highlightedBookingId(), bookings);
      },
      error: error => {
        this.loading.set(false);
        this.loadError.set(formatApiError(error, 'Unable to load your bookings right now.'));
      }
    });
  }

  initials(): string {
    const source = this.profile()?.name || this.profile()?.email || 'P';
    return source.trim().slice(0, 1).toUpperCase();
  }

  formatAmount(value: number): string {
    return new Intl.NumberFormat('en-IN', { maximumFractionDigits: 0 }).format(Number(value || 0));
  }

  showFareBreakdown(booking: UserBookingSummary): boolean {
    return booking.baseAmount > 0 || booking.gstAmount > 0 || booking.convenienceFee > 0;
  }

  refundAmount(booking: UserBookingSummary): number {
    return Number(booking.refundAmount ?? booking.totalAmount ?? 0);
  }

  refundStatusLabel(booking: UserBookingSummary): string {
    return booking.refundAmount != null ? 'Completed' : 'Not Available';
  }

  formatTravelDate(value: string): string {
    if (!value) {
      return 'Date unavailable';
    }

    return new Intl.DateTimeFormat('en-GB', {
      day: '2-digit',
      month: 'short'
    }).format(new Date(`${value}T00:00:00`));
  }

  formatTravelTime(value: string): string {
    if (!value) {
      return 'Time unavailable';
    }

    const [hours = '0', minutes = '0'] = value.split(':');
    const date = new Date();
    date.setHours(Number(hours), Number(minutes), 0, 0);

    return new Intl.DateTimeFormat('en-US', {
      hour: 'numeric',
      minute: '2-digit'
    }).format(date);
  }

  busLabel(booking: UserBookingSummary): string {
    return booking.registrationNumber || (booking.busId != null ? `Bus #${booking.busId}` : 'Not available');
  }

  ticketBusLabel(): string {
    const journey = this.ticketDetail()?.journey;
    return journey?.registrationNumber || (journey?.busId != null ? `Bus #${journey.busId}` : 'Not available');
  }

  badgeLabel(booking: UserBookingSummary): string {
    if (this.isCancelled(booking)) {
      return 'Cancelled';
    }

    if (this.isPending(booking)) {
      return 'Pending';
    }

    return booking.travelDate < this.today() ? 'Past' : 'Upcoming';
  }

  badgeClass(booking: UserBookingSummary): string {
    if (this.isCancelled(booking)) {
      return 'status-cancelled';
    }

    if (this.isPending(booking)) {
      return 'status-pending';
    }

    return booking.travelDate < this.today() ? 'status-past' : 'status-future';
  }

  cancellationMessage(booking: UserBookingSummary): string {
    const cancellationType = booking.cancellationType?.toString().trim().toUpperCase();

    if (cancellationType === 'USER') {
      return 'Ticket cancelled by user. Refund processed based on departure time.';
    }

    if (cancellationType === 'ADMIN') {
      return 'Trip cancelled by system. 50% operator compensation applied.';
    }

    if (cancellationType === 'OPERATOR_BUS') {
      return 'Trip cancelled because the bus was disabled. Full refund issued.';
    }

    return 'Trip cancelled by operator. Full refund issued.';
  }

  canCancelTicket(booking: UserBookingSummary): boolean {
    return this.isConfirmed(booking) && booking.travelDate >= this.today();
  }

  viewDetails(bookingId: number): void {
    this.highlightedBookingId.set(bookingId);
    this.activeSection.set('bookings');
    this.detailOpen.set(true);
    this.detailLoading.set(true);
    this.detailError.set('');
    this.ticketDetail.set(null);

    this.bookingApi.ticket(bookingId).subscribe({
      next: detail => {
        this.ticketDetail.set(detail);
        this.detailLoading.set(false);
      },
      error: error => {
        this.detailLoading.set(false);
        this.detailError.set(formatApiError(error, 'Unable to load ticket details right now.'));
      }
    });
  }

  downloadTicket(bookingId: number): void {
    this.bookingApi.downloadTicket(bookingId).subscribe({
      next: file => {
        const fileUrl = URL.createObjectURL(file);
        const link = document.createElement('a');
        link.href = fileUrl;
        link.download = `ticket-${bookingId}.txt`;
        link.click();
        URL.revokeObjectURL(fileUrl);
      },
      error: error => {
        this.toast.error(formatApiError(error, 'Unable to download ticket right now.'));
      }
    });
  }

  resumePayment(bookingId: number): void {
    void this.router.navigate(['/payment'], { queryParams: { bookingId } });
  }

  openCancelModal(booking: UserBookingSummary): void {
    if (!this.canCancelTicket(booking)) {
      this.toast.error('Only upcoming confirmed tickets can be cancelled.');
      return;
    }

    this.cancelModalBooking.set(booking);
  }

  closeCancelModal(): void {
    if (this.cancellingBookingId()) {
      return;
    }

    this.cancelModalBooking.set(null);
  }

  confirmCancelTicket(): void {
    const booking = this.cancelModalBooking();
    if (!booking) {
      return;
    }

    this.cancellingBookingId.set(booking.bookingId);

    this.bookingApi.cancelTicket(booking.bookingId).subscribe({
      next: response => {
        this.cancellingBookingId.set(null);
        this.cancelModalBooking.set(null);
        this.bookings.update(items => items.map(item => item.bookingId === booking.bookingId ? this.applyCancelledBooking(item, response) : item));

        if (this.ticketDetail()?.bookingId === booking.bookingId) {
          this.ticketDetail.update(detail => {
            if (!detail) {
              return detail;
            }

            return {
              ...detail,
              status: response.booking?.status ?? 'CANCELLED',
              refundAmount: response.refundAmount,
              cancellationType: response.booking?.cancellationType ?? 'USER',
              journey: detail.journey ? {
                ...detail.journey,
                busId: response.booking?.journey?.busId ?? detail.journey.busId,
                registrationNumber: response.booking?.journey?.registrationNumber ?? detail.journey.registrationNumber
              } : detail.journey
            };
          });
        }

        this.activeTab.set('cancelled');
        this.toast.success(response.message || 'Ticket cancelled successfully.');
      },
      error: error => {
        this.cancellingBookingId.set(null);
        this.toast.error(formatApiError(error, 'Unable to cancel ticket right now.'));
      }
    });
  }

  goHome(): void {
    void this.router.navigate(['/']);
  }

  closeDetails(): void {
    this.detailOpen.set(false);
    this.detailLoading.set(false);
    this.detailError.set('');
  }

  logout(): void {
    this.authApi.logout();
    void this.router.navigate(['/login']);
  }

  isPending(booking: UserBookingSummary): boolean {
    const status = this.normalizeStatus(booking.status);
    const paymentStatus = this.normalizeStatus(booking.paymentStatus);
    return status === 'pending' || paymentStatus === 'pending';
  }

  private isFutureTrip(booking: UserBookingSummary): boolean {
    return this.isConfirmed(booking) && booking.travelDate >= this.today();
  }

  private isPastTrip(booking: UserBookingSummary): boolean {
    return this.isConfirmed(booking) && booking.travelDate < this.today();
  }

  isCancelled(booking: UserBookingSummary): boolean {
    const status = this.normalizeStatus(booking.status);
    return status === 'cancelled' || status === 'failed';
  }

  private isConfirmed(booking: UserBookingSummary): boolean {
    return this.normalizeStatus(booking.status) === 'confirmed';
  }

  private normalizeStatus(status: string | null | undefined): string {
    return status?.trim().toLowerCase() ?? '';
  }

  private selectInitialTab(bookingId: number | null, bookings: UserBookingSummary[]): void {
    if (!bookingId) {
      this.activeTab.set(
        this.futureTrips().length
          ? 'future'
          : this.pendingTrips().length
            ? 'pending'
            : this.pastTrips().length
              ? 'past'
              : 'cancelled'
      );
      return;
    }

    const matched = bookings.find(booking => booking.bookingId === bookingId);
    if (!matched) {
      return;
    }

    if (this.isPending(matched)) {
      this.activeTab.set('pending');
      return;
    }

    if (this.isCancelled(matched)) {
      this.activeTab.set('cancelled');
      return;
    }

    this.activeTab.set(matched.travelDate < this.today() ? 'past' : 'future');
  }

  private syncRouteState(): void {
    this.route.paramMap.subscribe(params => {
      const bookingId = Number(params.get('bookingId'));
      this.highlightedBookingId.set(bookingId || null);
    });
  }

  private toLocalIsoDate(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  private applyCancelledBooking(booking: UserBookingSummary, response: UserCancelBookingResponse): UserBookingSummary {
    return {
      ...booking,
      status: response.booking?.status ?? 'CANCELLED',
      refundAmount: Number(response.refundAmount ?? booking.refundAmount ?? 0),
      cancellationType: response.booking?.cancellationType ?? 'USER',
      registrationNumber: response.booking?.journey?.registrationNumber ?? booking.registrationNumber,
      busId: response.booking?.journey?.busId ?? booking.busId
    };
  }
}
