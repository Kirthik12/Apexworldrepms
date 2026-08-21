import { Component, AfterViewInit, ViewEncapsulation, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { LoanService, CreateLoanDto } from '../../../core/services/loan.service';
import { BookingService } from '../../../core/services/booking.service';
import { NgIf, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-loan-application',
  templateUrl: './loan-application.component.html',
  styleUrls: ['./loan-application.component.css'],
  encapsulation: ViewEncapsulation.None,
  imports: [RouterLink, NgIf, DecimalPipe, FormsModule],
})
export class LoanApplicationComponent implements OnInit, AfterViewInit, OnDestroy {
  bookingId: number | null = null;
  property: any = null;

  // Property & Price details
  propName: string = 'Skyline Heights';
  propLoc: string = 'OMR, Chennai';
  propSub: string = '3 BHK Apartment';
  propImg: string = '/assets/images/skyline_heights.png';
  propSize: string = '1650 sq.ft.';
  propBeds: string = '3 Beds';
  propBaths: string = '2 Baths';

  baseAmount: number = 8200000;
  gst: number = 984000;
  reg: number = 246000;
  total: number = 9430000;
  payable: number = 10000;
  remaining: number = 9420000;

  // Wizard state
  currentStep: number = 1;
  isApplying: boolean = false;
  successReferenceId: string = '';

  // Step 1
  fullName: string = '';
  mobile: string = '';
  email: string = '';
  dob: string = '';
  consent: boolean = false;

  // Step 2
  pan: string = '';
  aadhaar: string = '';
  gender: string = '';
  maritalStatus: string = '';
  residentialAddress: string = '';
  residentType: string = 'Resident Indian';
  yearsAtAddress: string = 'Less than 1 Year';

  // Step 3
  employmentType: string = '';
  companyName: string = '';
  designation: string = '';
  workExperience: string = '';
  monthlyIncome: number = 120000;
  otherIncome: number = 0;
  existingEmi: number = 0;
  cibilScore: string = '';

  // Step 4
  loanAmount: number = 5000000;
  tenureYears: number = 20;
  interestRate: number = 8.6;
  emiResult: number = 0;

  // Step 5
  bankName: string = 'State Bank of India';
  accountNumber: string = '';
  ifscCode: string = '';
  loanPurpose: string = 'Purchase of New Property';
  declarationAccepted: boolean = false;

  errors: { [key: string]: string } = {};

