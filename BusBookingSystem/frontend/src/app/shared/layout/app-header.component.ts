import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthApiService } from '../../core/api';

@Component({
  selector: 'app-header',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, RouterLink],
  template: `
    <header class="header" (click)="closeProfileMenu()">
      <a routerLink="/" class="brand">BusMate</a>

      @if (isAuthenticated()) {
        <div class="profile-group" (click)="$event.stopPropagation()">
          <button
            type="button"
            class="profile"
            (click)="toggleProfileMenu()"
            [attr.aria-expanded]="profileMenuOpen()"
            aria-haspopup="menu"
          >
            <span class="profile-avatar">{{ initials() }}</span>
            <span class="profile-name">{{ displayName() }}</span>
          </button>

          @if (profileMenuOpen()) {
            <section class="profile-menu" role="menu" aria-label="Profile menu">
              <p class="menu-title">Profile</p>
              <p class="menu-name">{{ displayName() }}</p>
              <p class="menu-line">{{ displayEmail() }}</p>

              <div class="menu-actions">
                <a routerLink="/ticket" (click)="profileMenuOpen.set(false)">My Tickets</a>
                <button type="button" class="danger" (click)="logout()">Logout</button>
              </div>
            </section>
          }
        </div>
      } @else {
        <div class="actions">
          <a routerLink="/login">Login</a>
          <a routerLink="/operator/register">Be a Bus Operator</a>
        </div>
      }
    </header>
  `,
  styles: [`
    .header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 14px 22px;
      border-bottom: 1px solid rgba(148,163,184,0.22);
      background: rgba(2,6,23,0.9);
      backdrop-filter: blur(10px);
      position: sticky;
      top: 0;
      z-index: 20;
    }

    .brand { text-decoration: none; color: #f8fafc; font-size: 1.25rem; font-weight: 800; letter-spacing: 0.05em; }

    .actions, .profile-group { display: flex; gap: 8px; align-items: center; }
    .profile-group { position: relative; }

    .actions a, .profile {
      min-height: 40px;
      padding: 0 14px;
      border-radius: 999px;
      border: 1px solid rgba(148,163,184,0.35);
      color: #e2e8f0;
      background: rgba(30,41,59,0.82);
      text-decoration: none;
      font: inherit;
      cursor: pointer;
    }

    .profile {
      display: inline-flex;
      align-items: center;
      gap: 10px;
      padding: 0 12px 0 8px;
      background: rgba(8, 47, 73, 0.88);
      border-color: rgba(125, 211, 252, 0.32);
      font-weight: 700;
    }

    .profile-avatar {
      width: 28px;
      height: 28px;
      border-radius: 50%;
      display: inline-grid;
      place-items: center;
      color: #f8fafc;
      background: linear-gradient(135deg, #16a34a, #0ea5e9);
      flex: 0 0 auto;
    }

    .profile-name {
      max-width: 160px;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .profile-menu {
      position: absolute;
      top: calc(100% + 10px);
      right: 0;
      min-width: 260px;
      padding: 14px;
      border-radius: 14px;
      border: 1px solid rgba(148,163,184,0.3);
      background: #0b1325;
      box-shadow: 0 20px 45px rgba(2,6,23,0.45);
      display: grid;
      gap: 6px;
      z-index: 30;
    }

    .menu-title {
      margin: 0;
      color: #94a3b8;
      font-size: 0.8rem;
      text-transform: uppercase;
      letter-spacing: 0.06em;
    }

    .menu-name {
      margin: 0;
      color: #f8fafc;
      font-size: 1rem;
      font-weight: 700;
    }

    .menu-line {
      margin: 0;
      color: #cbd5e1;
      font-size: 0.9rem;
      word-break: break-word;
    }

    .menu-actions {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: 8px;
      margin-top: 8px;
    }

    .menu-actions a,
    .menu-actions button {
      min-height: 38px;
      border-radius: 10px;
      border: 1px solid rgba(148,163,184,0.3);
      background: rgba(30,41,59,0.9);
      color: #e2e8f0;
      text-decoration: none;
      font: inherit;
      display: inline-grid;
      place-items: center;
      cursor: pointer;
    }

    .menu-actions .danger {
      border-color: rgba(239,68,68,0.35);
      color: #fecaca;
      background: rgba(127,29,29,0.35);
    }

    @media (max-width: 620px) {
      .actions a { font-size: 0.86rem; }
      .profile-name { display: none; }
      .profile { padding-right: 8px; }
      .menu-actions { grid-template-columns: 1fr; }
    }
  `]
})
export class AppHeaderComponent {
  private readonly authApi = inject(AuthApiService);
  private readonly router = inject(Router);

  readonly currentUser = computed(() => this.authApi.currentUser());
  readonly isAuthenticated = computed(() => this.authApi.isAuthenticated());
  readonly profileMenuOpen = signal(false);

  readonly displayName = computed(() => {
    const user = this.currentUser();
    return user?.name?.trim() || user?.email?.trim() || 'Passenger';
  });

  readonly displayEmail = computed(() => {
    return this.currentUser()?.email?.trim() || 'No email available';
  });
  initials(): string {
    const name = this.currentUser()?.name || this.currentUser()?.email || 'P';
    return name.trim().slice(0, 1).toUpperCase();
  }

  toggleProfileMenu(): void {
    this.profileMenuOpen.update(value => !value);
  }

  closeProfileMenu(): void {
    this.profileMenuOpen.set(false);
  }

  logout(): void {
    this.authApi.logout();
    this.profileMenuOpen.set(false);
    void this.router.navigate(['/login']);
  }
}
