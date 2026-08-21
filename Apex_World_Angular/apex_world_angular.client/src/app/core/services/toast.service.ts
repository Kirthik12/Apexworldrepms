import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface Toast {
  id: number;
  message: string;
  type: 'success' | 'error' | 'warning' | 'info';
  duration?: number;
}

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  private toastsSubject = new BehaviorSubject<Toast[]>([]);
  public toasts$ = this.toastsSubject.asObservable();
  private nextId = 0;

  show(message: string, type: 'success' | 'error' | 'warning' | 'info' = 'info', duration = 4000) {
    if (type === 'error' && duration === 4000) duration = 6000;
    if (type === 'warning' && duration === 4000) duration = 5000;
    
    // Check for duplicate toasts
    const currentToasts = this.toastsSubject.value;
    const isDuplicate = currentToasts.some(t => t.message === message && t.type === type);
    if (isDuplicate) return;

    const toast: Toast = { id: this.nextId++, message, type, duration };
    this.toastsSubject.next([...currentToasts, toast]);

    if (duration > 0) {
      setTimeout(() => this.remove(toast.id), duration);
    }
  }

  success(message: string) { this.show(message, 'success'); }
  error(message: string) { this.show(message, 'error'); }
  warning(message: string) { this.show(message, 'warning'); }
  info(message: string) { this.show(message, 'info'); }

  remove(id: number) {
    this.toastsSubject.next(this.toastsSubject.value.filter(t => t.id !== id));
  }
}
