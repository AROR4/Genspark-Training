import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ToastService } from '../../core/utils/toast.service';

@Component({
  selector: 'app-toast-outlet',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="toast-stack">
      @for (toast of toastService.toasts(); track toast.id) {
        <article class="toast" [class.error]="toast.tone === 'error'" [class.success]="toast.tone === 'success'">
          <span>{{ toast.message }}</span>
          <button type="button" (click)="toastService.dismiss(toast.id)">×</button>
        </article>
      }
    </div>
  `,
  styles: [`
    .toast-stack {
      position: fixed;
      top: 18px;
      right: 18px;
      z-index: 1000;
      width: min(380px, calc(100vw - 24px));
      display: grid;
      gap: 10px;
    }

    .toast {
      display: grid;
      grid-template-columns: 1fr auto;
      gap: 12px;
      align-items: start;
      padding: 14px 16px;
      color: #e2e8f0;
      border: 1px solid rgba(255, 255, 255, 0.12);
      border-radius: 16px;
      background: rgba(15, 23, 42, 0.94);
      box-shadow: 0 14px 40px rgba(0, 0, 0, 0.35);
      backdrop-filter: blur(16px);
    }

    .toast.success {
      border-color: rgba(34, 197, 94, 0.35);
    }

    .toast.error {
      border-color: rgba(239, 68, 68, 0.35);
    }

    .toast button {
      width: 28px;
      height: 28px;
      color: inherit;
      border: 0;
      border-radius: 999px;
      background: rgba(255, 255, 255, 0.08);
      font: inherit;
      cursor: pointer;
    }
  `]
})
export class ToastOutletComponent {
  readonly toastService = inject(ToastService);
}
