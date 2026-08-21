import { Component, OnInit, ViewEncapsulation, ChangeDetectorRef } from '@angular/core';
import { AdminHeader } from '../../../../shared/components/admin-header/admin-header';
import { ToastService } from '../../../../core/services/toast.service';
import { FormsModule } from '@angular/forms';
import { NgIf, NgFor, NgStyle, DecimalPipe, DatePipe } from '@angular/common';
import { LoanService, LoanApplicationDto } from '../../../../core/services/loan.service';
import { PaginationComponent } from '../../../../shared/components/pagination/pagination';

export interface Loan {
  id: number;
  buyerId: number;
  buyerName: string;
  bookingId: number;
  propertyId: number;
  propertyName: string;
  propertyValue: number;
  propertyAddress: string;
  loanAmount: number;
  bankName: string;
  interestRate: number;
  tenure: number;
  monthlyEmi: number;
  status: string; // "Pending", "Approved", "Rejected"
  createdAt: string;
  email: string;
  phone: string;
  address: string;
  employmentType: string;
  monthlyIncome: number;
}

@Component({
  selector: 'app-admin-loan-management',
  templateUrl: './admin-loan-management.component.html',
  styleUrls: ['./admin-loan-management.component.css'],
  standalone: true,
  encapsulation: ViewEncapsulation.None,
  imports: [AdminHeader, FormsModule, NgIf, NgFor, DecimalPipe, DatePipe, PaginationComponent],
})
export class AdminLoanManagementComponent implements OnInit {
  loans: Loan[] = [];
  allLoans: Loan[] = [];

  // Pagination State
  pageNumber: number = 1;
  pageSize: number = 7;
  totalItems: number = 0;
  get totalPages(): number {
    return Math.ceil(this.totalItems / this.pageSize) || 1;
  }

  // KPIs
  totalApps: number = 0;
  pendingApps: number = 0;
  totalFunded: number = 0;

  // Modal State
  actionLoanId: number | null = null;
  actionType: 'Approved' | 'Rejected' | null = null;
  selectedLoan: Loan | null = null;

  constructor(
    private loanService: LoanService,
    private cdr: ChangeDetectorRef,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.loadLoans();
  }

  loadLoans(): void {
    this.loanService.getAllLoans().subscribe({
      next: (res: any) => {
        if (res?.data) {
          this.allLoans = res.data.map((l: any) => ({
            id: l.id,
            buyerId: l.buyerId,
            buyerName: l.buyerName || (l.booking ? `${l.booking.firstName} ${l.booking.lastName}` : 'Unknown Buyer'),
            bookingId: l.bookingId,
            propertyId: l.propertyId,
            propertyName: l.property?.title || l.booking?.property?.title || 'Unknown Property',
            propertyValue: l.property?.price || l.booking?.property?.price || 0,
            propertyAddress: l.property?.location || l.booking?.property?.location || 'N/A',
            loanAmount: l.loanAmount,
            bankName: l.bankName || 'SBI Home Loans',
            interestRate: 8.5, // Assumed rate
            tenure: l.tenureYears || 20,
            monthlyEmi: l.monthlyEMI || 0,
            status: l.status,
            createdAt: l.createdAt,
            email: l.booking?.email || 'N/A',
            phone: l.booking?.phoneNumber || 'N/A',
            address: l.booking?.address || 'N/A',
            employmentType: l.employmentType || 'Salaried',
            monthlyIncome: l.monthlyIncome || 0,
          }));
          this.calculateKPIs();
          this.applyPagination();
        }
      },
      error: (err: any) => {
        console.error('Failed to load loans:', err);
      }
    });
  }

  applyPagination(): void {
    this.totalItems = this.allLoans.length;
    const startIndex = (this.pageNumber - 1) * this.pageSize;
    this.loans = this.allLoans.slice(startIndex, startIndex + this.pageSize);
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
    this.totalApps = this.allLoans.length;
    this.pendingApps = this.allLoans.filter((l) => l.status === 'Pending').length;

    this.totalFunded = this.allLoans
      .filter((l) => l.status === 'Approved')
      .reduce((sum, current) => sum + current.loanAmount, 0);
  }

  updateStatus(id: number, status: 'Approved' | 'Rejected'): void {
    this.loanService.updateLoanStatus(id, status).subscribe({
      next: () => {
        this.toastService.success(`✅ Loan application ${status.toLowerCase()} successfully.`);
        this.loadLoans();
      },
      error: (err: any) => {
        console.error(`Failed to update status for Loan #${id}:`, err);
        const errMsg = err?.error?.message || `Failed to update status for Loan #${id}.`;
        this.toastService.error(`❌ ${errMsg}`);
      }
    });
  }

  // Angular-bound modal state
  showDetailsModal: boolean = false;
  showConfirmModal: boolean = false;
  pendingAction: 'Approved' | 'Rejected' | null = null;
  isProcessing: boolean = false;

  promptStatusUpdate(id: number, status: 'Approved' | 'Rejected'): void {
    this.actionLoanId = id;
    this.actionType = status;
    this.pendingAction = status;
    this.showConfirmModal = true;
  }

  executeStatusUpdate(): void {
    if (this.actionLoanId !== null && this.actionType !== null) {
      this.isProcessing = true;
      this.updateStatus(this.actionLoanId, this.actionType);
    }
    this.closeConfirmModal();
  }

  closeConfirmModal(): void {
    this.showConfirmModal = false;
    this.actionLoanId = null;
    this.actionType = null;
    this.pendingAction = null;
    this.isProcessing = false;
  }

  viewDetails(loan: Loan): void {
    this.selectedLoan = loan;
    this.showDetailsModal = true;
  }

  closeDetailsModal(): void {
    this.selectedLoan = null;
    this.showDetailsModal = false;
  }

}
