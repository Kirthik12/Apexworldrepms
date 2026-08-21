import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ReviewDto, CreatePlatformReviewDto, CreatePropertyReviewDto } from '../models/review.model';
import { ApiResponse } from './property.service';

@Injectable({
  providedIn: 'root'
})
export class ReviewService {
  private buyerApiUrl = `${environment.apiUrl}/BuyerReview`;
  private adminApiUrl = `${environment.apiUrl}/AdminReview`;

  constructor(private http: HttpClient) { }

  // --- Buyer ---

  getBuyerReviews(): Observable<ApiResponse<ReviewDto[]>> {
    return this.http.get<ApiResponse<ReviewDto[]>>(this.buyerApiUrl);
  }

  submitPlatformReview(dto: CreatePlatformReviewDto): Observable<ApiResponse<number>> {
    return this.http.post<ApiResponse<number>>(`${this.buyerApiUrl}/platform`, dto);
  }

  submitPropertyReview(dto: CreatePropertyReviewDto): Observable<ApiResponse<number>> {
    return this.http.post<ApiResponse<number>>(`${this.buyerApiUrl}/property`, dto);
  }

  deleteBuyerReview(id: number): Observable<ApiResponse<any>> {
    return this.http.delete<ApiResponse<any>>(`${this.buyerApiUrl}/${id}`);
  }

  // --- Admin ---

  getAllReviews(reviewType?: string): Observable<ApiResponse<ReviewDto[]>> {
    let url = this.adminApiUrl;
    if (reviewType) {
      url += `?reviewType=${reviewType}`;
    }
    return this.http.get<ApiResponse<ReviewDto[]>>(url);
  }

  deleteAdminReview(id: number): Observable<ApiResponse<any>> {
    return this.http.delete<ApiResponse<any>>(`${this.adminApiUrl}/${id}`);
  }

  respondToReview(id: number, adminResponse: string): Observable<ApiResponse<any>> {
    return this.http.patch<ApiResponse<any>>(`${this.adminApiUrl}/${id}/respond`, { adminResponse });
  }
}
