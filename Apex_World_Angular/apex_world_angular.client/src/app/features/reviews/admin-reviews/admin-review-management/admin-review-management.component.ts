import { Component, OnInit, ViewEncapsulation, ChangeDetectorRef } from '@angular/core';
import { ReviewService } from '../../../../core/services/review.service';
import { EnquiryService, EnquiryDto } from '../../../../core/services/enquiry.service';
import { ReviewDto } from '../../../../core/models/review.model';
import { AdminHeader } from '../../../../shared/components/admin-header/admin-header';
import { NgIf, NgFor, DecimalPipe, DatePipe, SlicePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ToastService } from '../../../../core/services/toast.service';
import { PaginationComponent } from '../../../../shared/components/pagination/pagination';

@Component({
  selector: 'app-admin-review-management',
  templateUrl: './admin-review-management.component.html',
  styleUrls: ['./admin-review-management.component.css'],
  encapsulation: ViewEncapsulation.None,
  imports: [AdminHeader, NgIf, NgFor, DecimalPipe, DatePipe, FormsModule, SlicePipe, PaginationComponent],
})
export class AdminReviewManagementComponent implements OnInit {
  reviews: ReviewDto[] = [];
  enquiries: EnquiryDto[] = [];

  // Filtered lists
  filteredEnquiries: EnquiryDto[] = [];
  filteredPlatformReviews: ReviewDto[] = [];
  filteredPropertyReviews: ReviewDto[] = [];

  // Pagination State
  enquiryPage: number = 1;
  platformPage: number = 1;
  propertyPage: number = 1;
  pageSize: number = 7;

  // Active Tab
  activeTab: 'enquiries' | 'platform-reviews' | 'property-reviews' = 'enquiries';

  // Filters State
  searchText: string = '';
  selectedType: string = 'All';
  selectedStatus: string = 'All';

  // KPIs
  totalSubmissions: number = 0;
  unresolvedTickets: number = 0;
  averagePropertyRating: number = 0;

  // Modals state
  selectedItem: any = null;
  selectedItemType: 'enquiry' | 'review' | null = null;
  adminResponseText: string = '';

  isViewModalOpen: boolean = false;
  isRespondModalOpen: boolean = false;
  isDeleteModalOpen: boolean = false;
  itemToDelete: { id: number; type: 'enquiry' | 'review' } | null = null;

  constructor(
    private reviewService: ReviewService,
    private enquiryService: EnquiryService,
    public cdr: ChangeDetectorRef,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    // Load enquiries
    this.enquiryService.getAllEnquiries().subscribe({
      next: (res) => {
        if (res && res.data) {
          this.enquiries = res.data;
          this.applyFilters();
          this.calculateKPIs();
          this.cdr.detectChanges();
        }
      },
      error: (err) => console.error('Error loading enquiries', err)
    });

    // Load reviews
    this.reviewService.getAllReviews().subscribe({
      next: (res) => {
        if (res && res.data) {
          this.reviews = res.data;
          this.applyFilters();
          this.calculateKPIs();
          this.cdr.detectChanges();
        }
      },
      error: (err) => console.error('Error loading reviews', err)
    });
  }

  calculateKPIs(): void {
    this.totalSubmissions = this.enquiries.length + this.reviews.length;
    
    const pendingEnquiries = this.enquiries.filter(e => e.status !== 'Resolved').length;
    const pendingReviews = this.reviews.filter(r => r.status !== 'Resolved').length;
    this.unresolvedTickets = pendingEnquiries + pendingReviews;

    const propertyReviews = this.reviews.filter(r => r.reviewType === 'Property');
    if (propertyReviews.length > 0) {
      const sum = propertyReviews.reduce((acc, r) => acc + r.rating, 0);
      this.averagePropertyRating = sum / propertyReviews.length;
    } else {
      this.averagePropertyRating = 0;
    }
  }

  applyFilters(): void {
    const search = this.searchText.toLowerCase().trim();

    // 1. Filter Enquiries
    this.filteredEnquiries = this.enquiries.filter(e => {
      if (this.selectedType !== 'All' && this.selectedType !== 'Enquiry') return false;

      if (this.selectedStatus !== 'All') {
        if (this.selectedStatus === 'Pending' && e.status === 'Resolved') return false;
        if (this.selectedStatus === 'Resolved' && e.status !== 'Resolved') return false;
      }

      if (search) {
        const idMatch = `tk-e${e.id}`.includes(search) || e.id.toString().includes(search);
        const nameMatch = (e.buyerName || e.name || '').toLowerCase().includes(search);
        const subjectMatch = (e.subject || '').toLowerCase().includes(search);
        const messageMatch = (e.message || '').toLowerCase().includes(search);
        return idMatch || nameMatch || subjectMatch || messageMatch;
      }

      return true;
    });

    // 2. Filter Platform Reviews
    this.filteredPlatformReviews = this.reviews.filter(r => {
      if (r.reviewType !== 'Platform') return false;
      
      if (this.selectedType !== 'All' && this.selectedType !== 'Platform Review') return false;

      if (this.selectedStatus !== 'All') {
        if (this.selectedStatus === 'Pending' && r.status === 'Resolved') return false;
        if (this.selectedStatus === 'Resolved' && r.status !== 'Resolved') return false;
      }

      if (search) {
        const idMatch = `tk-p${r.id}`.includes(search) || r.id.toString().includes(search);
        const nameMatch = r.buyerName.toLowerCase().includes(search);
        const commentMatch = r.comment.toLowerCase().includes(search);
        return idMatch || nameMatch || commentMatch;
      }

      return true;
    });

    // 3. Filter Property Reviews
    this.filteredPropertyReviews = this.reviews.filter(r => {
      if (r.reviewType !== 'Property') return false;

      if (this.selectedType !== 'All' && this.selectedType !== 'Property Review') return false;

      if (this.selectedStatus !== 'All') {
        if (this.selectedStatus === 'Pending' && r.status === 'Resolved') return false;
        if (this.selectedStatus === 'Resolved' && r.status !== 'Resolved') return false;
      }

      if (search) {
        const idMatch = `tk-b${r.id}`.includes(search) || r.id.toString().includes(search);
        const nameMatch = r.buyerName.toLowerCase().includes(search);
        const propMatch = (r.propertyName || '').toLowerCase().includes(search);
        const commentMatch = r.comment.toLowerCase().includes(search);
        return idMatch || nameMatch || propMatch || commentMatch;
      }

      return true;
    });

    // Reset pagination to first page when filtering
    this.enquiryPage = 1;
    this.platformPage = 1;
    this.propertyPage = 1;
    this.cdr.detectChanges();
  }

