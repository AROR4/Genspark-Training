import { CommonModule } from '@angular/common';
import { Component, computed, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthApiService } from '../../core/api';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  template: `
    <header class="app-header">
      <a class="brand" routerLink="/user/search">
        <span class="brand-mark"></span>
        <span>BusHub</span>
      </a>

      <nav class="header-nav">
        @if (role() === 'user') {
          <a routerLink="/user/search" routerLinkActive="active">Search</a>
          <a routerLink="/user/tickets" routerLinkActive="active">Tickets</a>
        }

        @if (role() === 'operator') {
          <a routerLink="/operator/dashboard" routerLinkActive="active">Operator</a>
        }

        @if (role() === 'admin') {
          <a routerLink="/admin/dashboard" routerLinkActive="active">Admin</a>
        }
      </nav>

      <button class="session-pill" type="button" (click)="logout()">
        {{ currentUser()?.name || currentUser()?.email || 'Logout' }}
      </button>
    </header>
  `,
  styles: [`
    .app-header {
      position: sticky;
      top: 0;
      z-index: 10;
      display: grid;
      grid-template-columns: auto 1fr auto;
      align-items: center;
      gap: 16px;
      padding: 18px 24px;
      border-bottom: 1px solid rgba(255,255,255,0.08);
      background: rgba(6, 8, 10, 0.88);
      backdrop-filter: blur(18px);
    }

    .brand,
    .header-nav a,
    .session-pill {
      color: #f8fafc;
      text-decoration: none;
      font: inherit;
    }

    .brand {
      display: inline-flex;
      align-items: center;
      gap: 10px;
      font-weight: 800;
    }

    .brand-mark {
      width: 28px;
      height: 28px;
      border-radius: 8px;
      background: linear-gradient(135deg, #ef4444, #2563eb);
    }

    .header-nav {
      display: flex;
      gap: 10px;
      justify-content: center;
      flex-wrap: wrap;
    }

    .header-nav a,
    .session-pill {
      min-height: 40px;
      display: inline-flex;
      align-items: center;
      padding: 0 14px;
      border-radius: 999px;
      background: rgba(255,255,255,0.08);
    }

    .header-nav a.active {
      background: #2563eb;
    }

    .session-pill {
      border: 0;
      cursor: pointer;
    }

    @media (max-width: 760px) {
      .app-header {
        grid-template-columns: 1fr;
      }

      .header-nav {
        justify-content: start;
      }
    }
  `]
})
export class AppHeaderComponent {
  private readonly authApi = inject(AuthApiService);
  private readonly router = inject(Router);

  readonly currentUser = computed(() => this.authApi.currentUser());
  readonly role = computed(() => this.authApi.role());

  logout(): void {
    this.authApi.logout();
    void this.router.navigate(['/login']);
  }
}
