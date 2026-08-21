import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PaymentInitiateRequestDto } from '../models/payment.model';
import { ApiResponse } from './property.service';

@Injectable({
  providedIn: 'root'
})
export class PaymentService {
  private apiUrl = `${environment.apiUrl}/Payment`;
  private adminApiUrl = `${environment.apiUrl}/admin/AdminPayment`;

  constructor(private http: HttpClient) { }

  initiatePayment(request: PaymentInitiateRequestDto): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/initiate`, request);
  }

  verifyPayment(paymentLinkId: string): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/verify/${paymentLinkId}`, {});
  }

  getAdminPayments(): Observable<ApiResponse<any>> {
    return this.http.get<ApiResponse<any>>(this.adminApiUrl);
  }
}