  setTab(tab: 'enquiries' | 'platform-reviews' | 'property-reviews'): void {
    this.activeTab = tab;
    this.cdr.detectChanges();
  }

  getStars(rating: number): number[] {
    return Array(rating).fill(0);
  }

  getEmptyStars(rating: number): number[] {
    return Array(5 - rating).fill(0);
  }

  // --- PAGINATION HELPERS ---

  getEnquiryTotalPages(): number {
    return Math.ceil(this.filteredEnquiries.length / this.pageSize) || 1;
  }

  getPlatformTotalPages(): number {
    return Math.ceil(this.filteredPlatformReviews.length / this.pageSize) || 1;
  }

  getPropertyTotalPages(): number {
    return Math.ceil(this.filteredPropertyReviews.length / this.pageSize) || 1;
  }

  getEnquiryPages(): number[] {
    return Array(this.getEnquiryTotalPages()).fill(0).map((_, i) => i + 1);
  }

  getPlatformPages(): number[] {
    return Array(this.getPlatformTotalPages()).fill(0).map((_, i) => i + 1);
  }

  getPropertyPages(): number[] {
    return Array(this.getPropertyTotalPages()).fill(0).map((_, i) => i + 1);
  }

  // --- ACTIONS ---

  openViewModal(item: any, type: 'enquiry' | 'review'): void {
    this.selectedItem = item;
    this.selectedItemType = type;
    this.isViewModalOpen = true;
    this.cdr.detectChanges();
  }

  closeViewModal(): void {
    this.isViewModalOpen = false;
    this.selectedItem = null;
    this.selectedItemType = null;
    this.cdr.detectChanges();
  }

  openRespondModal(item: any, type: 'enquiry' | 'review'): void {
    this.selectedItem = item;
    this.selectedItemType = type;
    this.adminResponseText = item.adminResponse || '';
    this.isRespondModalOpen = true;
    this.cdr.detectChanges();
  }

  closeRespondModal(): void {
    this.isRespondModalOpen = false;
    this.selectedItem = null;
    this.selectedItemType = null;
    this.adminResponseText = '';
    this.cdr.detectChanges();
  }

  submitResponse(): void {
    if (!this.adminResponseText.trim()) {
      this.toastService.warning('Please enter a response.');
      return;
    }

    if (this.selectedItemType === 'enquiry') {
      this.enquiryService.resolveEnquiry(this.selectedItem.id, this.adminResponseText).subscribe({
        next: () => {
          this.toastService.success('✅ Enquiry resolved successfully.');
          this.loadData();
          this.closeRespondModal();
        },
        error: (err) => {
          this.toastService.error('Failed to resolve enquiry.');
          console.error(err);
        }
      });
    } else if (this.selectedItemType === 'review') {
      this.reviewService.respondToReview(this.selectedItem.id, this.adminResponseText).subscribe({
        next: () => {
          this.toastService.success('✅ Review response submitted successfully.');
          this.loadData();
          this.closeRespondModal();
        },
        error: (err) => {
          this.toastService.error('Failed to respond to review.');
          console.error(err);
        }
      });
    }
  }

  openDeleteModal(id: number, type: 'enquiry' | 'review'): void {
    this.itemToDelete = { id, type };
    this.isDeleteModalOpen = true;
    this.cdr.detectChanges();
  }

  closeDeleteModal(): void {
    this.isDeleteModalOpen = false;
    this.itemToDelete = null;
    this.cdr.detectChanges();
  }

  confirmDelete(): void {
    if (!this.itemToDelete) return;

    const deletingType = this.itemToDelete.type;
    const deletingId = this.itemToDelete.id;

    // Close the modal immediately
    this.closeDeleteModal();

    if (deletingType === 'enquiry') {
      this.enquiryService.deleteEnquiry(deletingId).subscribe({
        next: () => {
          this.toastService.success('✅ Enquiry deleted successfully.');
          this.loadData();
        },
        error: (err) => {
          this.toastService.error('Failed to delete enquiry.');
          console.error(err);
        }
      });
    } else if (deletingType === 'review') {
      this.reviewService.deleteAdminReview(deletingId).subscribe({
        next: () => {
          this.toastService.success('✅ Review deleted successfully.');
          this.loadData();
        },
        error: (err) => {
          this.toastService.error('Failed to delete review.');
          console.error(err);
        }
      });
    }
  }
}
