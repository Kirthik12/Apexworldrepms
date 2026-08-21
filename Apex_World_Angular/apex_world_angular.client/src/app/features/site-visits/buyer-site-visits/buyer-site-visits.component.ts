import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import {
  FormBuilder,
  FormGroup,
  Validators,
  FormsModule,
  ReactiveFormsModule,
} from '@angular/forms';
import { BookingService } from '../../../core/services/booking.service';
import { PropertyService } from '../../../core/services/property.service';
import { PropertyDto } from '../../../core/models/property.model';
import { BookingRequestDto } from '../../../core/models/booking.model';
import { environment } from '../../../../environments/environment';
import { NgIf, NgFor, NgStyle, DecimalPipe, DatePipe } from '@angular/common';

@Component({
  selector: 'app-buyer-site-visits',
  templateUrl: './buyer-site-visits.component.html',
  styleUrls: ['./buyer-site-visits.component.css'],
  imports: [
    NgIf,
    RouterLink,
    FormsModule,
    ReactiveFormsModule,
    NgFor,
    NgStyle,
    DecimalPipe,
    DatePipe,
  ],
})
export class BuyerSiteVisitsComponent implements OnInit {
  currentStep = 1;
  propertyId!: number;
  property: PropertyDto | null = null;

  backendUrl = environment.apiUrl.replace('/api/v1', '');

  bookingForm!: FormGroup;
  today: Date = new Date();
  selectedDate: Date = new Date();
  selectedSlot: string = '11:00 AM - 12:00 PM';
  calendarDays: number[] = [];
  emptyPrefixDays: number[] = [];
  isSuccess = false;
  isSubmitting = false;
  successBookingId = '';

  availableSlots = [
    '09:00 AM - 10:00 AM',
    '10:00 AM - 11:00 AM',
    '11:00 AM - 12:00 PM',
    '12:00 PM - 01:00 PM',
    '02:00 PM - 03:00 PM',
    '03:00 PM - 04:00 PM',
    '04:00 PM - 05:00 PM',
  ];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private bookingService: BookingService,
    private propertyService: PropertyService,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.selectedDate.setDate(this.selectedDate.getDate() + 1);
    this.initForm();
    this.generateCalendar();
    this.route.queryParams.subscribe((params) => {
      if (params['id']) {
        this.propertyId = parseInt(params['id'], 10);
        this.loadProperty(this.propertyId);
      } else {
        alert('No property selected for booking.');
        this.router.navigate(['/buyer-dashboard/properties']);
      }
    });
  }

  initForm(): void {
    this.bookingForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: ['', Validators.required],
      permanentAddress: ['', Validators.required],
      purposeOfPurchase: ['Self Residence'],
    });
  }

  loadProperty(id: number): void {
    this.propertyService.getPropertyById(id).subscribe({
      next: (res) => {
        if (res.data) {
          this.property = res.data;
          this.cdr.detectChanges();
        }
      },
      error: () => {
        alert('Failed to load property details.');
        this.router.navigate(['/buyer-dashboard/properties']);
      },
    });
  }

  nextStep(): void {
    if (this.currentStep === 1) {
      // Validate Step 1 form
      Object.keys(this.bookingForm.controls).forEach((key) => {
        this.bookingForm.get(key)?.markAsTouched();
      });
      if (this.bookingForm.invalid) return;
    }

    if (this.currentStep < 3) {
      this.currentStep++;
    }
  }

  prevStep(): void {
    if (this.currentStep > 1) {
      this.currentStep--;
    }
  }

  selectSlot(slot: string): void {
    this.selectedSlot = slot;
  }

  // Very basic date logic for the dummy calendar UI
  nextMonth(): void {
    this.selectedDate = new Date(
      this.selectedDate.getFullYear(),
      this.selectedDate.getMonth() + 1,
      this.selectedDate.getDate(),
    );
    this.generateCalendar();
  }

  prevMonth(): void {
    this.selectedDate = new Date(
      this.selectedDate.getFullYear(),
      this.selectedDate.getMonth() - 1,
      this.selectedDate.getDate(),
    );
    this.generateCalendar();
  }

  generateCalendar(): void {
    const year = this.selectedDate.getFullYear();
    const month = this.selectedDate.getMonth();
    const daysInMonth = new Date(year, month + 1, 0).getDate();
    const firstDayIndex = new Date(year, month, 1).getDay();

    this.emptyPrefixDays = Array.from({ length: firstDayIndex }, (_, i) => i);
    this.calendarDays = Array.from({ length: daysInMonth }, (_, i) => i + 1);
  }

  isDateDisabled(day: number): boolean {
    const compareDate = new Date(this.selectedDate.getFullYear(), this.selectedDate.getMonth(), day);
    const currentDate = new Date(this.today.getFullYear(), this.today.getMonth(), this.today.getDate());
    return compareDate <= currentDate;
  }

  selectDate(day: number): void {
    if (this.isDateDisabled(day)) return;
    const d = new Date(this.selectedDate.getFullYear(), this.selectedDate.getMonth(), day);
    this.selectedDate = d;
  }

  confirmBooking(): void {
    if (!this.property || this.isSubmitting) return;
    this.isSubmitting = true;

    // Combine date and time slot logic
    const timeParts = this.selectedSlot.split(' - ')[0].split(':');
    let hour = parseInt(timeParts[0], 10);
    const minute = parseInt(timeParts[1].substring(0, 2), 10);
    const ampm = timeParts[1].substring(3, 5);
    if (ampm === 'PM' && hour !== 12) hour += 12;
    if (ampm === 'AM' && hour === 12) hour = 0;

    const scheduledDate = new Date(
      Date.UTC(
        this.selectedDate.getFullYear(),
        this.selectedDate.getMonth(),
        this.selectedDate.getDate(),
        hour,
        minute,
        0,
        0,
      ),
    );

    const formValues = this.bookingForm.value;

    const request: BookingRequestDto = {
      propertyId: this.property.id,
      scheduledDate: scheduledDate.toISOString(),
      firstName: formValues.firstName,
      lastName: formValues.lastName,
      email: formValues.email,
      phoneNumber: formValues.phoneNumber,
      permanentAddress: formValues.permanentAddress,
    };

    this.bookingService.createBooking(request).subscribe({
      next: (res) => {
        if (res.data) {
          this.isSuccess = true;
          this.successBookingId = `AWB#${res.data.id.toString().padStart(4, '0')}`;
        }
        this.isSubmitting = false;
      },
      error: (err) => {
        console.error(err);
        const msg = err.error?.message || 'Failed to schedule site visit. Please try again.';
        alert(msg);
        this.isSubmitting = false;
      },
    });
  }

  // Formatting helpers for sidebar
  get taxBaseValue(): number {
    return this.property ? this.property.price : 0;
  }
  get gstRate(): number {
    return 0.12;
  } // Simplified
  get gstValue(): number {
    return this.taxBaseValue * this.gstRate;
  }
  get regValue(): number {
    return this.taxBaseValue * 0.03;
  }
  get totalValue(): number {
    return this.taxBaseValue + this.gstValue + this.regValue;
  }

  get coverImage(): string {
    if (this.property && this.property.images && this.property.images.length > 0) {
      const img = this.property.images[0].imageUrl;
      return img.startsWith('http') ? img : `${this.backendUrl}${img}`;
    }
    return '../../../assets/images/no_image_icon.png';
  }
}
