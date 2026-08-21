import { Component, OnInit, ViewEncapsulation, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { DecimalPipe, NgIf } from '@angular/common';
import { PaymentService } from '../../../core/services/payment.service';

@Component({
  selector: 'app-payment-success',
  templateUrl: './payment-success.component.html',
  styleUrls: ['./payment-success.component.css'],
  encapsulation: ViewEncapsulation.None,
  imports: [RouterLink, DecimalPipe, NgIf],
})
export class PaymentSuccessComponent implements OnInit {
  bookingId: string = '';
  paymentId: string = '';
  amount: number = 10000;
  dateStr: string = '';
  verifying: boolean = false;
  verified: boolean = true;

  constructor(
    private route: ActivatedRoute, 
    private router: Router,
    private paymentService: PaymentService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.route.queryParams.subscribe((params) => {
      this.bookingId = params['bookingId'] || 'N/A';
      this.paymentId = params['razorpay_payment_id'] || 'Processing...';
      
      this.dateStr = new Date().toLocaleDateString('en-IN', {
        day: 'numeric',
        month: 'long',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
      });

      const plinkId = params['razorpay_payment_link_id'];
      if (plinkId) {
        this.verifying = true;
        this.verified = false;
        this.cdr.detectChanges();

        this.paymentService.verifyPayment(plinkId).subscribe({
          next: (res: any) => {
            this.verifying = false;
            this.verified = true;
            if (res?.data?.transactionId) {
              this.paymentId = res.data.transactionId;
            }
            this.cdr.detectChanges();
            // Broadcast success to refresh bookings lists in other open tabs
            localStorage.setItem('payment_completed_sync', JSON.stringify({ bookingId: this.bookingId, time: Date.now() }));
          },
          error: (err) => {
            console.error('Failed to verify payment', err);
            this.verifying = false;
            this.verified = false;
            this.cdr.detectChanges();
          }
        });
      }
    });
  }
}
