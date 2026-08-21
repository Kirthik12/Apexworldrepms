import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PropertyDto, PropertyCreateDto, PropertyUpdateDto, PropertyStatusUpdateDto } from '../models/property.model';

export interface PaginatedResponse<T> {
  totalItems: number;
  pageNumber: number;
  pageSize: number;
  items: T[];
}

export interface ApiResponse<T> {
  success: boolean;
  message?: string;
  data: T;
  errors?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class PropertyService {
  private apiUrl = `${environment.apiUrl}/Property`;
  private adminApiUrl = `${environment.apiUrl}/admin/AdminProperty`;

  constructor(private http: HttpClient) { }

  // --- Public / Buyer Methods ---

  getListedProperties(category?: string, pageNumber: number = 1, pageSize: number = 10): Observable<ApiResponse<PaginatedResponse<PropertyDto>>> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (category) {
      params = params.set('category', category);
    }

    return this.http.get<ApiResponse<PaginatedResponse<PropertyDto>>>(this.apiUrl, { params });
  }

  getPropertyById(id: number): Observable<ApiResponse<PropertyDto>> {
    return this.http.get<ApiResponse<PropertyDto>>(`${this.apiUrl}/${id}`);
  }

  searchProperties(status: string = 'Approved', name?: string, id?: number, pageNumber: number = 1, pageSize: number = 10): Observable<ApiResponse<PaginatedResponse<PropertyDto>>> {
    let params = new HttpParams()
      .set('status', status)
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (name) params = params.set('name', name);
    if (id) params = params.set('id', id.toString());

    return this.http.get<ApiResponse<PaginatedResponse<PropertyDto>>>(`${this.apiUrl}/search`, { params });
  }

  // --- Admin Methods ---

  getAllPropertiesAdmin(pageNumber: number = 1, pageSize: number = 10): Observable<ApiResponse<PaginatedResponse<PropertyDto>>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<ApiResponse<PaginatedResponse<PropertyDto>>>(this.adminApiUrl, { params });
  }

  getAdminPropertyById(id: number): Observable<ApiResponse<PropertyDto>> {
    return this.http.get<ApiResponse<PropertyDto>>(`${this.adminApiUrl}/${id}`);
  }

  addProperty(formData: FormData): Observable<ApiResponse<PropertyDto>> {
    return this.http.post<ApiResponse<PropertyDto>>(this.adminApiUrl, formData);
  }

  updateProperty(id: number, data: PropertyUpdateDto): Observable<ApiResponse<PropertyDto>> {
    return this.http.put<ApiResponse<PropertyDto>>(`${this.adminApiUrl}/${id}`, data);
  }

  updatePropertyStatus(id: number, data: PropertyStatusUpdateDto): Observable<ApiResponse<PropertyDto>> {
    return this.http.patch<ApiResponse<PropertyDto>>(`${this.adminApiUrl}/${id}/status`, data);
  }

  deleteProperty(id: number): Observable<ApiResponse<string>> {
    return this.http.delete<ApiResponse<string>>(`${this.adminApiUrl}/${id}`);
  }
}
