import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from './property.service';
import { PropertyDto } from '../models/property.model';

@Injectable({
  providedIn: 'root'
})
export class WishlistService {
  private apiUrl = `${environment.apiUrl}/Wishlist`;

  constructor(private http: HttpClient) { }

  getWishlistedProperties(): Observable<ApiResponse<{items: PropertyDto[], totalCount: number} | PropertyDto[]>> {
    return this.http.get<ApiResponse<{items: PropertyDto[], totalCount: number} | PropertyDto[]>>(`${this.apiUrl}/properties`);
  }

  addToWishlist(propertyId: number): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/${propertyId}`, {});
  }

  removeFromWishlist(propertyId: number): Observable<ApiResponse<any>> {
    return this.http.delete<ApiResponse<any>>(`${this.apiUrl}/${propertyId}`);
  }

  bulkRemoveFromWishlist(propertyIds: number[]): Observable<ApiResponse<any>> {
    return this.http.delete<ApiResponse<any>>(`${this.apiUrl}/bulk`, { body: propertyIds });
  }
}
