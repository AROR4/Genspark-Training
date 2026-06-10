import { Component, inject, signal } from '@angular/core';
import { LoginModel } from '../../Models/LoginModel';
import { form, minLength, required,FormField } from '@angular/forms/signals';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../Services/auth.service';
import { User } from '../../Models/User';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  imports: [FormsModule,FormField],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  private readonly authService= inject(AuthService);
  loginModel = signal(new LoginModel());
  progress = signal(false);
  errorMessage = signal('');
  loginForm = form(this.loginModel,(path)=>{
  required(path.username);
  required(path.password);
  minLength(path.username, 3);
  });

  constructor( private router: Router) {}
  handleLoginClick(event?: Event) {
    
  event?.preventDefault();
    this.errorMessage.set('');
  if (
    this.loginForm.username().errors().length > 0 ||
    this.loginForm.password().errors().length > 0
  ) {
    console.log('Form is Invalid');
    return;
  }
    this.progress.set(true);
    this.authService
    .login(this.loginModel())
    .subscribe({
      next: (response) => {
        this.progress.set(false);
        this.router.navigate(['/dashboard']);
      },
      error: (error) => {
        this.errorMessage.set(
          error?.error?.message ||
          'Invalid username or password'
        );
        this.progress.set(false);
      }});
  } 
}
