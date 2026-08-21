import { Component, ViewEncapsulation, OnInit, NgZone } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { LoginRequestDto, AuthResponse } from '../../../core/models/auth.model';
import { NgIf } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SocialAuthService, GoogleSigninButtonModule } from '@abacritt/angularx-social-login';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
  encapsulation: ViewEncapsulation.None,
  imports: [RouterLink, NgIf, FormsModule, GoogleSigninButtonModule],
})
export class LoginComponent implements OnInit {
  credentials: LoginRequestDto = {
    username: '',
    password: '',
  };
  isLoading = false;
  errorMessage = '';

  constructor(
    private authService: AuthService,
    private router: Router,
    private socialAuthService: SocialAuthService,
    private ngZone: NgZone
  ) {}

  ngOnInit(): void {
    console.log('SocialAuthService initialized, subscribing to authState...');
    this.socialAuthService.authState.subscribe((user) => {
      console.log('SocialAuthService authState emitted:', user);
      this.ngZone.run(() => {
        if (user) {
          this.isLoading = true;
          if (!user.idToken) {
            this.errorMessage = 'Google Authentication failed: No ID token provided.';
            this.isLoading = false;
            return;
          }
          
          this.authService.googleLogin(user.idToken).subscribe({
            next: (response: AuthResponse) => {
              this.isLoading = false;
              localStorage.setItem('isGoogleAuth', 'true'); // Keep this if used elsewhere
              
              // Redirect to the returnUrl from query params, or default to /buyer-dashboard
              const returnUrl = this.router.parseUrl(this.router.url).queryParams['returnUrl'] || '/buyer-dashboard';
              this.showLoaderAndRedirect('Google Authorized! Redirecting...', returnUrl);
            },
            error: (err: any) => {
              this.isLoading = false;
              if (err.error && err.error.message) {
                  this.errorMessage = err.error.message;
              } else if (err.error && typeof err.error === 'string') {
                  this.errorMessage = err.error;
              } else {
                  this.errorMessage = 'Google Authentication failed on server.';
              }
              console.error('Google Login error', err);
            }
          });
        } else {
          console.log('Auth state is null (user signed out or popup closed without auth)');
        }
      });
    }, (error) => {
      console.error('SocialAuthService authState error:', error);
    });
  }

  onSubmit() {
    if (!this.credentials.username || !this.credentials.password) {
      this.errorMessage = 'Please enter both username and password.';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.authService.login(this.credentials).subscribe({
      next: (response: AuthResponse) => {
        this.isLoading = false;

        // Show the existing premium loader animation
        this.showLoaderAndRedirect(
          response.role === 'Admin'
            ? 'Admin Authorized! Redirecting...'
            : 'Buyer Authorized! Redirecting...',
          response.role === 'Admin' ? '/admin-dashboard' : '/buyer-dashboard',
        );
      },
      error: (err: any) => {
        this.isLoading = false;
        // Try to get the specific error message from the backend's ApiResponse
        if (err.error && err.error.message) {
            this.errorMessage = err.error.message;
        } else if (err.error && typeof err.error === 'string') {
            this.errorMessage = err.error;
        } else {
            this.errorMessage = 'Authentication failed! Please review your credentials.';
        }
        console.error('Login error', err);
      },
    });
  }

  private showLoaderAndRedirect(msg: string, url: string) {
    let loader = document.createElement('div');
    loader.style.position = 'fixed';
    loader.style.top = '0';
    loader.style.left = '0';
    loader.style.width = '100%';
    loader.style.height = '100%';
    loader.style.background = 'linear-gradient(135deg, #0f172a 0%, #1e1b4b 100%)';
    loader.style.zIndex = '999999';
    loader.style.display = 'flex';
    loader.style.flexDirection = 'column';
    loader.style.justifyContent = 'center';
    loader.style.alignItems = 'center';

    const style = document.createElement('style');
    style.innerHTML = `
      .premium-loader-container { display: flex; flex-direction: column; align-items: center; justify-content: center; position: relative; z-index: 10; }
      .glowing-ring { width: 80px; height: 80px; border-radius: 50%; border: 2px solid rgba(255,255,255,0.1); border-top-color: #818cf8; border-right-color: #c084fc; animation: spin-glow 1.5s cubic-bezier(0.4, 0, 0.2, 1) infinite; box-shadow: 0 0 20px rgba(129, 140, 248, 0.4); margin-bottom: 30px; }
      .inner-ring { width: 40px; height: 40px; border-radius: 50%; border: 2px solid rgba(255,255,255,0.1); border-bottom-color: #38bdf8; border-left-color: #818cf8; animation: spin-glow-reverse 2s cubic-bezier(0.4, 0, 0.2, 1) infinite; position: absolute; top: 20px; }
      @keyframes spin-glow { 0% { transform: rotate(0deg); filter: drop-shadow(0 0 5px #818cf8); } 100% { transform: rotate(360deg); filter: drop-shadow(0 0 15px #c084fc); } }
      @keyframes spin-glow-reverse { 0% { transform: rotate(360deg); } 100% { transform: rotate(0deg); } }
      .status-text { color: #f8fafc; font-family: 'Inter', sans-serif; font-weight: 500; font-size: 1.1rem; letter-spacing: 0.5px; opacity: 1; transition: opacity 0.4s ease, transform 0.4s ease; text-shadow: 0 2px 10px rgba(0,0,0,0.5); }
      .status-text.fade { opacity: 0; transform: translateY(5px); }
      .glass-panel { position: absolute; width: 100%; height: 100%; backdrop-filter: blur(12px); background: radial-gradient(circle at 50% 50%, rgba(30, 27, 75, 0.4) 0%, rgba(15, 23, 42, 0.8) 100%); z-index: 1; }
    `;
    document.head.appendChild(style);

    loader.innerHTML = `
      <div class="glass-panel"></div>
      <div class="premium-loader-container">
        <div class="glowing-ring"></div>
        <div class="inner-ring"></div>
        <div id="loading-msg" class="status-text">${msg}</div>
      </div>
    `;

    document.body.appendChild(loader);

    setTimeout(() => {
      // Remove loader when angular router navigates
      document.body.removeChild(loader);
      this.router.navigate([url]);
    }, 2000); // reduced timeout for better UX
  }
}
