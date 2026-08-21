import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AiCompanionService {
  private isOpenSubject = new BehaviorSubject<boolean>(false);
  private propertyIdSubject = new BehaviorSubject<number | null>(null);

  isOpen$ = this.isOpenSubject.asObservable();
  propertyId$ = this.propertyIdSubject.asObservable();

  open(propertyId: number | null = null) {
    this.propertyIdSubject.next(propertyId);
    this.isOpenSubject.next(true);
  }

  close() {
    this.isOpenSubject.next(false);
  }

  toggle(propertyId: number | null = null) {
    if (this.isOpenSubject.value) {
      this.close();
    } else {
      this.open(propertyId);
    }
  }

  setPropertyContext(propertyId: number | null) {
    this.propertyIdSubject.next(propertyId);
  }
}
