// @ts-nocheck
import { NgIf } from '@angular/common';
import { Component, AfterViewInit, ViewEncapsulation } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-change-password',
  templateUrl: './change-password.component.html',
  styleUrls: ['./change-password.component.css'],
  encapsulation: ViewEncapsulation.None,
  imports: [FormsModule, NgIf],
})
export class ChangePasswordComponent implements AfterViewInit {
  constructor(private authService: AuthService, private toastService: ToastService) {}
  ngAfterViewInit() {
    // =====================================================================
    //  CHANGE PASSWORD CONTROLLER
    // =====================================================================
    'use strict';

    // Handle Dropdown globally and Buyer Name fetching
    

    window.addEventListener('click', function (e) {
      if (!e.target.closest('.user-dropdown')) {
        var dropdowns = document.getElementsByClassName('user-dropdown-menu');
        for (var i = 0; i < dropdowns.length; i++) {
          var openDropdown = dropdowns[i];
          if (openDropdown.classList.contains('show')) {
            openDropdown.classList.remove('show');
          }
        }
      }
    });

    // Show/Hide Password Toggle
    window.togglePwd = function(inputId, iconEl) {
      const input = document.getElementById(inputId);
      if (input.type === 'password') {
        input.type = 'text';
        iconEl.textContent = '🙈';
      } else {
        input.type = 'password';
        iconEl.textContent = '👁️';
      }
    };

    // Form Submission & Validation
    document.getElementById('cp-form').addEventListener('submit', (e) => {
      e.preventDefault();

      const current = document.getElementById('currentPwd');
      const newPwd = document.getElementById('newPwd');
      const confirmPwd = document.getElementById('confirmPwd');

      // Reset invalid classes
      newPwd.classList.remove('invalid');
      confirmPwd.classList.remove('invalid');

      if (newPwd.value !== confirmPwd.value) {
        newPwd.classList.add('invalid');
        confirmPwd.classList.add('invalid');
        alert('⚠️ New password and confirmation do not match.');
        return;
      }

      if (newPwd.value.length < 8) {
        newPwd.classList.add('invalid');
        alert('⚠️ Password must be at least 8 characters long.');
        return;
      }

      // Secure password change via API
      this.authService.changePassword({ 
        currentPassword: current.value, 
        newPassword: newPwd.value 
      }).subscribe({
        next: (res) => {
          this.toastService.success('Password updated successfully!');

          // Clear inputs
          current.value = '';
          newPwd.value = '';
          confirmPwd.value = '';
        },
        error: (err) => {
          alert('Failed to update password. Please check your current password.');
        }
      });
    });
  }
}


