import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { BookingApiService, BookingResponse } from '../../../core/api';
import { AuthApiService } from '../../../core/api/auth-api.service';
import { SearchFlowService } from '../../../core/state/search-flow.service';
import { formatApiError } from '../../../core/utils/api-error.util';
import { ToastService } from '../../../core/utils/toast.service';
import { AppHeaderComponent } from '../../../shared/layout/app-header.component';

@Component({
  selector: 'app-user-tickets-page',
  standalone: true,
  imports: [CommonModule, AppHeaderComponent],
  template: `
    <app-header />
    <main class="page">
      <section class="hero">
        <span class="eyebrow">Tickets</span>
        <h1>Your bookings</h1>
        <p>{{ message() }}</p>
      </section>

      @if (focusedTicket()) {
        <section class="ticket featured">
          <div>
            <strong>{{ focusedTicket()?.ticketNumber || focusedTicket()?.bookingCode }}</strong>
            <span>{{ focusedTicket()?.journey?.sourceCityName }} to {{ focusedTicket()?.journey?.destinationCityName }}</span>
          </div>
          <div class="actions">
            <button type="button" (click)="sendEmail(focusedTicket()!.bookingId)">Email ticket</button>
            <button type="button" (click)="download(focusedTicket()!.bookingId)">Download</button>
          </div>
        </section>
      }

      <section class="ticket-list">
        @for (ticket of tickets(); track ticket.bookingId) {
          <article class="ticket">
            <div>
              <strong>{{ ticket.ticketNumber || ticket.bookingCode }}</strong>
              <span>{{ ticket.journey?.sourceCityName }} to {{ ticket.journey?.destinationCityName }}</span>
              <small>{{ ticket.passengers?.length || 0 }} passenger(s) · {{ ticket.paymentStatus }}</small>
            </div>
            <div>
              <strong>Rs {{ ticket.totalAmount }}</strong>
              <small>{{ ticket.status }}</small>
            </div>
          </article>
        } @empty {
          <article class="ticket empty">No tickets found for the current session.</article>
        }
      </section>
    </main>
  `,
  styles: [`
    .page { min-height: 100dvh; padding: 28px; color: #f8fafc; background: #050607; }
    .hero, .ticket { border: 1px solid rgba(255,255,255,0.08); border-radius: 24px; background: rgba(255,255,255,0.06); }
    .hero, .ticket { padding: 24px; }
    .eyebrow, p, .ticket span, .ticket small { color: #94a3b8; }
    h1, p { margin: 0; }
    .ticket-list { display: grid; gap: 14px; margin-top: 24px; }
    .ticket, .actions { display: flex; align-items: center; justify-content: space-between; gap: 16px; }
    .ticket span, .ticket small { display: block; }
    .ticket small { margin-top: 4px; }
    .actions button { min-height: 46px; padding: 0 14px; border: 0; border-radius: 14px; color: #fff; background: #2563eb; }
    .featured { margin-top: 24px; }
    .empty { justify-content: center; }
    @media (max-width: 760px) { .ticket { flex-direction: column; align-items: start; } .actions { width: 100%; flex-wrap: wrap; } }
  `]
})
export class UserTicketsPageComponent {
  private readonly bookingApi = inject(BookingApiService);
  private readonly authApi = inject(AuthApiService);
  private readonly state = inject(SearchFlowService);
  private readonly route = inject(ActivatedRoute);
  private readonly toast = inject(ToastService);

  readonly message = signal('Loading tickets from the live bookings API.');
  readonly tickets = signal<BookingResponse[]>([]);
  readonly focusedTicket = computed(() => this.state.latestBooking());

  ngOnInit(): void {
    const bookingId = Number(this.route.snapshot.paramMap.get('bookingId'));

    if (bookingId) {
      this.bookingApi.ticket(bookingId).subscribe({
        next: ticket => {
          this.state.setLatestBooking(ticket);
          this.toast.success('Ticket loaded.');
        },
        error: error => {
          const message = formatApiError(error, 'Failed to load ticket.');
          this.message.set(message);
          this.toast.error(message);
        }
      });
    }

    this.bookingApi.list().subscribe({
      next: bookings => {
        const currentUser = this.authApi.currentUser();
        const filtered = bookings.filter(item =>
          item.contactEmail === currentUser?.email || item.contactPhone === currentUser?.phoneNumber
        );
        this.tickets.set(filtered.length ? filtered : bookings);
        this.message.set(`Loaded ${bookings.length} booking record(s).`);
      },
      error: error => {
        const message = formatApiError(error, 'Failed to load tickets.');
        this.message.set(message);
        this.toast.error(message);
      }
    });
  }

  sendEmail(bookingId: number): void {
    this.bookingApi.emailTicket(bookingId).subscribe({
      next: response => {
        this.message.set(response.message || 'Ticket email request sent.');
        this.toast.success(response.message || 'Ticket email request sent.');
      },
      error: error => {
        const message = formatApiError(error, 'Email send failed.');
        this.message.set(message);
        this.toast.error(message);
      }
    });
  }

  download(bookingId: number): void {
    window.open(this.bookingApi.downloadUrl(bookingId), '_blank', 'noopener,noreferrer');
  }
}
