import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map, of, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { API_ENDPOINTS } from './api-endpoints';
import { AuthResponse, BookingResponse, UserBookingSummary, UserProfileResponse } from './api.models';
import { AuthApiService } from './auth-api.service';

@Injectable({ providedIn: 'root' })
export class UserApiService {
  private readonly http = inject(HttpClient);
  private readonly authApi = inject(AuthApiService);

  profile(): Observable<UserProfileResponse> {
    const currentUser = this.authApi.currentUser();
    if (!currentUser) {
      return throwError(() => new Error('Unable to load user profile.'));
    }

    return of(this.toProfile(currentUser));
  }

  bookings(): Observable<UserBookingSummary[]> {
    return this.http.get<BookingResponse[]>(this.url(API_ENDPOINTS.user.bookings)).pipe(
      map(bookings => bookings.map(booking => this.fromBookingResponse(booking)))
    );
  }

  private fromBookingResponse(booking: BookingResponse): UserBookingSummary {
    return {
      bookingId: booking.bookingId,
      status: booking.status,
      paymentStatus: booking.paymentStatus,
      travelDate: booking.journey?.travelDate ?? '',
      departureTime: booking.journey?.departureTime ?? '',
      sourceCity: booking.journey?.sourceCityName ?? null,
      destinationCity: booking.journey?.destinationCityName ?? null,
      operatorName: booking.journey?.operatorName ?? null,
      busId: booking.journey?.busId ?? null,
      registrationNumber: booking.journey?.registrationNumber ?? null,
      contactPhone: booking.contactPhone ?? null,
      seats: (booking.passengers ?? []).map(passenger => passenger.seatNumber ?? '').filter(Boolean),
      baseAmount: Number(booking.baseAmount ?? 0),
      gstAmount: Number(booking.gstAmount ?? 0),
      convenienceFee: Number(booking.convenienceFee ?? 0),
      totalAmount: Number(booking.totalAmount ?? 0),
      refundAmount: booking.refundAmount != null ? Number(booking.refundAmount) : this.inferRefundAmount(booking),
      operatorLoss: booking.operatorLoss != null ? Number(booking.operatorLoss) : null,
      adminRevenue: booking.adminRevenue != null ? Number(booking.adminRevenue) : null,
      cancellationType: booking.cancellationType ?? this.inferCancellationType(booking),
      ticketNumber: booking.ticketNumber,
      bookingCode: booking.bookingCode
    };
  }

  private inferRefundAmount(booking: BookingResponse): number | null {
    const status = booking.status?.trim().toLowerCase();
    if (status !== 'cancelled') {
      return null;
    }

    return Number(booking.totalAmount ?? 0);
  }

  private inferCancellationType(booking: BookingResponse): string | null {
    const status = booking.status?.trim().toLowerCase();
    if (status !== 'cancelled') {
      return null;
    }

    return 'OPERATOR_TRIP';
  }

  private toProfile(user: AuthResponse): UserProfileResponse {
    return {
      name: user.name ?? null,
      email: user.email ?? null,
      phone: user.phoneNumber ?? null
    };
  }

  private url(endpoint: string): string {
    return `${environment.apiBaseUrl}${endpoint}`;
  }
}
