import { Injectable, signal, computed } from '@angular/core';

export interface ErrorNotification {
  id: string;
  message: string;
  timestamp: Date;
}

@Injectable({
  providedIn: 'root',
})
export class ErrorNotificationService {
  private readonly notifications = signal<ErrorNotification[]>([]);
  private readonly dismissTimeoutMs = 5000;

  readonly currentError = computed(() => {
    const all = this.notifications();
    return all.length > 0 ? all[0] : null;
  });

  readonly hasError = computed(() => this.notifications().length > 0);

  showError(message: string): void {
    const notification: ErrorNotification = {
      id: this.generateId(),
      message,
      timestamp: new Date(),
    };

    this.notifications.update((current) => [...current, notification]);

    setTimeout(() => {
      this.dismiss(notification.id);
    }, this.dismissTimeoutMs);
  }

  dismiss(id: string): void {
    this.notifications.update((current) =>
      current.filter((n) => n.id !== id)
    );
  }

  dismissAll(): void {
    this.notifications.set([]);
  }

  private generateId(): string {
    return `${Date.now()}-${Math.random().toString(36).substring(2, 9)}`;
  }
}
