import { Component, OnInit, ViewEncapsulation, ChangeDetectorRef } from '@angular/core';
import { BookingService } from '../../../../core/services/booking.service';
import { BookingDto } from '../../../../core/models/booking.model';
import { AdminHeader } from '../../../../shared/components/admin-header/admin-header';
import { NgIf, NgFor, NgClass, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PaginationComponent } from '../../../../shared/components/pagination/pagination';
import { ToastService } from '../../../../core/services/toast.service';

@Component({
  selector: 'app-admin-site-visit-management',
  templateUrl: './admin-site-visit-management.html',
  styleUrls: ['./admin-site-visit-management.css'],
  encapsulation: ViewEncapsulation.None,
  imports: [AdminHeader, NgIf, NgFor, NgClass, DatePipe, FormsModule, PaginationComponent],
})
export class AdminSiteVisitManagement implements OnInit {
  // ── Data & Pagination ─────────────────────────────────────────────────────
  siteVisits: BookingDto[] = [];
  allVisitsForKPI: BookingDto[] = [];

  totalItems: number = 0;
  pageNumber: number = 1;
  pageSize: number = 7;
  get totalPages(): number {
    return Math.ceil(this.totalItems / this.pageSize) || 1;
  }

  // ── KPI Counters ──────────────────────────────────────────────────
  totalRequests: number = 0;
  approvedVisits: number = 0;
  pendingVisits: number = 0;
  deniedVisits: number = 0;

  // ── Checkbox Selection State ──────────────────────────────────────
  selectedBookingIds = new Set<number>();

  // ── Modal State ───────────────────────────────────────────────────
  actionBookingId: number | null = null;
  actionType: 'standard' | 'reschedule' | 'cancel' | null = null;

  constructor(
    private bookingService: BookingService,
    private cdr: ChangeDetectorRef,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.loadBookings();
  }

  // ── Load Data ─────────────────────────────────────────────────────
  loadBookings(): void {
    this.bookingService.getAdminBookings(this.pageNumber, this.pageSize).subscribe({
      next: (res) => {
        if (res.data) {
          (window as any).debugRes = res;

          this.siteVisits = res.data.items.map((b: any) => ({
            ...b,
            propertyName: b.property?.title || b.propertyName || 'Unknown Property',
            buyerName: b.firstName ? `${b.firstName} ${b.lastName}` : `Buyer #${b.buyerId}`,
            bookingDate: b.createdAt || b.bookingDate,
          }));
          this.totalItems = res.data.totalItems;
          this.allVisitsForKPI = this.siteVisits;
          this.selectedBookingIds.clear();
          this.updateKPIs();
          this.cdr.markForCheck();
        }
      },
      error: (err) => {
        console.error('Failed to load admin bookings', err);
        this.toastService.error('Failed to load bookings');
        this.cdr.markForCheck();
      },
    });
  }

  changePage(newPage: number): void {
    const totalPages = Math.ceil(this.totalItems / this.pageSize);
    if (newPage < 1 || (totalPages > 0 && newPage > totalPages)) return;
    this.pageNumber = newPage;
    this.loadBookings();
  }

  onPageChange(page: number): void {
    this.pageNumber = page;
    this.loadBookings();
  }

  updateKPIs(): void {
    this.totalRequests = this.totalItems;
    this.approvedVisits = this.allVisitsForKPI.filter((v) => v.status === 'Approved' || v.status === 'PendingPayment').length;
    this.pendingVisits = this.allVisitsForKPI.filter(
      (v) => v.status === 'PendingAdminApproval' || v.status === 'Pending' || v.status === 'RescheduleRequested' || v.status === 'CancellationRequested'
    ).length;
    this.deniedVisits = this.allVisitsForKPI.filter(
      (v) => v.status === 'Denied' || v.status === 'Rejected' || v.status === 'Cancelled'
    ).length;
  }

  // ── Booking Selection ─────────────────────────────────────────────
  toggleSelection(id: number): void {
    if (this.selectedBookingIds.has(id)) {
      this.selectedBookingIds.delete(id);
    } else {
      this.selectedBookingIds.add(id);
    }
    this.cdr.markForCheck();
  }

  toggleAll(event: any): void {
    if (event.target.checked) {
      this.siteVisits.forEach((item) => this.selectedBookingIds.add(item.id));
    } else {
      this.selectedBookingIds.clear();
    }
    this.cdr.markForCheck();
  }

  isAllSelected(): boolean {
    if (this.siteVisits.length === 0) return false;
    return this.siteVisits.every((item) => this.selectedBookingIds.has(item.id));
  }

