import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from './property.service';

export interface EnquiryDto {
  id: number;
  name?: string;
  buyerName: string;
  email: string;
  phone: string;
  subject?: string;
  message: string;
  status: string;
  adminResponse?: string;
  responseDate?: string;
  createdAt: string;
}

export interface CreateEnquiryDto {
  name: string;
  email: string;
  phone: string;
  subject: string;
  message: string;
}

@Injectable({
  providedIn: 'root'
})
export class EnquiryService {
  private apiUrl = `${environment.apiUrl}/Enquiry`;
  private adminApiUrl = `${environment.apiUrl}/admin/AdminEnquiry`;

  constructor(private http: HttpClient) { }

  // Public
  submitEnquiry(dto: CreateEnquiryDto): Observable<ApiResponse<number>> {
    // Map frontend DTO to backend EnquiryRequestDto properties
    const backendRequest = {
      buyerName: dto.name,
      email: dto.email,
      phone: dto.phone,
      message: dto.message
    };
    return this.http.post<ApiResponse<number>>(this.apiUrl, backendRequest);
  }

  // Admin
  getAllEnquiries(): Observable<ApiResponse<EnquiryDto[]>> {
    return this.http.get<ApiResponse<EnquiryDto[]>>(this.adminApiUrl);
  }

  resolveEnquiry(id: number, adminResponse: string): Observable<ApiResponse<any>> {
    return this.http.patch<ApiResponse<any>>(`${this.adminApiUrl}/${id}/resolve`, { adminResponse });
  }

  deleteEnquiry(id: number): Observable<ApiResponse<any>> {
    return this.http.delete<ApiResponse<any>>(`${this.adminApiUrl}/${id}`);
  }
}
