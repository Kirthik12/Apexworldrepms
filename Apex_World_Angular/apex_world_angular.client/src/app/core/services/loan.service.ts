import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from './property.service';

export interface LoanApplicationDto {
  id: number;
  buyerId: number;
  buyerName: string;
  bookingId: number;
  propertyId: number;
  propertyName?: string;
  propertyValue?: number;
  loanAmount: number;
  bankName: string;
  interestRate?: number;
  tenureYears: number;
  monthlyEMI?: number;
  employmentType: string;
  monthlyIncome: number;
  status: string;
  createdAt: string;
}

export interface CreateLoanDto {
  bookingId: number;
  loanAmount: number;
  tenureYears: number;
  employmentType: string;
  monthlyIncome: number;
  bankName?: string;
}

@Injectable({
  providedIn: 'root'
})
export class LoanService {
  private apiUrl = `${environment.apiUrl}/Loan`;
  private adminApiUrl = `${environment.apiUrl}/admin/Loan`;

  constructor(private http: HttpClient) { }

  // Buyer
  applyForLoan(dto: CreateLoanDto): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/apply`, dto);
  }

  getMyLoans(): Observable<ApiResponse<LoanApplicationDto[]>> {
    return this.http.get<ApiResponse<LoanApplicationDto[]>>(`${this.apiUrl}/my-loans`);
  }

  // Admin
  getAllLoans(): Observable<ApiResponse<LoanApplicationDto[]>> {
    return this.http.get<ApiResponse<LoanApplicationDto[]>>(this.adminApiUrl);
  }

  updateLoanStatus(id: number, status: string): Observable<ApiResponse<any>> {
    return this.http.patch<ApiResponse<any>>(`${this.adminApiUrl}/${id}/status`, { status });
  }
}
