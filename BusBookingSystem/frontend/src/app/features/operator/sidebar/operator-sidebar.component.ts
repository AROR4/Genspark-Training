import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthApiService } from '../../../core/api';

@Component({
  selector: 'app-operator-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  template: `
    <aside class="sidebar">

      <!-- BRAND -->
      <div class="brand">
        <strong>Operator Panel</strong>
        <small>BusMate operations</small>
      </div>

      <!-- NAV -->
      <nav class="nav">
        <a routerLink="/operator/buses" routerLinkActive="active">Manage Buses</a>
        <a routerLink="/operator/offices" routerLinkActive="active">Manage Offices</a>
        <a routerLink="/operator/schedules" routerLinkActive="active">Manage Schedules</a>
        <a routerLink="/operator/bookings" routerLinkActive="active">Manage Bookings</a>
        <a routerLink="/operator/revenue" routerLinkActive="active">Revenue</a>
      </nav>

      <!-- LOGOUT -->
      <div class="logout-section">
        <button class="logout-btn" (click)="logout()">Logout</button>
      </div>

    </aside>
  `,
  styles: [`
    .sidebar {
      width: 280px;
      min-height: calc(100dvh - 32px);
      padding: 22px;
      border-radius: 24px;
      background: #020617;
      border: 1px solid rgba(148,163,184,0.16);
      display: flex;
      flex-direction: column;
    }

    .brand {
      display: grid;
      gap: 4px;
      margin-bottom: 20px;
    }

    .brand strong {
      color: #f8fafc;
      font-size: 1.15rem;
    }

    .brand small {
      color: #94a3b8;
    }

    .nav {
      display: grid;
      gap: 10px;
    }

    .nav a {
      min-height: 46px;
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 0 14px;
      border-radius: 14px;
      text-decoration: none;
      border: 1px solid transparent;
      color: #94a3b8;
      transition: all 0.2s ease;
    }

    .nav a::before {
      content: '';
      width: 8px;
      height: 8px;
      border-radius: 999px;
      background: rgba(148,163,184,0.65);
    }

    .nav a:hover {
      background: rgba(59,130,246,0.12);
      border-color: rgba(96,165,250,0.2);
      color: #e2e8f0;
      transform: translateX(2px);
    }

    .nav a.active {
      background: linear-gradient(135deg, rgba(37,99,235,0.28), rgba(79,70,229,0.22));
      border-color: rgba(96,165,250,0.35);
      color: #f8fafc;
    }

    .nav a.active::before {
      background: #60a5fa;
    }

    /* LOGOUT */
    .logout-section {
      margin-top: auto;
      padding-top: 20px;
      border-top: 1px solid rgba(148,163,184,0.1);
    }

    .logout-btn {
      width: 100%;
      min-height: 44px;
      border-radius: 12px;
      border: none;
      background: rgba(239,68,68,0.9);
      color: white;
      cursor: pointer;
      transition: all 0.2s ease;
    }

    .logout-btn:hover {
      background: #ef4444;
      transform: translateY(-1px);
    }

    @media (max-width: 980px) {
      .sidebar {
        width: 100%;
        min-height: auto;
      }
    }
  `]
})
export class OperatorSidebarComponent {
  private readonly router = inject(Router);
  private readonly authApi = inject(AuthApiService);

  logout(): void {
    if (!confirm('Are you sure you want to logout?')) return;

    this.authApi.logout();
    void this.router.navigate(['/login']);
  }
}
