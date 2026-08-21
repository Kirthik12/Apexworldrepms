import { Component, OnInit, ViewEncapsulation, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { PaymentService } from '../../../core/services/payment.service';
import { PaymentInitiateRequestDto } from '../../../core/models/payment.model';
import { PropertyService } from '../../../core/services/property.service';
import { BookingService } from '../../../core/services/booking.service';
import { ToastService } from '../../../core/services/toast.service';
import { environment } from '../../../../environments/environment';
import { FormsModule } from '@angular/forms';
import { DecimalPipe } from '@angular/common';

@Component({
  selector: 'app-buyer-payments',
  templateUrl: './buyer-payments.component.html',
  styleUrls: ['./buyer-payments.component.css'],
  encapsulation: ViewEncapsulation.None,
  imports: [RouterLink, FormsModule, DecimalPipe],
})
export class BuyerPaymentComponent implements OnInit {
  propName: string = 'Unknown Property';
  propLoc: string = 'Unknown Location';
  propSub: string = 'Property';
  propImg: string = '/assets/images/placeholder.jpg';
  bookingId: number = 0;

  propSize: string = '📐 —';
  propBeds: string = '🛏️ —';
  propBaths: string = '🛀 —';

  baseAmount: number = 8200000;
  gst: number = 0;
  reg: number = 0;
  total: number = 0;
  payable: number = 10000;

  selectedMethod: string = 'loan';

  notices: { [key: string]: string } = {
    loan: 'ℹ️ You will be redirected to our loan partners page to check eligibility and apply for a loan.',
    netbanking:
      'ℹ️ You will be redirected to our secure Net Banking portal to complete the transaction.',
    card: 'ℹ️ You will be redirected to our Razorpay card payment gateway to enter card details.',
    upi: 'ℹ️ You will be prompted to scan a QR code or enter your UPI ID to complete the payment.',
  };

  btnTexts: { [key: string]: string } = {
    loan: 'Proceed to Apply Loan →',
    netbanking: 'Proceed to Pay via Net Banking →',
    card: 'Proceed to Pay via Card →',
    upi: 'Proceed to Pay via UPI →',
  };

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private paymentService: PaymentService,
    private propertyService: PropertyService,
    private bookingService: BookingService,
    private cdr: ChangeDetectorRef,
    private toastService: ToastService
  ) {}

  ngOnInit() {
    this.route.queryParams.subscribe((params) => {
      if (params['id']) {
        const parsedId = parseInt(params['id'], 10);
        if (!isNaN(parsedId)) {
          this.bookingId = parsedId;
          this.bookingService.getBuyerBookingById(this.bookingId).subscribe({
            next: (res) => {
              if (res.data) {
                const b = res.data;
                const prop = b.property || {};
                this.propName = prop.title || 'Unknown Property';
                this.propLoc = '📍 ' + (prop.address || 'Unknown Location');
                this.baseAmount = prop.price || 0;
                
                if (prop.images && prop.images.length > 0) {
                  const img = prop.images[0].imageUrl;
                  this.propImg = img.startsWith('http') ? img : `${environment.apiUrl.replace('/api/v1', '')}${img}`;
                } else {
                  this.propImg = '/assets/images/placeholder.jpg';
                }
                
                this.propBeds = prop.bedrooms ? '🛏️ ' + prop.bedrooms + ' Beds' : '🛏️ —';
                this.propBaths = prop.bathrooms ? '🛀 ' + prop.bathrooms + ' Baths' : '🛀 —';
                this.propSize = prop.areaSize ? '📐 ' + prop.areaSize + ' sq.ft' : '📐 —';
                this.propSub = prop.category?.name || 'Property';

                this.calculateTotals();
                this.cdr.detectChanges();
              }
            },
            error: (err) => {
              console.error('Failed to load booking details for payment', err);
            }
          });
        }
      }
      
      if (params['name']) this.propName = params['name'];
      if (params['loc']) this.propLoc = '📍 ' + params['loc'];
      if (params['price']) this.parsePrice(params['price']);
      if (params['img']) this.propImg = params['img'].replace('../../', '/');

      this.calculateTotals();
    });

    // Cross-tab synchronization listener: redirects this tab if payment is verified in the new tab
    window.addEventListener('storage', (event) => {
      if (event.key === 'payment_completed_sync') {
        try {
          const syncData = JSON.parse(event.newValue || '{}');
          if (String(syncData.bookingId) === String(this.bookingId)) {
            this.router.navigate(['/buyer-dashboard/bookings']);
          }
        } catch (e) {}
      }
    });

    this.startAutoSlide();
  }

  parsePrice(priceStr: string) {
    if (!priceStr) return;
    const lower = priceStr.toLowerCase();
    if (lower.includes('cr') || lower.includes('crore')) {
      const match = priceStr.match(/[\d.]+/);
      if (match) this.baseAmount = Math.round(parseFloat(match[0]) * 10000000);
    } else if (
      lower.includes('lakh') ||
      lower.includes('lakhs') ||
      lower.includes('lac') ||
      lower.includes('lacs')
    ) {
      const match = priceStr.match(/[\d.]+/);
      if (match) this.baseAmount = Math.round(parseFloat(match[0]) * 100000);
    } else {
      const num = parseInt(priceStr.replace(/\D/g, ''), 10);
      if (!isNaN(num)) this.baseAmount = num;
    }
  }

  calculateTotals() {
    this.gst = Math.round(this.baseAmount * 0.12);
    this.reg = Math.round(this.baseAmount * 0.03);
    this.total = this.baseAmount + this.gst + this.reg;
    this.payable = 10000;
  }

  isProcessing: boolean = false;

  proceedToPay() {
    if (this.selectedMethod === 'loan') {
      const fwdParams = {
        property: this.propName,
        loc: this.propLoc,
        price: this.baseAmount,
        img: this.propImg,
        bookingId: this.bookingId,
        method: this.selectedMethod,
      };
      this.router.navigate(['/buyer-dashboard/loan-application'], { queryParams: fwdParams });
      return;
    }

    this.isProcessing = true;

    const request: PaymentInitiateRequestDto = {
      bookingId: this.bookingId,
      paymentMethod: this.selectedMethod,
      paymentDetails: `Payment for property ${this.propName}`,
      buyerName: localStorage.getItem('username') || 'Buyer',
      phoneNumber: localStorage.getItem('userPhone') || '',
    };

    this.paymentService.initiatePayment(request).subscribe({
      next: (res: any) => {
        this.isProcessing = false;
        if (res.data && res.data.paymentLinkUrl) {
          this.toastService.info('Opening Razorpay checkout in a new tab...');
          window.open(res.data.paymentLinkUrl, '_blank');
        } else {
          this.toastService.info('Processing fallback payment...');
          setTimeout(() => {
            this.toastService.success('Payment successful! Redirecting to bookings...');
            this.router.navigate(['/buyer-dashboard/bookings']);
          }, 1500);
        }
      },
      error: (err: any) => {
        this.isProcessing = false;
        console.error('Payment initiation failed', err);
        this.toastService.error('Failed to initiate payment. Please try again.');
      },
    });
  }

  // Carousel logic
  curSlide = 0;
  carouselTimer: any;

  goSlide(n: number) {
    if (n < 0) n = 3;
    if (n > 3) n = 0;
    this.curSlide = n;
    const track = document.getElementById('pm-carousel-track');
    if (track) {
      track.style.transform = `translateX(-${this.curSlide * 25}%)`;
    }
  }

  startAutoSlide() {
    this.carouselTimer = setInterval(() => this.goSlide(this.curSlide + 1), 3000);
  }

  stopAutoSlide() {
    if (this.carouselTimer) {
      clearInterval(this.carouselTimer);
    }
  }
}
