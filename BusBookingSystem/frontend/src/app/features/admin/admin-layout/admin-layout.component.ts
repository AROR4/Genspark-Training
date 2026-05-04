import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AdminSidebarComponent } from '../sidebar/admin-sidebar.component';

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, AdminSidebarComponent],
  template: `
    <main class="layout">
      <app-admin-sidebar />
      <section class="content">
        <router-outlet />
      </section>
    </main>
  `,
  styles: [`
    .layout {
      min-height: 100dvh;
      padding: 16px;
      background: #0f172a;
      display: grid;
      grid-template-columns: 280px minmax(0, 1fr);
      gap: 16px;
    }

    .content {
      min-width: 0;
      padding: 24px;
      border-radius: 24px;
      background: #0f172a;
      border: 1px solid rgba(148,163,184,0.12);
      box-shadow: inset 0 1px 0 rgba(255,255,255,0.03);
    }

    @media (max-width: 980px) {
      .layout {
        grid-template-columns: 1fr;
      }

      .content {
        padding: 18px;
      }
    }
  `]
})
export class AdminLayoutComponent {}
