import { Component, inject, Signal, signal } from '@angular/core';
import { User } from '../../Models/User';
import { AuthService } from '../../Services/auth.service';

@Component({
  selector: 'app-profile',
  imports: [],
  templateUrl: './profile.html',
  styleUrl: './profile.css',
})
export class Profile {
  private authService =inject(AuthService);
  user = signal<User | null>(null);

  constructor() {}

  ngOnInit() {
    this.authService.userSubject.subscribe(user => {
      if (user) {
        this.user.set(user);
      }
    });
  }
}
