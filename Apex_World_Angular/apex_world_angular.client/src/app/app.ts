import { Component, AfterViewInit, OnInit } from '@angular/core';
import { Router, RouterOutlet, NavigationEnd } from '@angular/router';
import { ToastComponent } from './shared/components/toast/toast.component';
import { ToastService } from './core/services/toast.service';
import { AuthService } from './core/services/auth.service';
import { AiCompanionService } from './core/services/ai-companion.service';
import { AiCompanionDrawerComponent } from './features/property-management/buyer-property-details/ai-companion-drawer/ai-companion-drawer.component';
import { CommonModule } from '@angular/common';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  standalone: true,
  imports: [RouterOutlet, ToastComponent, CommonModule, AiCompanionDrawerComponent]
})
export class App implements AfterViewInit, OnInit {
  title = 'ApexWorld';
  isAiDrawerOpen = false;
  propertyContextId: number | null = null;

  constructor(
    private router: Router, 
    private toastService: ToastService,
    private authService: AuthService,
    private aiCompanionService: AiCompanionService
  ) {
    (window as any).toastService = this.toastService;
  }

  ngAfterViewInit() {
    // Global interceptor for legacy vanilla JS redirects and HTML links
    document.body.addEventListener('click', (e: any) => {
      // 1. Intercept legacy anchor tags
      const link = e.target.closest('a');
      if (link && link.getAttribute('href')) {
        const href = link.getAttribute('href').toLowerCase();
        if (href.includes('.html')) {
          e.preventDefault();
          e.stopPropagation();
          this.routeLegacyPath(href);
          return;
        }
      }

      // 2. Intercept legacy generic buttons that used to redirect to login
      const btn = e.target.closest("button, .btn, .section-link");
      if (btn) {
        if (btn.closest("#enquiry-desk") || btn.closest("#top-offers-banner") || btn.closest(".search-container") || btn.closest(".cancel-modal-overlay")) return;
        
        // Exclude specific submit buttons that shouldn't redirect to login globally
        if (btn.getAttribute('type') === 'submit' || btn.id === 'signup-submit') return;

        // If it's a generic unhandled button on the landing page, it historically went to login
        if (window.location.pathname === '/' && !link) {
            e.preventDefault();
            e.stopPropagation();
            this.router.navigate(['/login']);
        }
      }
    });

    // Intercept window.location.href changes if possible, or just intercept click
  }

  private routeLegacyPath(href: string) {
    if (href.includes('login.html')) {
      this.router.navigate(['/login']);
    } else if (href.includes('signup.html')) {
      this.router.navigate(['/signup']);
    } else if (href.includes('forgot_password.html')) {
      this.router.navigate(['/forgot-password']);
    } else if (href.includes('change_password.html')) {
      this.router.navigate(['/change-password']);
    } else if (href.includes('logout_loading.html')) {
      this.router.navigate(['/logout']);
    } else if (href.includes('index.html')) {
      this.router.navigate(['/']);
    } else if (href.includes('admin_dashboard.html')) {
      this.router.navigate(['/admin-dashboard']);
    } else if (href.includes('buyer.html')) {
      this.router.navigate(['/buyer-dashboard']);
    } else {
      console.warn('Unhandled legacy path:', href);
    }
  }

  ngOnInit() {
    this.aiCompanionService.isOpen$.subscribe(open => {
      this.isAiDrawerOpen = open;
    });

    this.aiCompanionService.propertyId$.subscribe(id => {
      this.propertyContextId = id;
    });

    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: any) => {
      const url = event.urlAfterRedirects || event.url || '';
      // Dynamically extract ID parameter if on a property details view
      if (url.includes('property-details') || url.includes('property_details')) {
        const match = url.match(/[?&]id=(\d+)/);
        if (match) {
          const propId = parseInt(match[1], 10);
          this.aiCompanionService.setPropertyContext(propId);
          return;
        }
      }
      this.aiCompanionService.setPropertyContext(null);
    });
  }

  get isBuyerLoggedIn(): boolean {
    const isLoggedIn = localStorage.getItem('isLoggedIn') === 'true';
    const role = localStorage.getItem('userRole');
    return isLoggedIn && role === 'Buyer';
  }

  toggleAiDrawer() {
    this.aiCompanionService.toggle(this.propertyContextId);
  }

  closeAiDrawer() {
    this.aiCompanionService.close();
  }
}
