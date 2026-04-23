import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { API_ENDPOINTS } from './api-endpoints';
import {
  BusResponse,
  BusScheduleResponse,
  CreateBusRequest,
  CreateBusScheduleRequest,
  OperatorRegisterRequest,
  OperatorRegistrationResponse
} from './api.models';

@Injectable({ providedIn: 'root' })
export class OperatorApiService {
  constructor(private readonly http: HttpClient) {}

  register(payload: OperatorRegisterRequest): Observable<OperatorRegistrationResponse> {
    return this.http.post<OperatorRegistrationResponse>(this.url(API_ENDPOINTS.operators.register), payload);
  }

  createBus(payload: CreateBusRequest): Observable<BusResponse> {
    return this.http.post<BusResponse>(this.url(API_ENDPOINTS.operators.buses), payload);
  }

  listBuses(): Observable<BusResponse[]> {
    return this.http.get<BusResponse[]>(this.url(API_ENDPOINTS.operators.buses));
  }

  createSchedule(payload: CreateBusScheduleRequest): Observable<BusScheduleResponse> {
    return this.http.post<BusScheduleResponse>(this.url(API_ENDPOINTS.operators.schedules), payload);
  }

  listSchedules(): Observable<BusScheduleResponse[]> {
    return this.http.get<BusScheduleResponse[]>(this.url(API_ENDPOINTS.operators.schedules));
  }

  private url(endpoint: string): string {
    return `${environment.apiBaseUrl}${endpoint}`;
  }
}
