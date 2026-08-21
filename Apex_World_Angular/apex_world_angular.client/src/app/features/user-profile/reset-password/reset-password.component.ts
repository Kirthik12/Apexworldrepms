import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgIf } from '@angular/common';

import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-reset-password',
  templateUrl: './reset-password.component.html',
  styleUrls: ['./reset-password.component.css'],
  imports: [FormsModule, NgIf],
})
export class ResetPasswordComponent {
  currentPassword = '';
  newPassword = '';
  confirmPassword = '';

  // UI State
  showCurrentPassword = false;
  showNewPassword = false;
  showConfirmPassword = false;

  constructor(private toastService: ToastService) {}

  toggleVisibility(field: 'current' | 'new' | 'confirm') {
    if (field === 'current') this.showCurrentPassword = !this.showCurrentPassword;
    if (field === 'new') this.showNewPassword = !this.showNewPassword;
    if (field === 'confirm') this.showConfirmPassword = !this.showConfirmPassword;
  }

  onSubmit() {
    if (!this.currentPassword || !this.newPassword || !this.confirmPassword) {
      this.toastService.error('Please fill all fields.');
      return;
    }

    if (this.newPassword !== this.confirmPassword) {
      this.toastService.error('New passwords do not match!');
      return;
    }

    if (this.newPassword.length < 6) {
      this.toastService.error('Password must be at least 6 characters.');
      return;
    }

    this.toastService.success('Password Updated Successfully!');
    this.currentPassword = '';
    this.newPassword = '';
    this.confirmPassword = '';

    // Reset visibility states
    this.showCurrentPassword = false;
    this.showNewPassword = false;
    this.showConfirmPassword = false;
  }
}
