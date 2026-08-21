import { Component, OnInit } from '@angular/core';
import { AdminHeader } from '../../shared/components/admin-header/admin-header';
import { FormsModule } from '@angular/forms';
import { NgIf } from '@angular/common';

import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-user-profile',
  templateUrl: './user-profile.component.html',
  styleUrls: ['./user-profile.component.css'],
  imports: [AdminHeader, FormsModule, NgIf],
})
export class UserProfileComponent implements OnInit {
  constructor(private toastService: ToastService) {}
  profile = {
    name: 'System Administrator',
    email: 'admin@apexworld.com',
    phone: '+91 99628 81452',
  };
  initials: string = 'AD';

  private adminProfileKey = 'admin_profile_data';

  ngOnInit(): void {
    this.loadProfile();
  }

  loadProfile(): void {
    try {
      const saved = localStorage.getItem(this.adminProfileKey);
      if (saved) {
        this.profile = JSON.parse(saved);
      }
    } catch (e) {
      // Keep defaults
    }
    this.updateInitials();
  }

  updateInitials(): void {
    if (this.profile.name) {
      this.initials =
        this.profile.name
          .split(' ')
          .map((w) => w[0])
          .join('')
          .substring(0, 2)
          .toUpperCase() || 'AD';
    } else {
      this.initials = 'AD';
    }
  }

  clearFields(): void {
    this.profile.name = '';
    this.profile.phone = '';
  }

  saveChanges(): void {
    localStorage.setItem(this.adminProfileKey, JSON.stringify(this.profile));
    this.updateInitials();
    this.toastService.success('Changes saved successfully');
  }
}
