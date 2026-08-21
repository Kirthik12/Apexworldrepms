import { Component, ViewEncapsulation } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { NgIf } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css'],
  encapsulation: ViewEncapsulation.None,
  imports: [RouterLink, NgIf, FormsModule],
})
export class RegisterComponent {
  username = '';
  password = '';
  confirmPassword = '';
  email = '';
  fullName = '';
  phoneNumber = '';
  city = '';

  isSubmitting = false;
  errorMessage = '';

  constructor(
    private authService: AuthService,
    private router: Router,
    private toastService: ToastService
  ) {}

  register(): void {
    if (!this.username || !this.password) {
      this.errorMessage = 'Username and password are required.';
      return;
    }

    if (this.password !== this.confirmPassword) {
      this.errorMessage = 'Passwords do not match.';
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';

    const registerData = {
      username: this.username,
      password: this.password,
      email: this.email || undefined,
      fullName: this.fullName || undefined,
      phoneNumber: this.phoneNumber || undefined,
      city: this.city || undefined,
    };

    this.authService.register(registerData).subscribe({
      next: (res) => {
        this.isSubmitting = false;
        this.toastService.success('Registration successful. Please login.');
        this.router.navigate(['/login']);
      },
      error: (err) => {
        this.isSubmitting = false;
        this.errorMessage = err.error?.message || 'Registration failed. Please try again.';
      },
    });
  }
}