  curSlide = 0;
  carouselTimer: any;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private loanService: LoanService,
    private bookingService: BookingService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.route.queryParams.subscribe((params) => {
      if (params['property']) {
        this.propName = params['property'];
      }
      if (params['loc']) {
        this.propLoc = params['loc'];
      }
      if (params['img']) {
        const img = params['img'];
        this.propImg = img.startsWith('http') ? img : `${environment.apiUrl.replace('/api/v1', '')}${img}`;
      }
      if (params['bookingId']) {
        const rawBookingId = String(params['bookingId'] || '');
        const parsed = parseInt(rawBookingId.replace(/\D/g, ''), 10);
        if (!isNaN(parsed)) {
          this.bookingId = parsed;
          this.loadBookingDetails(parsed);
        }
      }
      if (params['price']) {
        this.baseAmount = parseInt(params['price'], 10);
        this.calculateTotals();
      }
    });
  }

  ngAfterViewInit() {
    this.startAutoSlide();
  }

  ngOnDestroy() {
    this.stopAutoSlide();
  }

  loadBookingDetails(id: number) {
    this.bookingService.getBuyerBookingById(id).subscribe({
      next: (res) => {
        if (res.data) {
          const b = res.data;
          this.property = b.property;
          if (this.property) {
            this.propName = this.property.title || 'Unknown Property';
            this.propLoc = '📍 ' + (this.property.address || 'Unknown Location');
            this.propSub = this.property.category?.name || 'Property';
            this.baseAmount = this.property.price || 0;
            this.propSize = this.property.areaSize ? this.property.areaSize + ' sq.ft' : '—';
            this.propBeds = this.property.bedrooms ? this.property.bedrooms + ' Beds' : '—';
            this.propBaths = this.property.bathrooms ? this.property.bathrooms + ' Baths' : '—';
            if (this.property.images && this.property.images.length > 0) {
              const img = this.property.images[0].imageUrl;
              this.propImg = img.startsWith('http') ? img : `${environment.apiUrl.replace('/api/v1', '')}${img}`;
            } else {
              this.propImg = '/assets/images/placeholder.jpg';
            }
            this.calculateTotals();
            this.cdr.detectChanges();
          }
        }
      },
      error: (err) => console.error('Failed to load booking details', err)
    });
  }

  calculateTotals() {
    this.gst = Math.round(this.baseAmount * 0.12);
    this.reg = Math.round(this.baseAmount * 0.03);
    this.total = this.baseAmount + this.gst + this.reg;
    this.payable = 10000;
    this.remaining = this.total - this.payable;
    this.loanAmount = this.remaining;
    this.calcEMI();
  }

  calcEMI() {
    const P = this.loanAmount || this.remaining;
    const r = (this.interestRate || 8.6) / 12 / 100;
    const n = (this.tenureYears || 20) * 12;
    const emi = (P * r * Math.pow(1 + r, n)) / (Math.pow(1 + r, n) - 1);
    this.emiResult = isFinite(emi) ? Math.round(emi) : 0;
  }

  clearError(field: string) {
    this.errors[field] = '';
  }

  goToStep2() {
    let valid = true;
    if (!this.fullName.trim()) { this.errors['fullName'] = 'Full name is required.'; valid = false; }
    if (!this.mobile.trim()) { this.errors['mobile'] = 'Mobile number is required.'; valid = false; }
    else if (!/^[6789]\d{9}$/.test(this.mobile.trim())) { this.errors['mobile'] = 'Enter a valid 10-digit mobile number.'; valid = false; }
    if (!this.email.trim()) { this.errors['email'] = 'Email address is required.'; valid = false; }
    else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/i.test(this.email.trim())) { this.errors['email'] = 'Enter a valid email address.'; valid = false; }
    if (!this.dob) { this.errors['dob'] = 'Date of birth is required.'; valid = false; }
    else {
      const birth = new Date(this.dob);
      const today = new Date();
      let age = today.getFullYear() - birth.getFullYear();
      const m = today.getMonth() - birth.getMonth();
      if (m < 0 || (m === 0 && today.getDate() < birth.getDate())) age--;
      if (age < 21) { this.errors['dob'] = 'You must be at least 21 years old to apply.'; valid = false; }
    }
    if (!this.consent) { alert('Please authorize the credit profile access consent to proceed.'); valid = false; }
    if (valid) { this.currentStep = 2; window.scrollTo({ top: 0, behavior: 'smooth' }); }
  }

  goToStep3() {
    let valid = true;
    if (!this.pan.trim()) { this.errors['pan'] = 'PAN number is required.'; valid = false; }
    else if (!/^[A-Z]{5}[0-9]{4}[A-Z]{1}$/.test(this.pan.toUpperCase().trim())) { this.errors['pan'] = 'Invalid PAN format. E.g. ABCDE1234F'; valid = false; }
    if (!this.aadhaar.trim()) { this.errors['aadhaar'] = 'Aadhaar number is required.'; valid = false; }
    else if (!/^[2-9][0-9]{11}$/.test(this.aadhaar.replace(/\s/g, ''))) { this.errors['aadhaar'] = 'Aadhaar must be 12 digits.'; valid = false; }
    if (!this.gender) { this.errors['gender'] = 'Gender selection is required.'; valid = false; }
    if (!this.residentialAddress.trim()) { this.errors['residentialAddress'] = 'Residential address is required.'; valid = false; }
    if (valid) { this.currentStep = 3; window.scrollTo({ top: 0, behavior: 'smooth' }); }
  }

  goToStep4() {
    let valid = true;
    if (!this.employmentType) { this.errors['employmentType'] = 'Please select employment type.'; valid = false; }
    if (!this.companyName.trim()) { this.errors['companyName'] = 'Company name is required.'; valid = false; }
    if (!this.designation.trim()) { this.errors['designation'] = 'Designation is required.'; valid = false; }
    if (!this.workExperience) { this.errors['workExperience'] = 'Please select experience.'; valid = false; }
    if (!this.monthlyIncome || this.monthlyIncome < 10000) { this.errors['monthlyIncome'] = 'Gross monthly income must be at least ₹10,000.'; valid = false; }
    if (!this.cibilScore) { this.errors['cibilScore'] = 'Please select CIBIL score range.'; valid = false; }
    if (valid) { this.currentStep = 4; this.calcEMI(); window.scrollTo({ top: 0, behavior: 'smooth' }); }
  }

  submitLoanApplication() {
    if (!this.bookingId) { alert('No active booking reference found. Please select a booking first.'); return; }
    if (!this.accountNumber || this.accountNumber.length < 6) { alert('Please enter a valid bank account number.'); return; }
    if (!this.ifscCode || this.ifscCode.length !== 11) { alert('Please enter a valid 11-character IFSC code.'); return; }
    if (!this.declarationAccepted) { alert('Please accept the declaration before submitting.'); return; }
    this.isApplying = true;
    const dto: CreateLoanDto = {
      bookingId: this.bookingId,
      loanAmount: this.loanAmount || this.remaining,
      tenureYears: Number(this.tenureYears),
      employmentType: this.employmentType,
      monthlyIncome: this.monthlyIncome,
      bankName: this.bankName
    };
    this.loanService.applyForLoan(dto).subscribe({
      next: (res) => {
        this.isApplying = false;
        const refNum = res.data?.id ? `LOAN-2026-00${res.data.id}` : `LOAN-2026-0001`;
        this.successReferenceId = refNum;
        this.currentStep = 6;
        window.scrollTo({ top: 0, behavior: 'smooth' });
      },
      error: (err) => {
        console.error('Loan submission failed', err);
        alert(err.error?.message || 'Failed to submit loan application. Please try again.');
        this.isApplying = false;
      }
    });
  }

  goSlide(n: number) { this.curSlide = n; }
  startAutoSlide() { this.carouselTimer = setInterval(() => { this.curSlide = (this.curSlide + 1) % 4; }, 4000); }
  stopAutoSlide() { if (this.carouselTimer) { clearInterval(this.carouselTimer); } }
}
