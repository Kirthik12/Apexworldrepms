import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { User, PagedResponse } from '../models/user.model';

export interface UserProfileDto {
  id: number;
  email: string;
  firstName?: string;
  lastName?: string;
  phoneNumber?: string;
  address?: string;
  role: string;
}

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private apiUrl = `${environment.apiUrl}/Users`;

  constructor(private http: HttpClient) { }

  getProfile(): Observable<UserProfileDto> {
    return this.http.get<UserProfileDto>(`${this.apiUrl}/me`);
  }

  updateProfile(profileData: any): Observable<UserProfileDto> {
    return this.http.put<UserProfileDto>(`${this.apiUrl}/me`, profileData);
  }

  getBuyers(pageNumber: number = 1, pageSize: number = 10): Observable<PagedResponse<User>> {
    return this.http.get<PagedResponse<User>>(`${this.apiUrl}/buyers?pageNumber=${pageNumber}&pageSize=${pageSize}`);
  }

  toggleUserStatus(userId: number): Observable<any> {
    return this.http.patch(`${this.apiUrl}/${userId}/toggle-active`, {});
  }
}