  // ── Top Level Multi-Select Actions ───────────────────────────────
  approveSelected(): void {
    if (this.selectedBookingIds.size === 0) return;
    
    // Asynchronously approve all selected standard bookings
    const promises = Array.from(this.selectedBookingIds).map((id) => {
      const booking = this.siteVisits.find((b) => b.id === id);
      if (!booking) return Promise.resolve();

      let obs$;
      if (booking.status === 'CancellationRequested') {
        obs$ = this.bookingService.approveCancellation(id);
      } else if (booking.status === 'RescheduleRequested') {
        obs$ = this.bookingService.approveReschedule(id);
      } else {
        obs$ = this.bookingService.approveBooking(id);
      }
      return obs$.toPromise();
    });

    Promise.all(promises).then(() => {
      this.toastService.success(`✅ Selected requests approved successfully.`);
      this.loadBookings();
    }).catch((err) => {
      console.error('Failed to approve some bookings', err);
      this.toastService.error('Error approving some requests.');
      this.loadBookings();
    });
  }

  denySelected(): void {
    if (this.selectedBookingIds.size === 0) return;
    // Open deny dialog for the first selected item, or trigger bulk deny
    // For simplicity, we trigger rejection on all selected with a generic bulk reason
    const genericReason = 'Denied by administrator via bulk selection.';
    const promises = Array.from(this.selectedBookingIds).map((id) => {
      const booking = this.siteVisits.find((b) => b.id === id);
      if (!booking) return Promise.resolve();

      let obs$;
      if (booking.status === 'CancellationRequested') {
        obs$ = this.bookingService.rejectCancellation(id, genericReason);
      } else if (booking.status === 'RescheduleRequested') {
        obs$ = this.bookingService.rejectReschedule(id, genericReason);
      } else {
        obs$ = this.bookingService.rejectBooking(id, genericReason);
      }
      return obs$.toPromise();
    });

    Promise.all(promises).then(() => {
      this.toastService.success(`✅ Selected requests rejected successfully.`);
      this.loadBookings();
    }).catch((err) => {
      console.error('Failed to reject some bookings', err);
      this.toastService.error('Error rejecting some requests.');
      this.loadBookings();
    });
  }

  // ── Helper to format Booking ID to 13-digit AWB# format ─────────
  formatBookingId(id: number): string {
    return 'AWB#' + (1780000000000 + id).toString();
  }

  // ── API: Approval Workflows ───────────────────────────────────────
  approveBooking(id: number, type: 'standard' | 'reschedule' | 'cancel'): void {
    const booking = this.siteVisits.find((b) => b.id === id);
    if (!booking) return;

    let obs$;
    if (type === 'cancel') {
      obs$ = this.bookingService.approveCancellation(id);
    } else if (type === 'reschedule') {
      obs$ = this.bookingService.approveReschedule(id);
    } else {
      obs$ = this.bookingService.approveBooking(id);
    }

    obs$.subscribe({
      next: () => {
        this.toastService.success(`✅ Request approved successfully.`);
        this.loadBookings();
      },
      error: (err) => {
        console.error('Failed to approve booking', err);
        this.toastService.error('Failed to approve request.');
        this.cdr.markForCheck();
      },
    });
  }

  // ── Modal State ───────────────────────────────────────────────────
  showDenyModal: boolean = false;
  denyReason: string = '';

  // ── Modals: Denial Workflow ───────────────────────────────────────
  promptDeny(id: number, type: 'standard' | 'reschedule' | 'cancel'): void {
    this.actionBookingId = id;
    this.actionType = type;
    this.denyReason = '';
    this.showDenyModal = true;
    this.cdr.markForCheck();
  }

  closeDenyModal(): void {
    this.actionBookingId = null;
    this.actionType = null;
    this.denyReason = '';
    this.showDenyModal = false;
    this.cdr.markForCheck();
  }

  confirmDeny(): void {
    if (!this.actionBookingId || !this.actionType) return;

    let obs$;
    if (this.actionType === 'cancel') {
      obs$ = this.bookingService.rejectCancellation(this.actionBookingId, this.denyReason);
    } else if (this.actionType === 'reschedule') {
      obs$ = this.bookingService.rejectReschedule(this.actionBookingId, this.denyReason);
    } else {
      obs$ = this.bookingService.rejectBooking(this.actionBookingId, this.denyReason);
    }

    obs$.subscribe({
      next: () => {
        this.toastService.success(`✅ Request rejected successfully.`);
        this.loadBookings();
        this.closeDenyModal();
      },
      error: (err) => {
        console.error('Failed to reject booking', err);
        this.toastService.error('Failed to reject request.');
        this.closeDenyModal();
        this.cdr.markForCheck();
      },
    });
  }
}
