import { Component, OnInit } from '@angular/core';
import { NotificationService } from '../../../../core/services/notification.service';
import {
  AdminNotificationDto,
  BroadcastNotificationDto,
} from '../../../../core/models/notification.model';
import { AdminHeader } from '../../../../shared/components/admin-header/admin-header';
import { FormsModule } from '@angular/forms';
import { NgIf, NgFor, NgClass } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-admin-notification',
  templateUrl: './admin-notification.html',
  styleUrls: ['./admin-notification.css'],
  imports: [AdminHeader, FormsModule, NgIf, NgFor, NgClass, RouterLink],
})
export class AdminNotification implements OnInit {
  notifications: AdminNotificationDto[] = [];
  paginatedNotifications: AdminNotificationDto[] = [];
  dbReady: boolean = true;

  // Filter & Pagination State
  selectedCategory: string = 'All';
  unreadOnly: boolean = false;
  pageNumber: number = 1;
  pageSize: number = 5;
  totalItems: number = 0;
  unreadCountValue: number = 0;

  // Broadcast Form State
  broadcastData: BroadcastNotificationDto = {
    title: '',
    message: '',
    category: 'Announcement',
    targetAudience: 'AllUsers',
    targetRole: '',
    targetUserId: null,
  };

  constructor(private notificationService: NotificationService) {}

  ngOnInit(): void {
    this.renderNotifications();
    this.notificationService.unreadCount$.subscribe((count) => {
      this.unreadCountValue = count;
    });
  }

  getIconClassAndEmoji(type: string): { cls: string; emoji: string } {
    switch (type.toLowerCase()) {
      case 'booking':
      case 'bookings':
        return { cls: 'icon-booking', emoji: '📅' };
      case 'payment':
      case 'payments':
        return { cls: 'icon-payment', emoji: '💳' };
      case 'system':
        return { cls: 'icon-system', emoji: '⚙️' };
      case 'announcement':
        return { cls: 'icon-default', emoji: '📢' };
      default:
        return { cls: 'icon-default', emoji: '🔔' };
    }
  }

  formatTime(isoString: string): string {
    const date = new Date(isoString);
    return date.toLocaleString('en-US', {
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  }

  renderNotifications(): void {
    // The backend does not support filtering by category/unread in the query string yet.
    // So we fetch paginated, but since filtering happens on frontend, we might need all data.
    // For now we'll fetch page size and then filter. Ideally backend supports filtering.
    this.notificationService
      .getAdminNotifications(this.pageNumber, this.pageSize)
      .subscribe((res) => {
        if (res && res.data) {
          let items = res.data.items;

          if (this.selectedCategory !== 'All') {
            items = items.filter(
              (n) => n.category.toLowerCase() === this.selectedCategory.toLowerCase(),
            );
          }
          if (this.unreadOnly) {
            items = items.filter((n) => !n.isRead);
          }

          this.paginatedNotifications = items;
          this.totalItems = res.data.totalItems;
        }
      });
  }

  onFilterChange(): void {
    this.pageNumber = 1;
    this.renderNotifications();
  }

  nextPage(): void {
    if (this.pageNumber * this.pageSize < this.totalItems) {
      this.pageNumber++;
      this.renderNotifications();
    }
  }

  prevPage(): void {
    if (this.pageNumber > 1) {
      this.pageNumber--;
      this.renderNotifications();
    }
  }

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.totalItems / this.pageSize));
  }

  markAsRead(id: number): void {
    this.notificationService.markAdminNotificationAsRead(id).subscribe(() => {
      this.renderNotifications();
    });
  }

  deleteNotification(id: number): void {
    // API doesn't support delete yet in swagger, just mark as read for now
    this.markAsRead(id);
  }

  markAllAsRead(): void {
    this.notificationService.markAllAdminNotificationsAsRead().subscribe(() => {
      this.renderNotifications();
    });
  }

  get unreadCount(): number {
    return this.unreadCountValue;
  }

  get hasUnread(): boolean {
    return this.unreadCountValue > 0;
  }

  resetBroadcastForm(): void {
    this.broadcastData = {
      title: '',
      message: '',
      category: 'Announcement',
      targetAudience: 'AllUsers',
      targetRole: '',
      targetUserId: null,
    };
  }

  broadcastNotification(): void {
    if (!this.broadcastData.title || !this.broadcastData.message) return;

    this.notificationService.broadcastNotification(this.broadcastData).subscribe(() => {
      alert('Notification broadcasted successfully.');
      this.resetBroadcastForm();
      this.renderNotifications();
    });
  }
}
