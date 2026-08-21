import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from './property.service';

// ── Dashboard & Content DTOs ───────────────────────────────────────────────────

export interface DashboardMetricsDto {
  totalUsers: number;
  totalProperties: number;
  totalBookings: number;
  totalRevenue: number;
  recentActivities: string[];
}

export interface ContentDto {
  id: number;
  key: string;
  value: string;
  updatedAt: string;
  section?: string;
  contentType?: string;
  isActive?: boolean;
}

// ── Backup DTOs ────────────────────────────────────────────────────────────────

export interface BackupHistoryDto {
  id: number;
  backupName: string;
  backupType: string;       // Full, Differential, Log
  includeData: string;      // DatabaseOnly, FilesOnly, AllData
  filePath: string;
  fileSize: number;
  createdBy: string;
  status: string;           // Success, Failed
  errorMessage: string | null;
  parentBackupId: number | null;
  retentionUntil: string;
  checksum: string | null;
  createdAt: string;
}

export interface BackupConfigurationDto {
  id?: number;
  frequency: string;        // Daily, Weekly, Monthly
  backupType: string;       // Full, Differential, Log
  retentionDays: number;
  backupTime: string;       // e.g. "02:00"
  storagePath: string;
  isEnabled: boolean;
  createdBy?: string;
}

export interface BackupStatusDto {
  lastBackupDate: string | null;
  nextScheduledBackup: string | null;
  status: string;
  usedStorageGB: number;
  totalStorageGB: number;
  percentageUsed: number;
}

export interface ManualBackupRequestDto {
  backupName: string;
  backupType: string;       // Full, Differential, Log
  backupDestination: string;
  backupDescription: string;
  includeData: string;      // DatabaseOnly, FilesOnly, AllData
}

export interface RestorePreviewDto {
  isValid: boolean;
  validationMessage: string;
  estimatedRestoreTimeSeconds: number;
  requiredBackupChain: {
    id: number;
    backupName: string;
    backupType: string;
    includeData: string;
    createdAt: string;
    fileSize: number;
    fileExists: boolean;
  }[];
}

// ── Reports DTOs ────────────────────────────────────────────────────────────────

export interface ReportResponseDto {
  id: number;
  reportName: string;
  reportType: string;
  propertyScope: string;
  startDate: string;
  endDate: string;
  generatedOn: string;
  generatedBy: string;
  status: string;       // Scheduled | Processing | Completed | Failed
  format: string;       // PDF | Excel | CSV
  fileUrl: string | null;
}

export interface ReportRequestDto {
  reportName: string;
  reportType: string;
  startDate: string;
  endDate: string;
  format: string;               // PDF, Excel, CSV
  propertyType?: string;
  paymentMethod?: string;
  reportStatus?: string;
  sortBy?: string;
  sortOrder?: string;
  includeSummary?: boolean;
  includeCharts?: boolean;
  includeTables?: boolean;
  includeStatistics?: boolean;
  includeTransactionHistory?: boolean;
  includePaymentBreakdown?: boolean;
  includeBookingDetails?: boolean;
  buyerName?: string;
  propertyName?: string;
  bookingId?: string;
  transactionId?: string;
}

export interface ReportFilterDto {
  reportType?: string;
  status?: string;
  dateRange?: string;
  searchTerm?: string;
}

export interface ReportDashboardStatsDto {
  totalReports: number;
  bookingReports: number;
  paymentReports: number;
  loanReports: number;
  siteVisitReports: number;
  salesReports: number;
  enquiryReports: number;
  usersReports: number;
  propertiesReports: number;
  completedReports: number;
  scheduledReports: number;
  failedReports: number;
  trend?: {
    totalReportsTrendPercent: number;
    bookingTrendPercent: number;
    paymentTrendPercent: number;
    loanTrendPercent: number;
    siteVisitTrendPercent: number;
  };
}

export interface ReportChartDataDto {
  trendLabels: string[];
  generatedSeries: number[];
  downloadedSeries: number[];
  typeSplit: { [key: string]: number };
  statusSplit: { [key: string]: number };
}

