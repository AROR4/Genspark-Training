import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { API_ENDPOINTS } from './api-endpoints';

type RawCity = string | {
  cityId?: number | string | null;
  id?: number | string | null;
  cityName?: string | null;
  name?: string | null;
};

export interface CityOption {
  id: number;
  name: string;
}

@Injectable({ providedIn: 'root' })
export class CitiesApiService {
  constructor(private readonly http: HttpClient) {}

  list(): Observable<string[]> {
    return this.listOptions().pipe(map(items => items.map(item => item.name)));
  }

  listOptions(): Observable<CityOption[]> {
    return this.http.get<RawCity[]>(this.url(API_ENDPOINTS.common.cities)).pipe(
      map(items => {
        const options = items
          .map(item => this.normalize(item))
          .filter((item): item is CityOption => !!item);

        return options.filter((value, index, arr) => {
          return arr.findIndex(item => item.id === value.id || item.name.toLowerCase() === value.name.toLowerCase()) === index;
        });
      })
    );
  }

  private normalize(item: RawCity): CityOption | null {
    if (typeof item === 'string') {
      const name = item.trim();
      return name ? { id: -1, name } : null;
    }

    const name = String(item.cityName || item.name || '').trim();
    const rawId = item.cityId ?? item.id;
    const id = rawId == null ? -1 : Number(rawId);

    if (!name) {
      return null;
    }

    return { id: Number.isFinite(id) ? id : -1, name };
  }

  private url(endpoint: string): string {
    return `${environment.apiBaseUrl}${endpoint}`;
  }
}
