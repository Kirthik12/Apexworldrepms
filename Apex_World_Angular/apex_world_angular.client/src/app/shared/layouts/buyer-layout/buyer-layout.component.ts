import {
  Component,
  AfterViewInit,
  ViewEncapsulation,
  HostListener,
  ElementRef,
  ChangeDetectorRef,
  OnInit,
} from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { NotificationBellComponent } from '../../components/notification-bell/notification-bell.component';
import { UserService } from '../../../core/services/user.service';

@Component({
  selector: 'app-buyer-layout',
  templateUrl: './buyer-layout.component.html',
  styleUrls: ['./buyer-layout.component.css'],
  encapsulation: ViewEncapsulation.None,
  imports: [
    RouterLink,
    RouterLinkActive,
    NotificationBellComponent,
    RouterOutlet,
  ],
})
export class BuyerLayoutComponent implements OnInit, AfterViewInit {
  isUserDropdownOpen = false;
  displayName = 'Dashboard';

  constructor(
    private router: Router,
    private eRef: ElementRef,
    private userService: UserService,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit() {
    this.loadUserDisplayName();
    // Watch for potential profile changes via window events or localStorage polling
    window.addEventListener('profileUpdated', () => {
      this.loadUserDisplayName();
    });
  }

  loadUserDisplayName() {
    this.userService.getProfile().subscribe({
      next: (profile) => {
        if (profile) {
          const first = profile.firstName || '';
          const last = profile.lastName || '';
          const full = `${first} ${last}`.trim();
          this.displayName = full || profile.email || 'Buyer';
          this.cdr.detectChanges();
        }
      },
      error: () => {
        const storedUsername = localStorage.getItem('username');
        if (storedUsername) {
          this.displayName = storedUsername;
          this.cdr.detectChanges();
        }
      }
    });
  }

  ngAfterViewInit() {
    // Top-bar slide rotation
    const slides = document.querySelectorAll('.top-offer-slide');
    let curSlide = 0;
    if (slides.length > 0) {
      setInterval(() => {
        (slides[curSlide] as HTMLElement).style.opacity = '0';
        (slides[curSlide] as HTMLElement).style.pointerEvents = 'none';
        curSlide = (curSlide + 1) % slides.length;
        (slides[curSlide] as HTMLElement).style.opacity = '1';
        (slides[curSlide] as HTMLElement).style.pointerEvents = 'auto';
      }, 3000);
    }
  }

  toggleDropdown(event: Event) {
    event.stopPropagation();
    this.isUserDropdownOpen = !this.isUserDropdownOpen;
  }

  @HostListener('document:click', ['$event'])
  clickout(event: Event) {
    // If click is outside the dropdown container, close it
    if (!this.eRef.nativeElement.querySelector('.user-dropdown')?.contains(event.target)) {
      this.isUserDropdownOpen = false;
    }
  }

  logout() {
    this.isUserDropdownOpen = false;
    localStorage.removeItem('isLoggedIn');
    this.router.navigate(['/login']);
  }
}