// ── Legacy DTO kept for backwards compatibility ────────────────────────────────
export interface BackupDto {
  id: number;
  fileName: string;
  sizeMB: number;
  createdAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class SystemService {
  private apiUrl = environment.apiUrl;
  private backupBase = `${this.apiUrl}/admin/Backup`;
  private reportsBase = `${this.apiUrl}/Reports`; // Use correct version route matching backend controller

  constructor(private http: HttpClient) { }

  // ── Dashboard Metrics ──────────────────────────────────────────────────────
  getDashboardMetrics(): Observable<ApiResponse<DashboardMetricsDto>> {
    return this.http.get<ApiResponse<DashboardMetricsDto>>(`${this.apiUrl}/DashboardMetrics`);
  }

  // ── Reports Management ────────────────────────────────────────────────────
  getReports(filter?: ReportFilterDto): Observable<ReportResponseDto[]> {
    let params = new HttpParams();
    if (filter) {
      if (filter.reportType) params = params.set('reportType', filter.reportType);
      if (filter.status) params = params.set('status', filter.status);
      if (filter.dateRange) params = params.set('dateRange', filter.dateRange);
      if (filter.searchTerm) params = params.set('searchTerm', filter.searchTerm);
    }
    return this.http.get<ReportResponseDto[]>(this.reportsBase, { params });
  }

  generateReport(dto: ReportRequestDto): Observable<any> {
    return this.http.post<any>(this.reportsBase, dto);
  }

  getReportById(id: number): Observable<ReportResponseDto> {
    return this.http.get<ReportResponseDto>(`${this.reportsBase}/${id}`);
  }

  deleteReport(id: number): Observable<any> {
    return this.http.delete<any>(`${this.reportsBase}/${id}`);
  }

  getReportDashboardStats(): Observable<ReportDashboardStatsDto> {
    return this.http.get<ReportDashboardStatsDto>(`${this.reportsBase}/filter`);
  }

  getReportChartData(period: string): Observable<ReportChartDataDto> {
    return this.http.get<ReportChartDataDto>(`${this.reportsBase}/chart-data`, {
      params: new HttpParams().set('period', period)
    });
  }

  downloadReport(id: number): Observable<Blob> {
    return this.http.get(`${this.reportsBase}/${id}/download`, { responseType: 'blob' });
  }

  // ── Content Management ────────────────────────────────────────────────────
  getAllContents(): Observable<ApiResponse<ContentDto[]>> {
    return this.http.get<ApiResponse<ContentDto[]>>(`${this.apiUrl}/Contents`);
  }

  getPublicContents(): Observable<ApiResponse<ContentDto[]>> {
    return this.http.get<ApiResponse<ContentDto[]>>(`${this.apiUrl}/Contents/public`);
  }

  createContent(dto: any): Observable<ApiResponse<ContentDto>> {
    return this.http.post<ApiResponse<ContentDto>>(`${this.apiUrl}/Contents`, dto);
  }

  updateContent(id: number, dto: any): Observable<ApiResponse<ContentDto>> {
    return this.http.put<ApiResponse<ContentDto>>(`${this.apiUrl}/Contents/${id}`, dto);
  }

  deleteContent(id: number): Observable<ApiResponse<string>> {
    return this.http.delete<ApiResponse<string>>(`${this.apiUrl}/Contents/${id}`);
  }

  // ── Backup: History ───────────────────────────────────────────────────────
  getBackupHistory(): Observable<ApiResponse<BackupHistoryDto[]>> {
    return this.http.get<ApiResponse<BackupHistoryDto[]>>(this.backupBase);
  }

  getBackupById(id: number): Observable<ApiResponse<BackupHistoryDto>> {
    return this.http.get<ApiResponse<BackupHistoryDto>>(`${this.backupBase}/${id}`);
  }

  deleteBackup(id: number): Observable<ApiResponse<string>> {
    return this.http.delete<ApiResponse<string>>(`${this.backupBase}/${id}`);
  }

  downloadBackup(id: number): Observable<Blob> {
    return this.http.get(`${this.backupBase}/${id}/download`, { responseType: 'blob' });
  }

  // ── Backup: Manual Trigger ────────────────────────────────────────────────
  createBackup(dto: ManualBackupRequestDto): Observable<ApiResponse<BackupHistoryDto>> {
    return this.http.post<ApiResponse<BackupHistoryDto>>(this.backupBase, dto);
  }

  // ── Backup: Dashboard Status KPIs ────────────────────────────────────────
  getBackupStatus(): Observable<ApiResponse<BackupStatusDto>> {
    return this.http.get<ApiResponse<BackupStatusDto>>(`${this.backupBase}/status`);
  }

  // ── Backup: Settings ─────────────────────────────────────────────────────
  getBackupSettings(): Observable<ApiResponse<BackupConfigurationDto>> {
    return this.http.get<ApiResponse<BackupConfigurationDto>>(`${this.backupBase}/settings`);
  }

  saveBackupSettings(dto: BackupConfigurationDto): Observable<ApiResponse<BackupConfigurationDto>> {
    return this.http.put<ApiResponse<BackupConfigurationDto>>(`${this.backupBase}/settings`, dto);
  }

  // ── Backup: Restore ───────────────────────────────────────────────────────
  getRestorePreview(id: number): Observable<ApiResponse<RestorePreviewDto>> {
    return this.http.get<ApiResponse<RestorePreviewDto>>(`${this.backupBase}/${id}/restore-preview`);
  }

  executeRestore(id: number): Observable<ApiResponse<string>> {
    return this.http.post<ApiResponse<string>>(`${this.backupBase}/${id}/restore`, { confirmation: true });
  }

  // ── Legacy: kept for backwards compatibility ──────────────────────────────
  getBackups(): Observable<ApiResponse<BackupDto[]>> {
    return this.http.get<ApiResponse<BackupDto[]>>(`${this.apiUrl}/Backups`);
  }
}
