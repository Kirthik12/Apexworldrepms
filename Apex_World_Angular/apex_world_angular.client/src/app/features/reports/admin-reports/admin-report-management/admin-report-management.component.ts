import { Component, OnInit, OnDestroy, ViewEncapsulation, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import {
  SystemService,
  ReportResponseDto,
  ReportRequestDto,
  ReportFilterDto,
  ReportDashboardStatsDto,
  ReportChartDataDto
} from '../../../../core/services/system.service';
import { AdminHeader } from '../../../../shared/components/admin-header/admin-header';
import { FormsModule } from '@angular/forms';
import { NgIf, NgFor, NgClass, DatePipe, DecimalPipe } from '@angular/common';
import { ToastService } from '../../../../core/services/toast.service';
import { PaginationComponent } from '../../../../shared/components/pagination/pagination';

declare var Chart: any;

@Component({
  selector: 'app-admin-report-management',
  templateUrl: './admin-report-management.component.html',
  styleUrls: ['./admin-report-management.component.css'],
  encapsulation: ViewEncapsulation.None,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [AdminHeader, FormsModule, NgIf, NgFor, NgClass, DatePipe, DecimalPipe, PaginationComponent],
})
export class AdminReportManagementComponent implements OnInit, OnDestroy {
  // ── Reports List ─────────────────────────────────────────────────────────
  reports: ReportResponseDto[] = [];
  filteredReports: ReportResponseDto[] = [];
  isLoading = false;

  // ── KPI Dashboard Stats ──────────────────────────────────────────────────
  stats: ReportDashboardStatsDto = {
    totalReports: 0,
    bookingReports: 0,
    paymentReports: 0,
    loanReports: 0,
    siteVisitReports: 0,
    salesReports: 0,
    enquiryReports: 0,
    usersReports: 0,
    propertiesReports: 0,
    completedReports: 0,
    scheduledReports: 0,
    failedReports: 0,
    trend: {
      totalReportsTrendPercent: 0,
      bookingTrendPercent: 0,
      paymentTrendPercent: 0,
      loanTrendPercent: 0,
      siteVisitTrendPercent: 0
    }
  };

  // ── Active Filters ───────────────────────────────────────────────────────
  filterType = 'All';
  filterStatus = 'All';
  filterDateRange = 'All';
  searchTerm = '';

  // ── Selected KPI Card Filter ─────────────────────────────────────────────
  selectedKpiType = 'All';

  // ── Pagination ───────────────────────────────────────────────────────────
  currentPage = 1;
  itemsPerPage = 5;

  // ── Generate Report Modal state ──────────────────────────────────────────
  showGenerateModal = false;
  isGenerating = false;
  newReport: ReportRequestDto = {
    reportName: '',
    reportType: '',
    startDate: '',
    endDate: '',
    format: '', // PDF, Excel, CSV
    propertyType: 'All',
    paymentMethod: 'All',
    reportStatus: 'All',
    sortBy: 'Date',
    sortOrder: 'Descending',
    includeSummary: true,
    includeCharts: true,
    includeTables: true,
    includeStatistics: false,
    includeTransactionHistory: false,
    includePaymentBreakdown: false,
    includeBookingDetails: false,
    buyerName: '',
    propertyName: '',
    bookingId: '',
    transactionId: ''
  };

  // ── Chart Instances ──────────────────────────────────────────────────────
  private overviewChartInstance: any = null;
  private typeChartInstance: any = null;
  private statusChartInstance: any = null;
  activeChartPeriod = 'daily';
  activeChartType = 'line';

  constructor(
    private systemService: SystemService,
    private cdr: ChangeDetectorRef,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.loadAll();
  }

  ngOnDestroy(): void {
    this.destroyCharts();
  }

  loadAll(): void {
    this.loadReports();
    this.loadKPIStats();
    this.loadChartData(this.activeChartPeriod);
  }

  // ── Load Reports (GET /api/v1/Reports) ───────────────────────────────────
  loadReports(): void {
    this.isLoading = true;
    this.cdr.markForCheck();

    const filters: ReportFilterDto = {
      reportType: this.filterType === 'All' ? undefined : this.filterType,
      status: this.filterStatus === 'All' ? undefined : this.filterStatus,
      dateRange: this.filterDateRange === 'All' ? undefined : this.filterDateRange,
      searchTerm: this.searchTerm ? this.searchTerm : undefined
    };

    this.systemService.getReports(filters).subscribe({
      next: (res) => {
        this.reports = res || [];
        this.applyClientFilters();
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        console.error('Error loading reports', err);
        this.isLoading = false;
        this.cdr.markForCheck();
        this.toastService.warning('⚠️ Failed to load reports.');
      }
    });
  }

  // ── Load Dashboard KPIs (GET /api/v1/Reports/filter) ─────────────────────
  loadKPIStats(): void {
    this.systemService.getReportDashboardStats().subscribe({
      next: (res) => {
        if (res) {
          this.stats = res;
          this.cdr.markForCheck();
        }
      },
      error: (err) => console.error('Error loading KPI statistics', err)
    });
  }

  // ── Load Chart Analytics Data ────────────────────────────────────────────
  loadChartData(period: string): void {
    this.activeChartPeriod = period;
    this.systemService.getReportChartData(period).subscribe({
      next: (res) => {
        if (res) {
          this.renderOverviewChart(res.trendLabels, res.generatedSeries, res.downloadedSeries);
          this.renderDonutCharts(res.typeSplit, res.statusSplit);
        }
      },
      error: (err) => console.error('Error loading chart data', err)
    });
  }

  // ── Apply Interactive KPI Filter & Client-side Filters ───────────────────
  selectKpiFilter(type: string): void {
    // Normalise matching values
    if (type === 'Booking Report') this.selectedKpiType = 'Booking';
    else if (type === 'Payment Report') this.selectedKpiType = 'Payment';
    else if (type === 'Loan Report') this.selectedKpiType = 'Loan';
    else if (type === 'Site-Visit Report') this.selectedKpiType = 'Site-Visit';
    else this.selectedKpiType = type;

    this.applyClientFilters();
  }

  applyClientFilters(): void {
    this.filteredReports = this.reports.filter(r => {
      if (this.selectedKpiType !== 'All') {
        return r.reportType.toLowerCase() === this.selectedKpiType.toLowerCase();
      }
      return true;
    });
    this.currentPage = 1;
    this.cdr.detectChanges();
  }

  resetFilters(): void {
    this.filterType = 'All';
    this.filterStatus = 'All';
    this.filterDateRange = 'All';
    this.searchTerm = '';
    this.selectedKpiType = 'All';
    this.loadReports();
  }

  setAllTime(): void {
    this.filterDateRange = 'All';
    this.loadReports();
  }

  quickGenerateReport(): void {
    const reportReq: any = {
      reportName: `QuickReport_${new Date().toISOString().slice(0, 10).replace(/-/g, '')}_${new Date().getTime()}`,
      reportType: this.filterType === 'All' ? 'Sales' : this.filterType,
      format: 'PDF',
      propertyType: 'All',
      paymentMethod: 'All',
      reportStatus: this.filterStatus === 'All' ? 'Completed' : this.filterStatus,
      sortBy: 'Date',
      sortOrder: 'Descending',
      includeSummary: true,
      includeCharts: true,
      includeTables: true,
      includeStatistics: false,
      includeTransactionHistory: false,
      includePaymentBreakdown: false,
      includeBookingDetails: false
    };
    
    if (this.filterDateRange !== 'All' && this.filterDateRange !== 'Future') {
      const days = parseInt(this.filterDateRange);
      if (!isNaN(days)) {
        const end = new Date();
        const start = new Date();
        start.setDate(end.getDate() - days);
        reportReq.startDate = start.toISOString().split('T')[0];
        reportReq.endDate = end.toISOString().split('T')[0];
      }
    }
    
    this.isGenerating = true;
    this.cdr.markForCheck();

    this.systemService.generateReport(reportReq).subscribe({
      next: (res) => {
        if (res.success) {
          this.toastService.success('✅ Quick Report generated successfully.');
          this.loadAll();
        }
        this.isGenerating = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        console.error('Error generating quick report', err);
        this.toastService.error('❌ Generation failed: ' + (err?.error?.message || 'Unknown error'));
        this.isGenerating = false;
        this.cdr.markForCheck();
      }
    });
  }

  // ── Pagination Computed Properties ───────────────────────────────────────
  get paginatedReports(): ReportResponseDto[] {
    const start = (this.currentPage - 1) * this.itemsPerPage;
    return this.filteredReports.slice(start, start + this.itemsPerPage);
  }

  get totalPages(): number[] {
    const total = Math.ceil(this.filteredReports.length / this.itemsPerPage);
    return Array.from({ length: total }, (_, i) => i + 1);
  }

  prevPage(): void { if (this.currentPage > 1) { this.currentPage--; this.cdr.markForCheck(); } }
  nextPage(): void { if (this.currentPage < this.totalPages.length) { this.currentPage++; this.cdr.markForCheck(); } }
  setPage(p: number): void { this.currentPage = p; this.cdr.markForCheck(); }
  onPageChange(page: number): void { this.currentPage = page; this.cdr.markForCheck(); }

  // ── Open / Close Generate Modal ──────────────────────────────────────────
  openGenerateModal(): void {
    this.newReport = {
      reportName: `REPMS_Report_${new Date().toISOString().slice(0, 10).replace(/-/g, '')}`,
      reportType: '',
      startDate: '',
      endDate: '',
      format: '',
      propertyType: 'All',
      paymentMethod: 'All',
      reportStatus: 'All',
      sortBy: 'Date',
      sortOrder: 'Descending',
      includeSummary: true,
      includeCharts: true,
      includeTables: true,
      includeStatistics: false,
      includeTransactionHistory: false,
      includePaymentBreakdown: false,
      includeBookingDetails: false,
      buyerName: '',
      propertyName: '',
      bookingId: '',
      transactionId: ''
    };
    this.showGenerateModal = true;
    this.cdr.detectChanges();
  }

  closeGenerateModal(): void {
    this.showGenerateModal = false;
    this.isGenerating = false;
    this.cdr.detectChanges();
  }

  // ── Generate Report (POST /api/v1/Reports) ───────────────────────────────
  generateReport(): void {
    if (!this.newReport.reportName || !this.newReport.reportType || !this.newReport.format) {
      this.toastService.warning('⚠️ Report Name, Type and Format are required.');
      return;
    }
    if (this.newReport.startDate && this.newReport.endDate) {
      if (new Date(this.newReport.endDate) < new Date(this.newReport.startDate)) {
        this.toastService.warning('⚠️ End date cannot be earlier than start date.');
        return;
      }
    }

    this.isGenerating = true;
    this.cdr.markForCheck();

    const reqToSubmit: any = { ...this.newReport };
    if (!reqToSubmit.startDate) delete reqToSubmit.startDate;
    if (!reqToSubmit.endDate) delete reqToSubmit.endDate;

    this.systemService.generateReport(reqToSubmit).subscribe({
      next: (res) => {
        if (res.success) {
          this.toastService.success('✅ Report generated successfully.');
          this.loadAll();
        }
        this.closeGenerateModal();
      },
      error: (err) => {
        console.error('Error generating report', err);
        this.toastService.error('❌ Generation failed: ' + (err?.error?.message || 'Unknown error'));
        this.closeGenerateModal();
      }
    });
  }

  // ── Download Report (GET /api/v1/Reports/{id}/download) ──────────────────
  downloadReport(id: number, format: string): void {
    this.toastService.info('⏳ Download starting...');
    this.systemService.downloadReport(id).subscribe({
      next: (blob) => {
        const report = this.reports.find(r => r.id === id);
        const filename = report ? report.reportName : `Report_${id}`;
        const ext = format.toLowerCase() === 'excel' ? 'xlsx' : format.toLowerCase();
        
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `${filename.replace(/\s+/g, '_')}.${ext}`;
        a.click();
        window.URL.revokeObjectURL(url);
        this.toastService.success('✅ Download completed.');
      },
      error: (err) => {
        console.error('Error downloading report', err);
        this.toastService.error('❌ Report file download failed.');
      }
    });
  }

  // ── Delete Report (DELETE /api/v1/Reports/{id}) ──────────────────────────
  deleteReport(id: number): void {
    if (!confirm('Are you sure you want to permanently delete this report record?')) {
      return;
    }

    this.systemService.deleteReport(id).subscribe({
      next: () => {
        this.toastService.success('✅ Report record deleted successfully.');
        this.loadAll();
      },
      error: (err) => {
        console.error('Error deleting report', err);
        this.toastService.error('❌ Failed to delete report record.');
      }
    });
  }

  exportFilteredCSV(): void {
    if (this.filteredReports.length === 0) {
      this.toastService.warning('ℹ️ No reports available to export.');
      return;
    }
    
    const headers = ['Report Name', 'Report Type', 'Property Scope', 'Date Range', 'Generated On', 'Generated By', 'Status', 'Format'];
    const rows = this.filteredReports.map(r => [
      r.reportName,
      r.reportType,
      r.propertyScope,
      (r.startDate || r.endDate)
        ? `${r.startDate ? String(r.startDate).split('T')[0] : ''} - ${r.endDate ? String(r.endDate).split('T')[0] : ''}`
        : 'All Time',
      r.generatedOn,
      r.generatedBy,
      r.status,
      r.format
    ]);
    
    const csvContent = [
      headers.join(','),
      ...rows.map(row => row.map(v => `"${(v || '').toString().replace(/"/g, '""')}"`).join(','))
    ].join('\n');
    
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.setAttribute('download', 'reports_list_export.csv');
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
    this.toastService.success('✅ Exported reports list to CSV.');
  }

  // ── Chart.js Overview Trend Render ───────────────────────────────────────
  private renderOverviewChart(labels: string[], generated: number[], downloaded: number[]): void {
    const ctx = document.getElementById('overviewChart') as any;
    if (!ctx) return;

    if (this.overviewChartInstance) {
      this.overviewChartInstance.destroy();
    }

    const type = this.activeChartType;
    const datasets = [
      {
        label: 'Reports Generated',
        data: generated,
        borderColor: '#2563EB',
        backgroundColor: type === 'area' ? 'rgba(37,99,235,0.1)' : '#2563EB',
        fill: type === 'area',
        tension: 0.3
      },
      {
        label: 'Reports Downloaded',
        data: downloaded,
        borderColor: '#10B981',
        backgroundColor: type === 'area' ? 'rgba(16,185,129,0.1)' : '#10B981',
        fill: type === 'area',
        tension: 0.3
      }
    ];

    this.overviewChartInstance = new Chart(ctx, {
      type: type === 'area' ? 'line' : type,
      data: { labels, datasets },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { display: false }
        },
        scales: {
          y: { beginAtZero: true }
        }
      }
    });
  }

  // ── Chart.js Donut Split Renders ─────────────────────────────────────────
  private renderDonutCharts(typeSplit: { [key: string]: number }, statusSplit: { [key: string]: number }): void {
    const typeCtx = document.getElementById('typeChart') as any;
    const statusCtx = document.getElementById('statusChart') as any;

    if (typeCtx) {
      if (this.typeChartInstance) this.typeChartInstance.destroy();
      this.typeChartInstance = new Chart(typeCtx, {
        type: 'doughnut',
        data: {
          labels: Object.keys(typeSplit),
          datasets: [{
            data: Object.values(typeSplit),
            backgroundColor: ['#3B82F6', '#10B981', '#F59E0B', '#8B5CF6', '#EF4444', '#EC4899', '#64748B']
          }]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          plugins: { legend: { position: 'bottom' } }
        }
      });
    }

    if (statusCtx) {
      if (this.statusChartInstance) this.statusChartInstance.destroy();
      this.statusChartInstance = new Chart(statusCtx, {
        type: 'doughnut',
        data: {
          labels: Object.keys(statusSplit),
          datasets: [{
            data: Object.values(statusSplit),
            backgroundColor: ['#10B981', '#F59E0B', '#3B82F6', '#EF4444']
          }]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          plugins: { legend: { position: 'bottom' } }
        }
      });
    }
  }

  setChartType(type: string): void {
    this.activeChartType = type;
    this.loadChartData(this.activeChartPeriod);
  }

  private destroyCharts(): void {
    if (this.overviewChartInstance) this.overviewChartInstance.destroy();
    if (this.typeChartInstance) this.typeChartInstance.destroy();
    if (this.statusChartInstance) this.statusChartInstance.destroy();
  }

  // ── UI Helper Badges ─────────────────────────────────────────────────────
  getStatusClass(status?: string): string {
    const s = status || 'Completed';
    if (s === 'Failed') return 'badge-failed';
    if (s === 'Scheduled') return 'badge-progress';
    if (s === 'Processing') return 'badge-processing';
    return 'badge-success';
  }
}
