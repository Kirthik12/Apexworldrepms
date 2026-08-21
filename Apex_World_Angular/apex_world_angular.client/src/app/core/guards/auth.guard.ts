import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {

  constructor(private authService: AuthService, private router: Router) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {
    
    const requiredRole = route.data['role'];
    const isLoggedIn = !!this.authService.getAccessToken();
    const userRole = this.authService.getRole();

    if (!isLoggedIn) {
      // Not logged in, redirect to login page with the return url
      this.router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
      return false;
    }

    if (requiredRole && userRole?.toLowerCase() !== requiredRole.toLowerCase()) {
      // Logged in but doesn't have the required role
      this.router.navigate(['/']);
      return false;
    }

    return true;
  }
  
}
