import { Injectable, signal } from '@angular/core';
import { BookingResponse, CreateCheckoutResponse, PaymentResponse, ScheduleSearchResponse } from '../api';

export interface SearchQuery {
  sourceCityId: number;
  destinationCityId: number;
  sourceCityName: string;
  destinationCityName: string;
  date: string;
}

export interface BookingDraft {
  contactEmail: string;
  contactPhone: string;
  passengers: Array<{ name: string; age: number; gender: string; seatAvailabilityId: number }>;
}

@Injectable({ providedIn: 'root' })
export class SearchFlowService {
  readonly searchQuery = signal<SearchQuery | null>(null);
  readonly searchResults = signal<ScheduleSearchResponse[]>([]);
  readonly selectedSchedule = signal<ScheduleSearchResponse | null>(null);
  readonly selectedSeatIds = signal<number[]>([]);
  readonly bookingDraft = signal<BookingDraft | null>(null);
  readonly latestCheckout = signal<CreateCheckoutResponse | null>(null);
  readonly latestPayment = signal<PaymentResponse | null>(null);
  readonly latestBooking = signal<BookingResponse | null>(null);

  setSearchQuery(query: SearchQuery | null): void {
    this.searchQuery.set(query);
  }

  setResults(results: ScheduleSearchResponse[]): void {
    this.searchResults.set(results);
  }

  setSelectedSchedule(schedule: ScheduleSearchResponse | null): void {
    this.selectedSchedule.set(schedule);
  }

  setSelectedSeatIds(seatIds: number[]): void {
    this.selectedSeatIds.set(seatIds);
  }

  setBookingDraft(draft: BookingDraft | null): void {
    this.bookingDraft.set(draft);
  }

  setLatestCheckout(checkout: CreateCheckoutResponse | null): void {
    this.latestCheckout.set(checkout);
  }

  setLatestPayment(payment: PaymentResponse | null): void {
    this.latestPayment.set(payment);
  }

  setLatestBooking(booking: BookingResponse | null): void {
    this.latestBooking.set(booking);
  }
}
