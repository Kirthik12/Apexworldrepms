import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-admin-header',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './admin-header.html',
  styleUrls: ['./admin-header.css'],
})
export class AdminHeader implements OnInit {
  @Input() breadcrumb: string = 'Admin Workspace';
  @Input() title: string = 'Dashboard';
  
  // Custom button inputs
  @Input() actionButtonText: string = '';
  @Input() actionButtonTarget: string = '';
  @Output() actionClick = new EventEmitter<void>();

  onActionClick() {
    this.actionClick.emit();
  }

  unreadCount: number = 0;

  ngOnInit(): void {
    // Optionally fetch real unread count here later, 
    // for now we pull from the local mock DB if available to display a badge
    this.calculateUnreadCount();
  }

  calculateUnreadCount(): void {
    if (typeof (window as any).DB !== 'undefined') {
      const data = (window as any).DB.get("notifications");
      if (data) {
        this.unreadCount = data.filter((n: any) => !n.read).length;
      }
    } else {
      // Fallback for localStorage if DB script is not globally available in this scope
      const localData = localStorage.getItem("notifications");
      if (localData) {
        try {
          const parsed = JSON.parse(localData);
          this.unreadCount = parsed.filter((n: any) => !n.read).length;
        } catch (e) {}
      }
    }
  }
}
