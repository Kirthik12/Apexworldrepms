import {
  Component,
  ViewEncapsulation,
  OnInit,
  ChangeDetectorRef,
  ChangeDetectionStrategy,
} from '@angular/core';
import { UserService } from '../../../../core/services/user.service';
import { User } from '../../../../core/models/user.model';
import { NgIf, NgFor } from '@angular/common';
import { PaginationComponent } from '../../../../shared/components/pagination/pagination';
import { AdminHeader } from '../../../../shared/components/admin-header/admin-header';

@Component({
  selector: 'app-admin-customer-management',
  templateUrl: './admin-customer-management.html',
  styleUrl: './admin-customer-management.css',
  standalone: true,
  encapsulation: ViewEncapsulation.None,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [AdminHeader, NgIf, NgFor, PaginationComponent],
})
export class AdminCustomerManagementComponent implements OnInit {
  allCustomers: User[] = []; // Full unfiltered list from API
  customers: User[] = []; // Currently displayed (filtered) list
  pageNumber: number = 1;
  pageSize: number = 10;
  totalCount: number = 0;

  // 'all' | 'active' | 'inactive'
  activeFilter: 'all' | 'active' | 'inactive' = 'all';

  // Confirmation modal state
  showConfirmModal: boolean = false;
  pendingToggleUser: User | null = null;

  get totalCustomers(): number {
    return this.allCustomers.length;
  }
  get activeCustomers(): number {
    return this.allCustomers.filter((c) => c.isActive).length;
  }
  get inactiveCustomers(): number {
    return this.allCustomers.filter((c) => !c.isActive).length;
  }

  constructor(
    private userService: UserService,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.loadBuyers();
  }

  loadBuyers(): void {
    this.userService.getBuyers(this.pageNumber, this.pageSize).subscribe({
      next: (res) => {
        this.allCustomers = res.items;
        this.totalCount = res.totalCount;
        this.applyFilter();
      },
      error: (err) => {
        console.error('Failed to load buyers', err);
        this.cdr.detectChanges();
      },
    });
  }

  /** Apply the current activeFilter to the full list */
  applyFilter(): void {
    if (this.activeFilter === 'active') {
      this.customers = this.allCustomers.filter((c) => c.isActive);
    } else if (this.activeFilter === 'inactive') {
      this.customers = this.allCustomers.filter((c) => !c.isActive);
    } else {
      this.customers = [...this.allCustomers];
    }
    this.cdr.detectChanges();
  }

  /** Called when a KPI card is clicked */
  setFilter(filter: 'all' | 'active' | 'inactive'): void {
    // Toggle off if clicking the same filter
    if (this.activeFilter === filter) {
      this.activeFilter = 'all';
    } else {
      this.activeFilter = filter;
    }
    this.applyFilter();
  }

  /**
   * Called when the toggle checkbox is clicked.
   * If currently ACTIVE (toggle ON → turning OFF = deactivate): show confirmation modal.
   * If currently INACTIVE (toggle OFF → turning ON = activate): proceed immediately.
   */
  onToggleClick(event: Event, user: User): void {
    event.preventDefault(); // Stop native checkbox flip — we control the flip ourselves

    if (user.isActive) {
      // User is currently ACTIVE → admin wants to DEACTIVATE → show modal
      this.pendingToggleUser = user;
      this.showConfirmModal = true;
      this.cdr.detectChanges();
    } else {
      // User is currently INACTIVE → admin wants to ACTIVATE → do it immediately
      this.executeToggle(user);
    }
  }

  confirmDeactivation(): void {
    if (this.pendingToggleUser) {
      this.executeToggle(this.pendingToggleUser);
    }
    this.closeModal();
  }

  cancelDeactivation(): void {
    this.closeModal();
  }

  private closeModal(): void {
    this.showConfirmModal = false;
    this.pendingToggleUser = null;
    this.cdr.detectChanges();
  }

  private executeToggle(user: User): void {
    const previousState = user.isActive;
    this.userService.toggleUserStatus(user.id).subscribe({
      next: (res) => {
        if (res && typeof res.isActive === 'boolean') {
          user.isActive = res.isActive;
        } else {
          user.isActive = !previousState;
        }
        // Re-apply filter so the row moves in/out of filtered view
        this.applyFilter();
      },
      error: (err) => {
        console.error('Failed to toggle user status', err);
        user.isActive = previousState;
        this.cdr.detectChanges();
      },
    });
  }

  nextPage(): void {
    if (this.pageNumber * this.pageSize < this.totalCount) {
      this.pageNumber++;
      this.loadBuyers();
    }
  }

  prevPage(): void {
    if (this.pageNumber > 1) {
      this.pageNumber--;
      this.loadBuyers();
    }
  }

  onPageChange(page: number): void {
    this.pageNumber = page;
    this.loadBuyers();
  }
}
