import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { 
  BuyerNotificationListDto, 
  AdminNotificationListDto, 
  BroadcastNotificationDto 
} from '../models/notification.model';
import { ApiResponse } from './property.service';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private buyerApiUrl = `${environment.apiUrl}/buyer/notifications`;
  private adminApiUrl = `${environment.apiUrl}/admin/notifications`;

  private unreadCountSubject = new BehaviorSubject<number>(0);
  unreadCount$ = this.unreadCountSubject.asObservable();

  constructor(private http: HttpClient) { }

  updateUnreadCount(count: number) {
    this.unreadCountSubject.next(count);
  }

  // --- Buyer Methods ---

  getBuyerNotifications(pageNumber: number = 1, pageSize: number = 20): Observable<ApiResponse<BuyerNotificationListDto>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());
    return this.http.get<ApiResponse<BuyerNotificationListDto>>(this.buyerApiUrl, { params }).pipe(
      tap(res => {
        if (res && res.data) {
          this.updateUnreadCount(res.data.unreadCount);
        }
      })
    );
  }

  markBuyerNotificationAsRead(id: number): Observable<ApiResponse<any>> {
    return this.http.patch<ApiResponse<any>>(`${this.buyerApiUrl}/${id}/read`, {});
  }

  markAllBuyerNotificationsAsRead(): Observable<ApiResponse<any>> {
    return this.http.patch<ApiResponse<any>>(`${this.buyerApiUrl}/read-all`, {});
  }

  // --- Admin Methods ---

  getAdminNotifications(pageNumber: number = 1, pageSize: number = 20): Observable<ApiResponse<AdminNotificationListDto>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());
    return this.http.get<ApiResponse<AdminNotificationListDto>>(this.adminApiUrl, { params }).pipe(
      tap(res => {
        if (res && res.data) {
          this.updateUnreadCount(res.data.unreadCount);
        }
      })
    );
  }

  markAdminNotificationAsRead(id: number): Observable<ApiResponse<any>> {
    return this.http.patch<ApiResponse<any>>(`${this.adminApiUrl}/${id}/read`, {});
  }

  markAllAdminNotificationsAsRead(): Observable<ApiResponse<any>> {
    return this.http.patch<ApiResponse<any>>(`${this.adminApiUrl}/read-all`, {});
  }

  broadcastNotification(dto: BroadcastNotificationDto): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.adminApiUrl}/broadcast`, dto);
  }
}
