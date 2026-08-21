import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PaginatedResponse } from './property.service';
import { BookingDto, BookingRequestDto } from '../models/booking.model';

@Injectable({
  providedIn: 'root'
})
export class BookingService {
  private buyerApiUrl = `${environment.apiUrl}/BuyerBooking`;
  private adminApiUrl = `${environment.apiUrl}/AdminBooking`;

  constructor(private http: HttpClient) { }

  // --- Buyer Methods ---

  getBuyerBookings(): Observable<ApiResponse<any[]>> {
    return this.http.get<ApiResponse<any[]>>(this.buyerApiUrl);
  }

  createBooking(bookingReq: BookingRequestDto): Observable<ApiResponse<BookingDto>> {
    // Adding Idempotency-Key as suggested by swagger
    const headers = new HttpHeaders().set('Idempotency-Key', crypto.randomUUID());
    return this.http.post<ApiResponse<BookingDto>>(`${this.buyerApiUrl}/book`, bookingReq, { headers });
  }

  cancelBooking(id: number): Observable<ApiResponse<any>> {
    return this.http.delete<ApiResponse<any>>(`${this.buyerApiUrl}/${id}`);
  }

  rescheduleBooking(id: number, newDate: string): Observable<ApiResponse<BookingDto>> {
    return this.http.patch<ApiResponse<BookingDto>>(`${this.buyerApiUrl}/${id}/reschedule`, { newDate }, {
      headers: new HttpHeaders({ 'Content-Type': 'application/json' })
    });
  }

  getBuyerBookingById(id: number): Observable<ApiResponse<BookingDto>> {
    return this.http.get<ApiResponse<BookingDto>>(`${this.buyerApiUrl}/${id}`);
  }

  markVisited(id: number): Observable<ApiResponse<any>> {
    return this.http.patch<ApiResponse<any>>(`${this.buyerApiUrl}/${id}/mark-visited`, {});
  }

  recordInterest(id: number, interest: string): Observable<ApiResponse<any>> {
    return this.http.patch<ApiResponse<any>>(`${this.buyerApiUrl}/${id}/interest`, { interest });
  }

  // --- Admin Methods ---

  getAdminBookings(pageNumber: number = 1, pageSize: number = 20, onlyPurchased?: boolean): Observable<ApiResponse<PaginatedResponse<BookingDto>>> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (onlyPurchased !== undefined) {
      params = params.set('onlyPurchased', onlyPurchased.toString());
    }

    return this.http.get<ApiResponse<PaginatedResponse<BookingDto>>>(this.adminApiUrl, { params });
  }

  getAdminBookingById(id: number): Observable<ApiResponse<BookingDto>> {
    return this.http.get<ApiResponse<BookingDto>>(`${this.adminApiUrl}/${id}`);
  }

  approveBooking(id: number): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.adminApiUrl}/${id}/approve`, {});
  }

  rejectBooking(id: number, reason: string = ''): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.adminApiUrl}/${id}/reject`, { reason });
  }

  approveReschedule(id: number): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.adminApiUrl}/${id}/reschedule/approve`, {});
  }

  rejectReschedule(id: number, reason: string = ''): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.adminApiUrl}/${id}/reschedule/reject`, { reason });
  }

  approveCancellation(id: number): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.adminApiUrl}/${id}/cancel/approve`, {});
  }

  rejectCancellation(id: number, reason: string = ''): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.adminApiUrl}/${id}/cancel/reject`, { reason });
  }
}
