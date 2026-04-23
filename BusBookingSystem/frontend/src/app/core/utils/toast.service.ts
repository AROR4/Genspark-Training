import { Injectable, signal } from '@angular/core';

export interface ToastItem {
  id: number;
  tone: 'success' | 'error' | 'info';
  message: string;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  readonly toasts = signal<ToastItem[]>([]);
  private nextId = 1;

  show(message: string, tone: ToastItem['tone'] = 'info', timeout = 4200): void {
    const id = this.nextId++;
    this.toasts.update(items => [...items, { id, tone, message }]);

    setTimeout(() => {
      this.dismiss(id);
    }, timeout);
  }

  success(message: string): void {
    this.show(message, 'success');
  }

  error(message: string): void {
    this.show(message, 'error', 5600);
  }

  info(message: string): void {
    this.show(message, 'info');
  }

  dismiss(id: number): void {
    this.toasts.update(items => items.filter(item => item.id !== id));
  }
}
