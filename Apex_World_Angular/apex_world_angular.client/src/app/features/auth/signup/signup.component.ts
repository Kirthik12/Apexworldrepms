import { Component, ViewEncapsulation, ChangeDetectorRef } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { NgIf } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-signup',
  templateUrl: './signup.component.html',
  styleUrls: ['./signup.component.css'],
  encapsulation: ViewEncapsulation.None,
  imports: [RouterLink, NgIf, FormsModule],
})
export class SignupComponent {
  fullName = '';
  email = '';
  phone = '';
  dob = '';
  username = '';
  password = '';
  confirmPassword = '';

  isSubmitting = false;
  errorMessage = '';
  successMessage = '';

  // Field-level errors
  errors: { [key: string]: string } = {};

  constructor(
    private authService: AuthService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  submit(): void {
    this.errorMessage = '';
    this.successMessage = '';

    // Check DOB age manually before submitting
    if (this.dob) {
      const dobDate = new Date(this.dob);
      const today = new Date();
      let age = today.getFullYear() - dobDate.getFullYear();
      const m = today.getMonth() - dobDate.getMonth();
      if (m < 0 || (m === 0 && today.getDate() < dobDate.getDate())) age--;
      
      if (age < 18) {
        this.errors['dob'] = 'You must be at least 18 years old';
        return;
      } else {
        delete this.errors['dob'];
      }
    }

    if (this.password !== this.confirmPassword) {
      return;
    }

    this.isSubmitting = true;

    const registerData = {
      username: this.username.trim(),
      password: this.password,
      fullName: this.fullName.trim(),
      email: this.email.trim(),
      phoneNumber: this.phone.trim(),
      city: '',
    };

    this.authService.register(registerData).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.successMessage = 'Account created successfully! Redirecting to Login...';
        setTimeout(() => this.router.navigate(['/login']), 1500);
      },
      error: (err: any) => {
        console.error('Registration error caught:', err);
        this.isSubmitting = false;
        
        // Handle ASP.NET Core ValidationProblemDetails format
        if (err.error && err.error.errors) {
          const firstErrorKey = Object.keys(err.error.errors)[0];
          this.errorMessage = err.error.errors[firstErrorKey][0];
        } else {
          this.errorMessage = err.error?.message || 'Registration failed. Please try again.';
        }
        this.cdr.detectChanges();
      },
    });
  }
}
