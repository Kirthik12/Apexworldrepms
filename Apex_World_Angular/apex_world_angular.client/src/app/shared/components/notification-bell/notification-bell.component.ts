import { Component, OnInit, OnDestroy, HostListener, ElementRef, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { NotificationService } from '../../../core/services/notification.service';
import {
  BuyerNotificationDto,
  AdminNotificationDto,
} from '../../../core/models/notification.model';
import { NgIf, NgFor, NgClass, DatePipe } from '@angular/common';

@Component({
  selector: 'app-notification-bell',
  templateUrl: './notification-bell.component.html',
  styleUrls: ['./notification-bell.component.css'],
  imports: [NgIf, NgFor, NgClass, DatePipe],
})
export class NotificationBellComponent implements OnInit, OnDestroy {
  unreadCount = 0;
  notifications: any[] = [];
  isDropdownOpen = false;
  private subs = new Subscription();

  constructor(
    private notificationService: NotificationService,
    private router: Router,
    private eRef: ElementRef,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit() {
    this.subs.add(
      this.notificationService.unreadCount$.subscribe((count) => {
        this.unreadCount = count;
        this.cdr.detectChanges();
      }),
    );
    // Fetch initial count
    this.fetchInitialCount();
  }

  fetchInitialCount() {
    if (this.router.url.includes('/admin-dashboard')) {
      this.notificationService.getAdminNotifications(1, 1).subscribe();
    } else {
      this.notificationService.getBuyerNotifications(1, 1).subscribe();
    }
  }

  ngOnDestroy() {
    this.subs.unsubscribe();
  }

  toggleDropdown(event: Event) {
    event.stopPropagation();
    this.isDropdownOpen = !this.isDropdownOpen;
    if (this.isDropdownOpen) {
      this.fetchRecentNotifications();
    }
  }

  @HostListener('document:click', ['$event'])
  clickout(event: Event) {
    if (!this.eRef.nativeElement.contains(event.target)) {
      this.isDropdownOpen = false;
    }
  }

  fetchRecentNotifications() {
    if (this.router.url.includes('/admin-dashboard')) {
      this.notificationService.getAdminNotifications(1, 5).subscribe((res) => {
        if (res && res.data) {
          this.notifications = res.data.items;
          this.cdr.detectChanges();
        }
      });
    } else {
      this.notificationService.getBuyerNotifications(1, 5).subscribe((res) => {
        if (res && res.data) {
          this.notifications = res.data.items;
          this.cdr.detectChanges();
        }
      });
    }
  }

  onNotificationClick(notification: any) {
    this.isDropdownOpen = false;

    if (!notification.isRead) {
      if (this.router.url.includes('/admin-dashboard')) {
        this.notificationService.markAdminNotificationAsRead(notification.id).subscribe(() => {
          this.cdr.detectChanges();
        });
      } else {
        this.notificationService.markBuyerNotificationAsRead(notification.id).subscribe(() => {
          this.cdr.detectChanges();
        });
      }
      notification.isRead = true; // Optimistic update
      this.cdr.detectChanges();
    }

    if (notification.actionUrl) {
      this.router.navigateByUrl(notification.actionUrl);
    }
  }

  goToAllNotifications() {
    this.isDropdownOpen = false;
    if (this.router.url.includes('/admin-dashboard')) {
      this.router.navigate(['/admin-dashboard/notifications']);
    } else {
      this.router.navigate(['/buyer-dashboard/notifications']);
    }
  }

  markAllAsRead(event: Event) {
    event.stopPropagation();
    if (this.router.url.includes('/admin-dashboard')) {
      this.notificationService.markAllAdminNotificationsAsRead().subscribe(() => {
        this.notifications.forEach((n) => (n.isRead = true));
        this.cdr.detectChanges();
      });
    } else {
      this.notificationService.markAllBuyerNotificationsAsRead().subscribe(() => {
        this.notifications.forEach((n) => (n.isRead = true));
        this.cdr.detectChanges();
      });
    }
  }
}
