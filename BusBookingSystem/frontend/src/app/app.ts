import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ToastOutletComponent } from './shared/layout/toast-outlet.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, ToastOutletComponent],
  templateUrl: './app.html',
  styleUrls: ['./app.css']
})
export class AppComponent {}
