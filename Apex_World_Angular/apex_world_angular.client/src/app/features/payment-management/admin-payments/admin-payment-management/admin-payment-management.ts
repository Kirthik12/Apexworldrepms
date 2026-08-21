import { Component, OnInit, ViewEncapsulation, ChangeDetectorRef } from '@angular/core';
import { AdminHeader } from '../../../../shared/components/admin-header/admin-header';
import { FormsModule } from '@angular/forms';
import { NgIf, NgFor, NgStyle, DecimalPipe, DatePipe, LowerCasePipe } from '@angular/common';
import { PaymentService } from '../../../../core/services/payment.service';
import { ToastService } from '../../../../core/services/toast.service';
import { PaginationComponent } from '../../../../shared/components/pagination/pagination';

export interface Payment {
  id: number;
  bookingId: number;
  propertyId: number;
  buyerId: number;
  amount: number;
  paymentMethod: string;
  status: string;
  transactionId: string;
  createdAt: string;

  propertyName?: string;
  propertyValue?: number;
  buyerName?: string;
  bookingRef?: string;
}

@Component({
  selector: 'app-admin-payment-management',
  templateUrl: './admin-payment-management.html',
  styleUrls: ['./admin-payment-management.css'],
  standalone: true,
  encapsulation: ViewEncapsulation.None,
  imports: [AdminHeader, FormsModule, NgIf, NgFor, DecimalPipe, DatePipe, LowerCasePipe, PaginationComponent],
})
export class AdminPaymentManagement implements OnInit {
  payments: Payment[] = [];
  allPayments: Payment[] = [];

  // Pagination State
  pageNumber: number = 1;
  pageSize: number = 7;
  totalItems: number = 0;
  get totalPages(): number {
    return Math.ceil(this.totalItems / this.pageSize) || 1;
  }

  // KPIs
  totalRevenue: number = 0;
  totalTax: number = 0;
  failedTransactions: number = 0;

  // Modal State
  selectedPayment: Payment | null = null;
  refundTxnId: string = '';

  constructor(
    private paymentService: PaymentService,
    private cdr: ChangeDetectorRef,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.loadPayments();
  }

  loadPayments(): void {
    this.paymentService.getAdminPayments().subscribe({
      next: (res: any) => {
        if (res?.data) {
          this.allPayments = res.data.map((p: any) => ({
            ...p,
            propertyName: p.booking?.property?.title || 'Unknown Property',
            propertyValue: p.booking?.property?.price || 0,
            buyerName: p.booking ? `${p.booking.firstName} ${p.booking.lastName}` : 'Unknown Buyer',
            bookingRef: p.booking ? `AWB#${p.booking.id}00` : 'N/A'
          }));
          this.calculateKPIs();
          this.applyPagination();
        }
      },
      error: (err: any) => {
        console.error('Failed to load payments:', err);
      },
    });
  }

  applyPagination(): void {
    this.totalItems = this.allPayments.length;
    const startIndex = (this.pageNumber - 1) * this.pageSize;
    this.payments = this.allPayments.slice(startIndex, startIndex + this.pageSize);
    this.cdr.detectChanges();
  }

  changePage(newPage: number): void {
    if (newPage < 1 || newPage > this.totalPages) return;
    this.pageNumber = newPage;
    this.applyPagination();
  }

  onPageChange(page: number): void {
    this.pageNumber = page;
    this.applyPagination();
  }

  calculateKPIs(): void {
    this.totalRevenue = 0;
    this.failedTransactions = 0;

    this.allPayments.forEach((p) => {
      if (p.status === 'Success') {
        this.totalRevenue += p.amount;
      } else if (p.status === 'Failed') {
        this.failedTransactions++;
      }
    });

    // Total tax and registration estimation
    const completedValuations = this.allPayments
      .filter((p) => p.status === 'Success')
      .reduce((sum, p) => sum + (p.propertyValue || 0), 0);
    this.totalTax = completedValuations * 0.15; // Est 15% Tax & Reg
  }

  // Angular Modal State
  showModal: boolean = false;
  showRefundModal: boolean = false;

