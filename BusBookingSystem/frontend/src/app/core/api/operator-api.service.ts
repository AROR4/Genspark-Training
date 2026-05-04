import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, catchError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { API_ENDPOINTS } from './api-endpoints';
import {
  BusResponse,
  BusScheduleResponse,
  CancelOperatorTripRequest,
  CreateBusRequest,
  CreateBusScheduleRequest,
  OperatorOfficeRequest,
  OperatorOfficeResponse,
  OperatorRevenueResponse,
  OperatorRegisterRequest,
  OperatorRouteResponse,
  OperatorRegistrationResponse,
  OperatorTripResponse
} from './api.models';

@Injectable({ providedIn: 'root' })
export class OperatorApiService {
  constructor(private readonly http: HttpClient) {}

  register(payload: OperatorRegisterRequest): Observable<OperatorRegistrationResponse> {
    return this.http.post<OperatorRegistrationResponse>(this.url(API_ENDPOINTS.operators.register), payload);
  }

  createBus(payload: CreateBusRequest): Observable<BusResponse> {
    return this.http
      .post<BusResponse>(this.url(API_ENDPOINTS.operators.addBus), payload)
      .pipe(catchError(() => this.http.post<BusResponse>(this.url(API_ENDPOINTS.operators.buses), payload)));
  }

  listBuses(): Observable<BusResponse[]> {
    return this.http.get<BusResponse[]>(this.url(API_ENDPOINTS.operators.buses));
  }

  listRoutes(): Observable<OperatorRouteResponse[]> {
    return this.http.get<OperatorRouteResponse[]>(this.url(API_ENDPOINTS.operators.routes));
  }

  listOffices(): Observable<OperatorOfficeResponse[]> {
    return this.http.get<OperatorOfficeResponse[]>(this.url(API_ENDPOINTS.operators.offices));
  }

  addOffice(payload: OperatorOfficeRequest): Observable<OperatorOfficeResponse> {
    return this.http.post<OperatorOfficeResponse>(this.url(API_ENDPOINTS.operators.addOffice), payload);
  }

  createSchedule(payload: CreateBusScheduleRequest): Observable<BusScheduleResponse> {
    return this.http.post<BusScheduleResponse>(this.url(API_ENDPOINTS.operators.addSchedule), payload);
  }

  listSchedules(): Observable<BusScheduleResponse[]> {
    return this.http.get<BusScheduleResponse[]>(this.url(API_ENDPOINTS.operators.schedules));
  }

  listTrips(): Observable<OperatorTripResponse[]> {
    return this.http.get<OperatorTripResponse[]>(this.url(API_ENDPOINTS.operators.trips));
  }

  revenue(): Observable<OperatorRevenueResponse> {
    return this.http.get<OperatorRevenueResponse>(this.url(API_ENDPOINTS.operators.revenue));
  }

  cancelTrip(scheduleId: number, payload: CancelOperatorTripRequest = {}): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(this.url(API_ENDPOINTS.operators.cancelTrip(scheduleId)), payload);
  }

  private url(endpoint: string): string {
    return `${environment.apiBaseUrl}${endpoint}`;
  }
}
