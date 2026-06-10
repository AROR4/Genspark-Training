import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { LoginModel } from '../Models/LoginModel';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { User } from '../Models/User';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  userSubject =new BehaviorSubject<User | null>(null);
  user$ = this.userSubject.asObservable();
  private apiUrl = 'https://dummyjson.com/auth/login';
  constructor(private http: HttpClient) {
    const storedUser = sessionStorage.getItem('loggedInUser');
    if (storedUser) {

      this.userSubject.next(
        JSON.parse(storedUser)
      );

    }
}

  getToken() {
    return sessionStorage.getItem('token');
  }

  login(data: LoginModel): Observable<User> {
    return this.http.post<User>(this.apiUrl,data)
    .pipe(
      tap(response => {
        sessionStorage.setItem('loggedInUser', JSON.stringify(response));
        if(response.accessToken) {
          console.log('Received token:', response.accessToken);
        sessionStorage.setItem('token', response.accessToken);
        }
        this.setUser(response);
      })
    );
  }

  setUser(user: any) {
  this.userSubject.next(user);
  }
   
  
  isLoggedIn(): boolean {
    return !!this.getToken();
}

  logout() {
  sessionStorage.removeItem('loggedInUser');
  sessionStorage.removeItem('token');

  this.userSubject.next(null);

}

}
