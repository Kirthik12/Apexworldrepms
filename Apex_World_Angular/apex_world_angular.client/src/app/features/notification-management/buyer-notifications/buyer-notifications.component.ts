import { Component, OnInit, OnDestroy, ViewEncapsulation } from '@angular/core';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { switchMap, takeUntil } from 'rxjs/operators';
import { NotificationService } from '../../../core/services/notification.service';
import { BuyerNotificationDto } from '../../../core/models/notification.model';
import { NgFor, NgIf, DatePipe } from '@angular/common';

@Component({
  selector: 'app-buyer-notifications',
  templateUrl: './buyer-notifications.component.html',
  styleUrls: ['./buyer-notifications.component.css'],
  encapsulation: ViewEncapsulation.None,
  imports: [NgFor, NgIf, DatePipe],
})
export class BuyerNotificationsComponent implements OnInit, OnDestroy {
  Math = Math;
  notifications: BuyerNotificationDto[] = [];
  totalItems = 0;
  pageNumber = 1;
  pageSize = 10;

  categories = ['All', 'Booking', 'Payment', 'General'];
  activeCategory = 'All';

  private filterChange$ = new Subject<{ category?: string; page: number }>();
  private destroy$ = new Subject<void>();

  constructor(
    private notificationService: NotificationService,
    private router: Router,
  ) {}

  ngOnInit() {
    this.filterChange$
      .pipe(
        switchMap(({ category, page }) =>
          // The real API doesn't support category filtering natively on this endpoint in swagger,
          // but we'll fetch all and filter client side if the backend doesn't support it,
          // or just fetch normally. For now we will fetch normally and let backend handle it if it can.
          this.notificationService.getBuyerNotifications(page, this.pageSize),
        ),
        takeUntil(this.destroy$),
      )
      .subscribe((res) => {
        if (res && res.data) {
          let items = res.data.items;
          if (this.activeCategory !== 'All') {
            items = items.filter((n: BuyerNotificationDto) => n.category === this.activeCategory);
          }
          this.notifications = items;
          this.totalItems = res.data.totalItems;
        }
      });

    this.emitFilter();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private emitFilter() {
    const category = this.activeCategory === 'All' ? undefined : this.activeCategory;
    this.filterChange$.next({ category, page: this.pageNumber });
  }

  setCategory(cat: string) {
    this.activeCategory = cat;
    this.pageNumber = 1;
    this.emitFilter();
  }

  nextPage() {
    if (this.pageNumber * this.pageSize < this.totalItems) {
      this.pageNumber++;
      this.emitFilter();
    }
  }

  prevPage() {
    if (this.pageNumber > 1) {
      this.pageNumber--;
      this.emitFilter();
    }
  }

  onNotificationClick(notification: BuyerNotificationDto) {
    if (!notification.isRead) {
      this.notificationService.markBuyerNotificationAsRead(notification.id).subscribe(() => {
        notification.isRead = true;
      });
    }
    if (notification.actionUrl) {
      this.router.navigateByUrl(notification.actionUrl);
    }
  }

  getCategoryClass(category: string): string {
    return 'n-card-icon ' + category.toLowerCase();
  }

  markAllRead() {
    this.notificationService.markAllBuyerNotificationsAsRead().subscribe(() => {
      this.notifications.forEach((n) => (n.isRead = true));
    });
  }
}
