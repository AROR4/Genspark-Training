import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, catchError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { API_ENDPOINTS } from './api-endpoints';
import {
  BookingResponse,
  ConfirmPaymentRequest,
  CreateBookingRequest,
  CreateCheckoutResponse,
  FailPaymentRequest,
  InitPaymentRequest,
  PaymentResponse,
  ScheduleSearchResponse,
  SeatAvailabilityResponse,
  TicketEmailResponse,
  UserCancelBookingResponse
} from './api.models';

@Injectable({ providedIn: 'root' })
export class BookingApiService {
  constructor(private readonly http: HttpClient) {}

  search(sourceCityId: number, destinationCityId: number, date: string): Observable<ScheduleSearchResponse[]> {
    const params = new HttpParams()
      .set('sourceCityId', sourceCityId)
      .set('destinationCityId', destinationCityId)
      .set('date', date);

    const legacyParams = new HttpParams()
      .set('sourceCityId', sourceCityId)
      .set('destinationCityId', destinationCityId)
      .set('travelDate', date);

    return this.http
      .get<ScheduleSearchResponse[]>(this.url(API_ENDPOINTS.bookings.search), { params })
      .pipe(catchError(() => {
        return this.http.get<ScheduleSearchResponse[]>(this.url(API_ENDPOINTS.bookings.legacySearch), { params: legacyParams });
      }));
  }

  seats(scheduleId: number): Observable<SeatAvailabilityResponse[]> {
    return this.http
      .get<SeatAvailabilityResponse[]>(this.url(API_ENDPOINTS.bookings.seats(scheduleId)))
      .pipe(catchError(() => {
        return this.http.get<SeatAvailabilityResponse[]>(this.url(API_ENDPOINTS.bookings.legacySeats(scheduleId)));
      }));
  }

  checkout(payload: CreateBookingRequest): Observable<CreateCheckoutResponse> {
    return this.http.post<CreateCheckoutResponse>(this.url(API_ENDPOINTS.bookings.checkout), payload);
  }

  create(payload: CreateBookingRequest): Observable<BookingResponse> {
    return this.http.post<BookingResponse>(this.url(API_ENDPOINTS.bookings.list), payload);
  }

  initPayment(payload: InitPaymentRequest): Observable<PaymentResponse> {
    return this.http.post<PaymentResponse>(this.url(API_ENDPOINTS.payment.init), payload);
  }

  payment(paymentId: number): Observable<PaymentResponse> {
    return this.http.get<PaymentResponse>(this.url(API_ENDPOINTS.payment.details(paymentId)));
  }

  confirmPayment(paymentId: number, payload: ConfirmPaymentRequest): Observable<BookingResponse> {
    return this.http.post<BookingResponse>(this.url(API_ENDPOINTS.payment.confirm(paymentId)), payload);
  }

  failPayment(paymentId: number, payload: FailPaymentRequest): Observable<PaymentResponse> {
    return this.http.post<PaymentResponse>(this.url(API_ENDPOINTS.payment.fail(paymentId)), payload);
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

  cancelTicket(bookingId: number): Observable<UserCancelBookingResponse> {
    return this.http.post<UserCancelBookingResponse>(this.url(API_ENDPOINTS.bookings.cancelTicket(bookingId)), {});
  }

  downloadTicket(bookingId: number): Observable<Blob> {
    return this.http.get(this.url(API_ENDPOINTS.bookings.downloadTicket(bookingId)), {
      responseType: 'blob'
    });
  }

  downloadUrl(bookingId: number): string {
    return this.url(API_ENDPOINTS.bookings.downloadTicket(bookingId));
  }

  private url(endpoint: string): string {
    return `${environment.apiBaseUrl}${endpoint}`;
  }
}
