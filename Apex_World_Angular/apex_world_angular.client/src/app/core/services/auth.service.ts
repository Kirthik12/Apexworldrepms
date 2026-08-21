import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, tap, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LoginRequestDto, AuthResponse, ApiTokenResponse, RefreshTokenRequestDto, LogoutRequestDto, RegisterBuyerDto } from '../models/auth.model';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = `${environment.apiUrl}/Auth`;

  private loggedIn = new BehaviorSubject<boolean>(this.hasToken());
  public isLoggedIn$ = this.loggedIn.asObservable();

  constructor(private http: HttpClient, private router: Router) { }

  login(credentials: LoginRequestDto): Observable<AuthResponse> {
    return this.http.post<ApiTokenResponse>(`${this.apiUrl}/login`, credentials).pipe(
      map(response => {
        // Unwrap ApiResponse wrapper: { success, data: { accessToken, refreshToken }, message }
        if (response && response.success && response.data) {
          const accessToken = response.data.accessToken;
          const refreshToken = response.data.refreshToken;
          // Decode JWT to extract role and username
          const payload = this.decodeJwt(accessToken);
          const role = payload?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
            || payload?.['role']
            || '';
          const username = payload?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name']
            || payload?.['unique_name']
            || payload?.['name']
            || '';
          return { accessToken, refreshToken, role, username } as AuthResponse;
        }
        throw new Error('Login failed: invalid response');
      }),
      tap(authResult => {
        if (authResult.accessToken) {
          this.setTokens(authResult);
          this.loggedIn.next(true);
        }
      })
    );
  }

  googleLogin(idToken: string): Observable<AuthResponse> {
    return this.http.post<ApiTokenResponse>(`${this.apiUrl}/google-login`, { idToken }).pipe(
      map(response => {
        if (response && response.success && response.data) {
          const accessToken = response.data.accessToken;
          const refreshToken = response.data.refreshToken;
          const payload = this.decodeJwt(accessToken);
          const role = payload?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
            || payload?.['role']
            || '';
          const username = payload?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name']
            || payload?.['unique_name']
            || payload?.['name']
            || '';
          return { accessToken, refreshToken, role, username } as AuthResponse;
        }
        throw new Error('Google Login failed: invalid response');
      }),
      tap(authResult => {
        if (authResult.accessToken) {
          this.setTokens(authResult);
          this.loggedIn.next(true);
        }
      })
    );
  }

  register(data: RegisterBuyerDto): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/register-buyer`, data);
  }

  forgotPassword(email: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/forgot-password`, { email });
  }

  resetPassword(data: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/reset-password`, data);
  }

  changePassword(data: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/change-password`, data);
  }

  logout(): void {
    const refreshToken = this.getRefreshToken();
    if (refreshToken) {
      const payload: LogoutRequestDto = { refreshToken };
      this.http.post(`${this.apiUrl}/logout`, payload).subscribe({
        next: () => this.clearTokens(),
        error: () => this.clearTokens()
      });
    } else {
      this.clearTokens();
    }
  }

  refreshToken(): Observable<AuthResponse> {
    const payload: RefreshTokenRequestDto = {
      accessToken: this.getAccessToken() || undefined,
      refreshToken: this.getRefreshToken() || undefined
    };

    return this.http.post<ApiTokenResponse>(`${this.apiUrl}/refresh`, payload).pipe(
      map(response => {
        if (response && response.success && response.data) {
          const accessToken = response.data.accessToken;
          const refreshToken = response.data.refreshToken;
          const decoded = this.decodeJwt(accessToken);
          const role = decoded?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
            || decoded?.['role'] || '';
          const username = decoded?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name']
            || decoded?.['name'] || '';
          return { accessToken, refreshToken, role, username } as AuthResponse;
        }
        throw new Error('Token refresh failed');
      }),
      tap(authResult => {
        if (authResult.accessToken) this.setTokens(authResult);
      })
    );
  }

  /** Decodes a JWT and returns its payload as a plain object */
  private decodeJwt(token: string): any {
    try {
      const base64Url = token.split('.')[1];
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      const jsonPayload = decodeURIComponent(
        atob(base64).split('').map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2)).join('')
      );
      return JSON.parse(jsonPayload);
    } catch {
      return null;
    }
  }

  // Token Management
  private setTokens(authResult: AuthResponse): void {
    if (authResult.accessToken) localStorage.setItem('accessToken', authResult.accessToken);
    if (authResult.refreshToken) localStorage.setItem('refreshToken', authResult.refreshToken);
    if (authResult.role) localStorage.setItem('userRole', authResult.role);
    if (authResult.username) localStorage.setItem('username', authResult.username);
    localStorage.setItem('isLoggedIn', 'true');
  }

  private clearTokens(): void {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('userRole');
    localStorage.removeItem('username');
    localStorage.setItem('isLoggedIn', 'false');
    this.loggedIn.next(false);
    this.router.navigate(['/login']);
  }

  getAccessToken(): string | null {
    return localStorage.getItem('accessToken');
  }

  getRefreshToken(): string | null {
    return localStorage.getItem('refreshToken');
  }

  getRole(): string | null {
    return localStorage.getItem('userRole');
  }

  private hasToken(): boolean {
    return !!this.getAccessToken();
  }
}
