import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { API_ENDPOINTS } from './api-endpoints';
import {
  BookingResponse,
  CreateBookingRequest,
  ScheduleSearchResponse,
  SeatAvailabilityResponse,
  TicketEmailResponse
} from './api.models';

@Injectable({ providedIn: 'root' })
export class BookingApiService {
  constructor(private readonly http: HttpClient) {}

  search(sourceCityName: string, destinationCityName: string, travelDate: string): Observable<ScheduleSearchResponse[]> {
    const params = new HttpParams()
      .set('sourceCityName', sourceCityName)
      .set('destinationCityName', destinationCityName)
      .set('travelDate', travelDate);

    return this.http.get<ScheduleSearchResponse[]>(this.url(API_ENDPOINTS.bookings.search), { params });
  }

  seats(scheduleId: number): Observable<SeatAvailabilityResponse[]> {
    return this.http.get<SeatAvailabilityResponse[]>(this.url(API_ENDPOINTS.bookings.seats(scheduleId)));
  }

  create(payload: CreateBookingRequest): Observable<BookingResponse> {
    return this.http.post<BookingResponse>(this.url(API_ENDPOINTS.bookings.list), payload);
  }

  list(): Observable<BookingResponse[]> {
    return this.http.get<BookingResponse[]>(this.url(API_ENDPOINTS.bookings.list));
  }

  ticket(bookingId: number): Observable<BookingResponse> {
    return this.http.get<BookingResponse>(this.url(API_ENDPOINTS.bookings.ticket(bookingId)));
  }

  emailTicket(bookingId: number): Observable<TicketEmailResponse> {
    return this.http.post<TicketEmailResponse>(this.url(API_ENDPOINTS.bookings.emailTicket(bookingId)), {});
  }

  downloadUrl(bookingId: number): string {
    return this.url(API_ENDPOINTS.bookings.downloadTicket(bookingId));
  }

  private url(endpoint: string): string {
    return `${environment.apiBaseUrl}${endpoint}`;
  }
}
