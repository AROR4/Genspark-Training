import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { API_ENDPOINTS } from './api-endpoints';
import {
  AddRouteRequest,
  AdminOperatorResponse,
  AdminRevenueResponse,
  AdminRouteResponse,
  OperatorDecisionRequest,
  OperatorRequestResponse
} from './api.models';

@Injectable({ providedIn: 'root' })
export class AdminApiService {
  constructor(private readonly http: HttpClient) {}

  listPendingOperators(): Observable<OperatorRequestResponse[]> {
    const params = new HttpParams().set('status', 'PENDING');
    return this.http.get<OperatorRequestResponse[]>(this.url(API_ENDPOINTS.admin.operatorRequests), { params });
  }

  getOperatorRequest(operatorId: number): Observable<OperatorRequestResponse> {
    return this.http.get<OperatorRequestResponse>(this.url(API_ENDPOINTS.admin.operatorRequest(operatorId)));
  }

  listRoutes(): Observable<AdminRouteResponse[]> {
    return this.http.get<AdminRouteResponse[]>(this.url(API_ENDPOINTS.admin.routes));
  }

  addRoute(payload: AddRouteRequest): Observable<void> {
    return this.http.post<void>(this.url(API_ENDPOINTS.admin.addRoute), payload);
  }

  listOperators(): Observable<AdminOperatorResponse[]> {
    return this.http.get<AdminOperatorResponse[]>(this.url(API_ENDPOINTS.admin.operators));
  }

  revenue(): Observable<AdminRevenueResponse> {
    return this.http.get<AdminRevenueResponse>(this.url(API_ENDPOINTS.admin.revenue));
  }

  approveOperator(operatorId: number, payload: OperatorDecisionRequest): Observable<void> {
    return this.http.post<void>(this.url(API_ENDPOINTS.admin.approveOperator(operatorId)), payload);
  }

  rejectOperator(operatorId: number, payload: OperatorDecisionRequest): Observable<void> {
    return this.http.post<void>(this.url(API_ENDPOINTS.admin.rejectOperator(operatorId)), payload);
  }

  disableOperator(operatorId: number, adminNotes: string): Observable<void> {
    return this.http.post<void>(this.url(API_ENDPOINTS.admin.disableOperator(operatorId)), {
      adminNotes
    });
  }

  private url(endpoint: string): string {
    return `${environment.apiBaseUrl}${endpoint}`;
  }
}
