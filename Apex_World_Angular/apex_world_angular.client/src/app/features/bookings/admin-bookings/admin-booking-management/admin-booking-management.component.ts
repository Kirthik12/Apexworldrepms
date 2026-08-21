import { Component, OnInit, ViewEncapsulation, ChangeDetectorRef } from '@angular/core';
import { AdminHeader } from '../../../../shared/components/admin-header/admin-header';
import { FormsModule } from '@angular/forms';
import { NgIf, NgFor, DecimalPipe, DatePipe } from '@angular/common';
import { BookingService } from '../../../../core/services/booking.service';
import { PaginationComponent } from '../../../../shared/components/pagination/pagination';

@Component({
  selector: 'app-admin-booking-management',
  templateUrl: './admin-booking-management.component.html',
  styleUrls: ['./admin-booking-management.component.css'],
  standalone: true,
  encapsulation: ViewEncapsulation.None,
  imports: [AdminHeader, FormsModule, NgIf, NgFor, DecimalPipe, DatePipe, PaginationComponent],
})
export class AdminBookingManagementComponent implements OnInit {
  bookings: any[] = [];
  allBookings: any[] = [];

  // Pagination State
  pageNumber: number = 1;
  pageSize: number = 7;
  totalItems: number = 0;
  get totalPages(): number {
    return Math.ceil(this.totalItems / this.pageSize) || 1;
  }

  // KPIs
  totalReservations: number = 0;
  pendingRequests: number = 0;
  totalRealizedValuation: string = 'INR 0';

  // Modal State
  showModal: boolean = false;
  selectedBooking: any = null;

  constructor(
    private bookingService: BookingService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadBookings();
  }

  loadBookings(): void {
    this.bookingService.getAdminBookings(1, 1000, true).subscribe({
      next: (res: any) => {
        if (res?.data?.items) {
          this.allBookings = res.data.items;
          this.calculateKPIs();
          this.applyPagination();
        }
      },
      error: (err: any) => {
        console.error('Failed to load admin bookings:', err);
      },
    });
  }

  applyPagination(): void {
    this.totalItems = this.allBookings.length;
    const startIndex = (this.pageNumber - 1) * this.pageSize;
    this.bookings = this.allBookings.slice(startIndex, startIndex + this.pageSize);
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
    this.totalReservations = this.allBookings.length;
    this.pendingRequests = this.allBookings.filter((b) => b.status === 'Paid' || b.status === 'CancellationRequested').length;

    // Filter for paid or approved bookings to calculate realized valuation
    const completedOrApproved = this.allBookings.filter(
      (b) => b.status === 'Paid' || b.status === 'Approved'
    );
    const sumValuation = completedOrApproved.reduce((sum, b) => sum + (b.property?.price || 0), 0);

    if (sumValuation >= 10000000) {
      this.totalRealizedValuation = `INR ${(sumValuation / 10000000).toFixed(2)} Cr`;
    } else if (sumValuation >= 100000) {
      this.totalRealizedValuation = `INR ${(sumValuation / 100000).toFixed(2)} Lac`;
    } else {
      this.totalRealizedValuation = `INR ${sumValuation.toLocaleString('en-IN')}`;
    }
  }

  viewReceipt(booking: any): void {
    this.selectedBooking = booking;
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.selectedBooking = null;
  }
  downloadReceiptPdf(): void {
    const printWindow = window.open('', '_blank');
    if (!printWindow) return;

    const prop = this.selectedBooking?.property || {};
    const buyerName = `${this.selectedBooking?.firstName || ''} ${this.selectedBooking?.lastName || ''}`;

    const content = `
      <html>
        <head>
          <title>Receipt - Booking #${this.selectedBooking?.id}</title>
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
          <div class="row"><span class="label">Booking ID</span><span class="value">#${this.selectedBooking?.id}</span></div>
          <div class="row"><span class="label">Reference Code</span><span class="value">AWB#${this.selectedBooking?.id}00</span></div>
          <div class="row"><span class="label">Property Target Name</span><span class="value">${prop.title || 'N/A'}</span></div>
          <div class="row"><span class="label">Property Address</span><span class="value">${prop.address || 'N/A'}</span></div>
          <div class="row"><span class="label">Buyer Name</span><span class="value">${buyerName}</span></div>
          <div class="row"><span class="label">Payment Reference</span><span class="value">${this.selectedBooking?.paymentReference || 'N/A'}</span></div>
          <div class="row"><span class="label">Payment Method</span><span class="value">${this.selectedBooking?.paymentMethod || 'N/A'}</span></div>
          <div class="row" style="border-top: 2px solid #E2E8F0; padding-top: 15px; margin-top: 20px;">
            <span class="label" style="font-size: 18px; color: #1E293B;">Advance Token Paid</span>
            <span class="value" style="font-size: 18px; color: #10B981;">INR 10,000.00</span>
          </div>
          <div class="footer">
            <p>Thank you for choosing ApexWorld. This is a computer-generated receipt and requires no physical signature.</p>
          </div>
        </body>
      </html>
    `;

    printWindow.document.write(content);
    printWindow.document.close();
    printWindow.print();
  }
}
