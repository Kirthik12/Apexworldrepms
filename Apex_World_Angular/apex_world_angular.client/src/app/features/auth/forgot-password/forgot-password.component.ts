import { Component, ViewEncapsulation } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { NgIf } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-forgot-password',
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.css'],
  encapsulation: ViewEncapsulation.None,
  imports: [NgIf, FormsModule, RouterLink],
})
export class ForgotPasswordComponent {
  step: 1 | 2 = 1;
  email: string = '';
  token: string = '';
  newPassword: string = '';
  confirmPassword: string = '';

  isSubmitting: boolean = false;
  successMessage: string = '';
  errorMessage: string = '';

  constructor(
    private authService: AuthService,
    private router: Router,
  ) {}

  requestReset() {
    if (!this.email) {
      this.errorMessage = 'Email is required';
      return;
    }
    this.isSubmitting = true;
    this.errorMessage = '';

    this.authService.forgotPassword(this.email).subscribe({
      next: (res) => {
        this.isSubmitting = false;
        this.successMessage = 'A password reset token has been sent to your email.';
        this.step = 2; // Move to step 2 to enter token and new password
      },
      error: (err) => {
        this.isSubmitting = false;
        this.errorMessage =
          err.error?.message || 'Failed to request password reset. Check if email is correct.';
      },
    });
  }

  resetPassword() {
    if (!this.token || !this.newPassword || !this.confirmPassword) {
      this.errorMessage = 'All fields are required';
      return;
    }
    if (this.newPassword !== this.confirmPassword) {
      this.errorMessage = 'Passwords do not match';
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';

    const payload = {
      email: this.email,
      token: this.token,
      newPassword: this.newPassword,
    };

    this.authService.resetPassword(payload).subscribe({
      next: (res) => {
        this.isSubmitting = false;
        this.successMessage = 'Password reset successfully! Redirecting to login...';

        setTimeout(() => {
          this.router.navigate(['/login']);
        }, 2000);
      },
      error: (err) => {
        this.isSubmitting = false;
        this.errorMessage =
          err.error?.message || 'Failed to reset password. The token may be invalid or expired.';
      },
    });
  }
}
