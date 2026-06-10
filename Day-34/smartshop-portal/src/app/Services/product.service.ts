import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { LoginModel } from '../Models/LoginModel';
import { BehaviorSubject, map, Observable, tap } from 'rxjs';
import { User } from '../Models/User';
import { ProductModel } from '../Models/Product';

@Injectable({
  providedIn: 'root',
})
export class ProductService {
  private apiUrl = 'https://dummyjson.com/products';
 constructor (private http: HttpClient) {}
  getProductData(): Observable<ProductModel[]> {
  return this.http
    .get<any>('https://dummyjson.com/products')
    .pipe(
      map(response => response.products)
    );

    }

    getProductById(id: number) : Observable<ProductModel> {
    return this.http.get<ProductModel>(
        `https://dummyjson.com/products/${id}`
    );
}

}
