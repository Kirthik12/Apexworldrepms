import { Component, OnInit, ViewEncapsulation, ChangeDetectorRef } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { UserService, UserProfileDto } from '../../../core/services/user.service';
import { AuthService } from '../../../core/services/auth.service';
import { NgIf, NgClass } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-buyer-profile',
  templateUrl: './buyer-profile.component.html',
  styleUrls: ['./buyer-profile.component.css'],
  encapsulation: ViewEncapsulation.None,
  imports: [RouterLink, NgIf, FormsModule],
})
export class BuyerProfileComponent implements OnInit {
  profile: UserProfileDto = {
    id: 0,
    email: '',
    firstName: '',
    lastName: '',
    phoneNumber: '',
    address: '',
    role: 'Buyer',
  };

  initials: string = 'U';
  fullName: string = '';

  showDeleteModal: boolean = false;
  deleteConfirmText: string = '';

  constructor(
    private userService: UserService,
    private authService: AuthService,
    private router: Router,
    private cdr: ChangeDetectorRef,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.loadProfile();
  }

  loadProfile(): void {
    this.userService.getProfile().subscribe({
      next: (data: any) => {
        if (data) {
          this.profile = data;
          this.fullName = `${this.profile.firstName || ''} ${this.profile.lastName || ''}`.trim();
          this.initials = this.getInitials(this.fullName);
          localStorage.setItem('username', this.fullName || this.profile.email);
          this.cdr.detectChanges();
        }
      },
      error: (err: any) => {
        console.error('Failed to load profile', err);
      },
    });
  }

  getInitials(name: string): string {
    if (!name) return 'U';
    return name
      .trim()
      .split(' ')
      .filter(Boolean)
      .slice(0, 2)
      .map((w) => w[0].toUpperCase())
      .join('');
  }

  updateProfile(): void {
    if (!this.fullName || !this.profile.email || !this.profile.phoneNumber) {
      this.toastService.error('Please fill in all required fields.');
      return;
    }

    const nameParts = this.fullName.trim().split(' ');
    this.profile.firstName = nameParts[0] || '';
    this.profile.lastName = nameParts.length > 1 ? nameParts.slice(1).join(' ') : '';

    this.userService.updateProfile(this.profile).subscribe({
      next: (data: any) => {
        if (data) {
          this.profile = data;
          this.fullName = `${this.profile.firstName || ''} ${this.profile.lastName || ''}`.trim();
          this.initials = this.getInitials(this.fullName);
          localStorage.setItem('username', this.fullName || this.profile.email);
          window.dispatchEvent(new Event('profileUpdated'));
          this.cdr.detectChanges();
          this.toastService.success(`Profile updated successfully! Welcome, ${this.fullName} 👋`);
        }
      },
      error: (err: any) => {
        this.toastService.error('Failed to update profile.');
        console.error(err);
      },
    });
  }

  openDeleteModal(): void {
    this.deleteConfirmText = '';
    this.showDeleteModal = true;
  }

  closeDeleteModal(): void {
    this.showDeleteModal = false;
  }

  confirmDelete(): void {
    if (this.deleteConfirmText === 'DELETE') {
      this.toastService.error('Account deleted. Redirecting...');
      setTimeout(() => {
        this.authService.logout();
      }, 2000);
    }
  }

  toggleTheme(): void {
    const isDark = !document.body.classList.contains('dark-mode');
    if (isDark) {
      document.body.classList.add('dark-mode');
      localStorage.setItem('apex_profile_theme', 'dark');
    } else {
      document.body.classList.remove('dark-mode');
      localStorage.setItem('apex_profile_theme', 'light');
    }
  }

  isDarkMode(): boolean {
    return document.body.classList.contains('dark-mode');
  }
}
