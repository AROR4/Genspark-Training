import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../Services/auth.service';

@Component({
  selector: 'app-dashboard',
  imports: [RouterLink],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class Dashboard {
  authService = inject(AuthService);
  userName = 'User';
  constructor() {
    this.authService.userSubject.subscribe(user => {
      if (user) {
        this.userName = user.firstName;
      }
    });
  }


}