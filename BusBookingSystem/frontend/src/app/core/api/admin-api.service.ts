import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { API_ENDPOINTS } from './api-endpoints';
import { OperatorDecisionRequest, OperatorRequestResponse } from './api.models';

@Injectable({ providedIn: 'root' })
export class AdminApiService {
  constructor(private readonly http: HttpClient) {}

  listOperatorRequests(status = 'PENDING'): Observable<OperatorRequestResponse[]> {
    const params = new HttpParams().set('status', status);
    return this.http.get<OperatorRequestResponse[]>(this.url(API_ENDPOINTS.admin.operatorRequests), { params });
  }

  getOperatorRequest(operatorId: number): Observable<OperatorRequestResponse> {
    return this.http.get<OperatorRequestResponse>(this.url(API_ENDPOINTS.admin.operatorRequest(operatorId)));
  }

  approveOperator(operatorId: number, payload: OperatorDecisionRequest): Observable<void> {
    return this.http.post<void>(this.url(API_ENDPOINTS.admin.approveOperator(operatorId)), payload);
  }

  rejectOperator(operatorId: number, payload: OperatorDecisionRequest): Observable<void> {
    return this.http.post<void>(this.url(API_ENDPOINTS.admin.rejectOperator(operatorId)), payload);
  }

  private url(endpoint: string): string {
    return `${environment.apiBaseUrl}${endpoint}`;
  }
}
