import { Component, ViewEncapsulation } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { NgIf } from '@angular/common';
import { AdminHeader } from '../../components/admin-header/admin-header';

@Component({
  selector: 'app-admin-layout',
  templateUrl: './admin-layout.component.html',
  styleUrls: ['./admin-layout.component.css'],
  encapsulation: ViewEncapsulation.None,
  imports: [RouterLink, RouterLinkActive, NgIf, AdminHeader, RouterOutlet],
})
export class AdminLayoutComponent {
  constructor(
    public router: Router,
    private authService: AuthService,
  ) {}

  isDashboard(): boolean {
    return this.router.url === '/admin-dashboard' || this.router.url === '/admin-dashboard/';
  }

  logout(): void {
    this.authService.logout();
  }
}
