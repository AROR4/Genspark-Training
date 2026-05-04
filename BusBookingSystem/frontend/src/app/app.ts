import { Component, inject } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { AuthApiService } from './core/api';
import { ToastOutletComponent } from './shared/layout/toast-outlet.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, ToastOutletComponent],
  templateUrl: './app.html',
  styleUrls: ['./app.css']
})
export class AppComponent {
  private readonly authApi = inject(AuthApiService);
  private readonly router = inject(Router);

  constructor() {
    queueMicrotask(() => this.redirectFromStoredToken());
  }

  private redirectFromStoredToken(): void {
    const token = localStorage.getItem('token');
    const role = this.authApi.role();

    if (!token || !role) {
      return;
    }

    const currentPath = this.router.url.split('?')[0] || '/';
    const authEntryPaths = new Set(['/', '/login', '/signup', '/admin/login', '/operator/register']);

    if (!authEntryPaths.has(currentPath)) {
      return;
    }

    const target = role === 'admin'
      ? '/admin/approvals'
      : role === 'operator'
        ? '/operator/buses'
        : '/';

    if (currentPath !== target) {
      void this.router.navigateByUrl(target);
    }
  }
}
