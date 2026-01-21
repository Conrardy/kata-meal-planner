import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';

export interface LogContext {
  [key: string]: unknown;
}

@Injectable({
  providedIn: 'root',
})
export class LoggingService {
  private readonly isProduction = environment.production;

  debug(message: string, context?: LogContext): void {
    if (!this.isProduction) {
      console.debug(this.formatMessage('DEBUG', message), context ?? '');
    }
  }

  info(message: string, context?: LogContext): void {
    if (!this.isProduction) {
      console.info(this.formatMessage('INFO', message), context ?? '');
    }
  }

  warn(message: string, context?: LogContext): void {
    console.warn(this.formatMessage('WARN', message), context ?? '');
  }

  error(message: string, context?: LogContext): void {
    console.error(this.formatMessage('ERROR', message), context ?? '');
  }

  private formatMessage(level: string, message: string): string {
    const timestamp = new Date().toISOString();
    return `[${timestamp}] [${level}] ${message}`;
  }
}
