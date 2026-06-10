import { Component, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { User } from '../../Models/User';
import { AuthService } from '../../Services/auth.service';

@Component({
  selector: 'app-header',
  imports: [RouterLink],
  templateUrl: './header.html',
  styleUrl: './header.css'
})
export class Header {
  private authService = inject(AuthService);
  private router = inject(Router);
  user: User | null = null;

  constructor() {
    this.authService.userSubject
      .subscribe(user => {
        this.user = user;
      });
  }

  logout() {

  this.authService.logout();

  this.router.navigate(['/login']);

}
}