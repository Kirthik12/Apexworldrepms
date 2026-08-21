import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError, BehaviorSubject } from 'rxjs';
import { catchError, filter, take, switchMap } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  private isRefreshing = false;
  private refreshTokenSubject: BehaviorSubject<any> = new BehaviorSubject<any>(null);

  constructor(public authService: AuthService) {}

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    
    // Attach token to outgoing requests
    if (this.authService.getAccessToken()) {
      request = this.addToken(request, this.authService.getAccessToken()!);
    }

    return next.handle(request).pipe(
      catchError((error) => {
        if (error instanceof HttpErrorResponse && error.status === 401) {
          // Ignore 401s from login or refresh endpoints
          if (request.url.includes('/Auth/login') || request.url.includes('/Auth/google-login') || request.url.includes('/Auth/refresh') || request.url.includes('/Auth/logout')) {
             return throwError(() => error);
          }
          return this.handle401Error(request, next);
        } else {
          return throwError(() => error);
        }
      })
    );
  }

  private addToken(request: HttpRequest<any>, token: string) {
    return request.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  private handle401Error(request: HttpRequest<any>, next: HttpHandler) {
    if (!this.isRefreshing) {
      this.isRefreshing = true;
      this.refreshTokenSubject.next(null);

      const token = this.authService.getRefreshToken();
      
        if (token) {
        return this.authService.refreshToken().pipe(
          switchMap((tokenResponse: any) => {
            this.isRefreshing = false;
            this.refreshTokenSubject.next(tokenResponse.accessToken);
            return next.handle(this.addToken(request, tokenResponse.accessToken));
          }),
          catchError((err) => {
            this.isRefreshing = false;
            if (localStorage.getItem('isGoogleAuth') !== 'true') {
              this.authService.logout();
            }
            return throwError(() => err);
          })
        );
      } else {
        this.isRefreshing = false;
        if (localStorage.getItem('isGoogleAuth') !== 'true') {
          this.authService.logout();
        }
        return throwError(() => new Error('No refresh token available'));
      }
    }

    return this.refreshTokenSubject.pipe(
      filter(token => token !== null),
      take(1),
      switchMap((token) => next.handle(this.addToken(request, token)))
    );
  }
}