  viewDetails(payment: Payment): void {
    this.selectedPayment = payment;
    this.showModal = true;
  }

  closeDetailsModal(): void {
    this.selectedPayment = null;
    this.showModal = false;
  }

  promptRefund(payment: Payment): void {
    this.selectedPayment = payment;
    this.refundTxnId = payment.transactionId;
    this.showRefundModal = true;
  }

  closeRefundModal(): void {
    this.showRefundModal = false;
  }

  confirmRefundAction(): void {
    if (!this.selectedPayment) return;

    this.selectedPayment.status = 'Refunded';
    this.calculateKPIs();

    this.toastService.success(`✅ Refund processed for Txn: ${this.selectedPayment.transactionId}`);
    this.closeRefundModal();
  }

  downloadReceiptPdf(): void {
    if (!this.selectedPayment) return;

    const printWindow = window.open('', '_blank');
    if (!printWindow) return;

    const formattedDate = new Date(this.selectedPayment.createdAt).toLocaleDateString('en-IN', {
      day: 'numeric',
      month: 'short',
      year: 'numeric',
    });

    const formattedValue = new Intl.NumberFormat('en-IN', {
      style: 'currency',
      currency: 'INR',
      maximumFractionDigits: 0
    }).format(this.selectedPayment.propertyValue || 0);

    const formattedAdvance = new Intl.NumberFormat('en-IN', {
      style: 'currency',
      currency: 'INR'
    }).format(this.selectedPayment.amount);

    const content = `
      <html>
        <head>
          <title>Receipt - ${this.selectedPayment.transactionId}</title>
          <style>
            body { font-family: 'Segoe UI', Arial, sans-serif; padding: 40px; color: #334155; }
            .header { text-align: center; border-bottom: 2px solid #1E3A8A; padding-bottom: 20px; margin-bottom: 30px; }
            .title { color: #1E3A8A; font-size: 24px; margin: 0; }
            .row { display: flex; justify-content: space-between; margin-bottom: 15px; font-size: 16px; border-bottom: 1px solid #F1F5F9; padding-bottom: 8px; }
            .label { color: #64748B; font-weight: 600; }
            .value { font-weight: 700; color: #1E293B; }
            .footer { margin-top: 40px; text-align: center; font-size: 12px; color: #94A3B8; }
          </style>
        </head>
        <body>
          <div class="header">
            <h2 class="title">ApexWorld Transaction Receipt</h2>
            <p>Official Confirmation of Advance Token Payment</p>
          </div>
          <div class="row"><span class="label">Transaction ID</span><span class="value">${this.selectedPayment.transactionId || 'N/A'}</span></div>
          <div class="row"><span class="label">Date Paid</span><span class="value">${formattedDate}</span></div>
          <div class="row"><span class="label">Booking Reference</span><span class="value">${this.selectedPayment.bookingRef || 'N/A'}</span></div>
          <div class="row"><span class="label">Buyer Name</span><span class="value">${this.selectedPayment.buyerName || 'N/A'}</span></div>
          <div class="row"><span class="label">Property Name</span><span class="value">${this.selectedPayment.propertyName || 'N/A'}</span></div>
          <div class="row"><span class="label">Property Value</span><span class="value">${formattedValue}</span></div>
          <div class="row"><span class="label">Payment Method</span><span class="value">${this.selectedPayment.paymentMethod}</span></div>
          <div class="row"><span class="label">Payment Type</span><span class="value">Token Advance</span></div>
          <div class="row" style="border-top: 2px solid #E2E8F0; padding-top: 15px; margin-top: 20px;">
            <span class="label" style="font-size: 18px; color: #1E293B;">Total Paid</span>
            <span class="value" style="font-size: 18px; color: #10B981;">${formattedAdvance}</span>
          </div>
          <div class="footer">
            <p>Thank you for trusting ApexWorld. This is a computer-generated receipt and requires no physical signature.</p>
          </div>
        </body>
      </html>
    `;

    printWindow.document.write(content);
    printWindow.document.close();
    printWindow.print();
  }
}
