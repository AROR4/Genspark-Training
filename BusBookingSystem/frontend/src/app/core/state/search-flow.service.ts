import { Injectable, signal } from '@angular/core';
import { BookingResponse, ScheduleSearchResponse } from '../api';

@Injectable({ providedIn: 'root' })
export class SearchFlowService {
  readonly searchResults = signal<ScheduleSearchResponse[]>([]);
  readonly selectedSchedule = signal<ScheduleSearchResponse | null>(null);
  readonly latestBooking = signal<BookingResponse | null>(null);

  setResults(results: ScheduleSearchResponse[]): void {
    this.searchResults.set(results);
  }

  setSelectedSchedule(schedule: ScheduleSearchResponse | null): void {
    this.selectedSchedule.set(schedule);
  }

  setLatestBooking(booking: BookingResponse | null): void {
    this.latestBooking.set(booking);
  }
}
