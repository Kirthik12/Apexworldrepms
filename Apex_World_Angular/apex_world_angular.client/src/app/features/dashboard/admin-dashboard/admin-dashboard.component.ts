import { Component, AfterViewInit, ViewEncapsulation, OnInit, ChangeDetectorRef } from '@angular/core';
import { SystemService } from '../../../core/services/system.service';
import { LoanService } from '../../../core/services/loan.service';
import { EnquiryService } from '../../../core/services/enquiry.service';
import { DashboardService } from '../../../core/services/dashboard.service';
import { RouterLink } from '@angular/router';
import { NgIf, NgFor } from '@angular/common';
import { forkJoin } from 'rxjs';

declare var Chart: any;

@Component({
  selector: 'app-admin-dashboard',
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.css'],
  encapsulation: ViewEncapsulation.None,
  imports: [RouterLink, NgIf, NgFor],
})
export class AdminDashboardComponent implements OnInit, AfterViewInit {
  // ── KPI Cards ──────────────────────────────────────────────────────
  activeListings: number = 0;
  totalRevenue: string = '₹0';
  pendingLoans: number = 0;
  unresolvedEnquiries: number = 0;
  listingsTrend: string = '+0% this week';
  revenueTrend: string = '+0% this week';
  loansTrend: string = '0 new today';
  enquiriesTrend: string = '0 urgent';

  // ── Ledger Tables (populated by API) ───────────────────────────────
  recentBookings: any[] = [];
  recentPayments: any[] = [];
  isLoading: boolean = true;

  constructor(
    private systemService: SystemService,
    private loanService: LoanService,
    private enquiryService: EnquiryService,
    private dashboardService: DashboardService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.isLoading = true;
    
    forkJoin({
      summary: this.dashboardService.getSummary(),
      bookings: this.dashboardService.getActiveBookings(),
      payments: this.dashboardService.getRecentPayments()
    }).subscribe({
      next: (res) => {
        // Summary
        if (res.summary.success && res.summary.data) {
          this.activeListings = res.summary.data.activeListings || 0;
          const rev = res.summary.data.totalCompletedRevenue || 0;
          this.totalRevenue = '₹' + rev.toLocaleString('en-IN');
          this.pendingLoans = res.summary.data.pendingLoans || 0;
          this.unresolvedEnquiries = res.summary.data.unresolvedEnquiries || 0;
          if (res.summary.data.loansTrend) this.loansTrend = res.summary.data.loansTrend;
          if (res.summary.data.enquiriesTrend) this.enquiriesTrend = res.summary.data.enquiriesTrend;
        }
        
        // Bookings
        if (res.bookings.success && res.bookings.data) {
          this.recentBookings = res.bookings.data;
        }
        
        // Payments
        if (res.payments.success && res.payments.data) {
          this.recentPayments = res.payments.data;
        }
        
        this.isLoading = false;
        this.cdr.detectChanges();
        // Wait for Angular change detection to render the templates before initializing charts
        setTimeout(() => {
          this.loadChartDataAndInit();
          this.bindChartControls();
        }, 0);
      },
      error: (err) => {
        console.error('Error loading dashboard data', err);
        this.isLoading = false;
      }
    });
  }

  ngAfterViewInit(): void {
    // Moved to loadDashboardData success callback to ensure DOM is ready
  }

  private loadChartDataAndInit(): void {
    // Load property distribution
    this.dashboardService.getPropertyCategoryDistribution().subscribe(res => {
      if (res.success && res.data) {
        this.propertyDistData = {
          labels: res.data.labels || res.data.map((d: any) => d.categoryName),
          data: res.data.data || res.data.map((d: any) => d.count)
        };
      }
      this.initPropertyChart();
    });

    // Load booking status overview
    this.loadBookingStatusData(this.activeBookingTime);

    // Initial load for trend chart
    this.loadRevenueTrendData(this.activeTime);
  }

  private loadBookingStatusData(period: string): void {
    this.dashboardService.getBookingStatusOverview(period).subscribe(res => {
      if (res.success && res.data) {
        if (res.data.monthly) {
          this.bookingStatusData = res.data;
        } else {
          // Mapping fallback if it's a flat array
          this.bookingStatusData[period].datasets[0].data = res.data.map((d: any) => d.count || 0);
          if (res.data[0].status) {
            this.bookingStatusData[period].labels = res.data.map((d: any) => d.status);
          }
        }
      }
      
      if (!this.bookingChart) {
        this.initBookingChart();
      } else {
        this.bookingChart.data = this.bookingStatusData[period];
        this.bookingChart.update();
      }
    });
  }

  private loadRevenueTrendData(period: string): void {
    this.dashboardService.getRevenueTrend(period).subscribe(res => {
      if (res.success && res.data) {
        const labels = res.data.labels || [];
        const data = res.data.data || [];
        
        // Update the trendData structure
        this.trendData['revenue'][period] = { labels, data };
        this.activeTime = period;
        
        if (!this.trendChart) {
          this.initTrendChart();
        } else {
          this.updateTrendChart();
        }
      }
    });
  }

  // ── Chart state ────────────────────────────────────────────────────
  private trendChart: any = null;
  private propertyChart: any = null;
  private bookingChart: any = null;

  private activeKPI = 'revenue';
  private activeTime = 'monthly';
  private activeBookingTime = 'monthly';
  private activeChartType = 'line';
  private activeChartFill = false;

  private monthLabels = [
    'Jan',
    'Feb',
    'Mar',
    'Apr',
    'May',
    'Jun',
    'Jul',
    'Aug',
    'Sep',
    'Oct',
    'Nov',
    'Dec',
  ];
  private weekLabels = ['Week 1', 'Week 2', 'Week 3', 'Week 4'];
  private dayLabels = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];

  // All data starts at zero — will be populated by the API
  trendData: Record<string, any> = {
    revenue: {
      title: 'Revenue Performance Trend',
      label: 'System Revenue (₹)',
      color: '#10B981',
      bg: 'rgba(16,185,129,0.1)',
      daily: { labels: this.dayLabels, data: new Array(7).fill(0) },
      weekly: { labels: this.weekLabels, data: new Array(4).fill(0) },
      monthly: { labels: this.monthLabels, data: new Array(12).fill(0) },
    },
    listings: {
      title: 'Active Listings Growth',
      label: 'Total Listings',
      color: '#3B82F6',
      bg: 'rgba(59,130,246,0.1)',
      daily: { labels: this.dayLabels, data: new Array(7).fill(0) },
      weekly: { labels: this.weekLabels, data: new Array(4).fill(0) },
      monthly: { labels: this.monthLabels, data: new Array(12).fill(0) },
    },
    loans: {
      title: 'Pending Loan Applications Trend',
      label: 'Applications',
      color: '#F59E0B',
      bg: 'rgba(245,158,11,0.1)',
      daily: { labels: this.dayLabels, data: new Array(7).fill(0) },
      weekly: { labels: this.weekLabels, data: new Array(4).fill(0) },
      monthly: { labels: this.monthLabels, data: new Array(12).fill(0) },
    },
    enquiries: {
      title: 'Unresolved Enquiries Trend',
      label: 'Enquiries',
      color: '#EF4444',
      bg: 'rgba(239,68,68,0.1)',
      daily: { labels: this.dayLabels, data: new Array(7).fill(0) },
      weekly: { labels: this.weekLabels, data: new Array(4).fill(0) },
      monthly: { labels: this.monthLabels, data: new Array(12).fill(0) },
    },
  };

  bookingStatusData: Record<string, any> = {
    daily: {
      labels: ['Pending', 'Confirmed', 'Completed', 'Cancelled'],
      datasets: [
        {
          label: 'Bookings',
          data: [0, 0, 0, 0],
          backgroundColor: ['#FCD34D', '#FDBA74', '#6EE7B7', '#FCA5A5'],
          borderRadius: 4,
        },
      ],
    },
    weekly: {
      labels: ['Pending', 'Confirmed', 'Completed', 'Cancelled'],
      datasets: [
        {
          label: 'Bookings',
          data: [0, 0, 0, 0],
          backgroundColor: ['#FCD34D', '#FDBA74', '#6EE7B7', '#FCA5A5'],
          borderRadius: 4,
        },
      ],
    },
    monthly: {
      labels: ['Pending', 'Confirmed', 'Completed', 'Cancelled'],
      datasets: [
        {
          label: 'Bookings',
          data: [0, 0, 0, 0],
          backgroundColor: ['#FCD34D', '#FDBA74', '#6EE7B7', '#FCA5A5'],
          borderRadius: 4,
        },
      ],
    },
  };

  propertyDistData = {
    labels: ['Apartment', 'Villa', 'Commercial', 'Plot'],
    data: [0, 0, 0, 0],
  };

  // ── Chart initialisation ───────────────────────────────────────────
  private initTrendChart(): void {
    const ctx = document.getElementById('revenueChart') as HTMLCanvasElement;
    if (!ctx || !(window as any)['Chart']) return;
    const d = this.trendData[this.activeKPI];
    const td = d[this.activeTime];
    this.trendChart = new Chart(ctx, {
      type: this.activeChartType,
      data: {
        labels: td.labels,
        datasets: [
          {
            label: d.label,
            data: td.data,
            borderColor: d.color,
            backgroundColor: d.bg,
            borderWidth: 2,
            fill: this.activeChartFill,
            tension: 0.4,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false } },
        scales: {
          y: { beginAtZero: true, grid: { color: '#f1f5f9' } },
          x: { grid: { display: false } },
        },
      },
    });
  }

  private initPropertyChart(): void {
    const ctx = document.getElementById('propertyTypeChart') as HTMLCanvasElement;
    if (!ctx || !(window as any)['Chart']) return;
    this.propertyChart = new Chart(ctx, {
      type: 'doughnut',
      data: {
        labels: this.propertyDistData.labels,
        datasets: [
          {
            data: this.propertyDistData.data,
            backgroundColor: ['#3B82F6', '#10B981', '#F59E0B', '#6366F1'],
            borderWidth: 0,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { position: 'right' } },
        cutout: '70%',
      },
    });
  }

  private initBookingChart(): void {
    const ctx = document.getElementById('bookingStatusChart') as HTMLCanvasElement;
    if (!ctx || !(window as any)['Chart']) return;
    this.bookingChart = new Chart(ctx, {
      type: 'bar',
      data: this.bookingStatusData[this.activeBookingTime],
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false } },
        scales: { y: { beginAtZero: true }, x: { grid: { display: false } } },
      },
    });
  }

  private updateTrendChart(): void {
    if (!this.trendChart) return;
    const d = this.trendData[this.activeKPI];
    const td = d[this.activeTime];
    const titleEl = document.getElementById('trend-title');
    if (titleEl) titleEl.innerText = d.title;
    this.trendChart.config.type = this.activeChartType;
    this.trendChart.data.labels = td.labels;
    this.trendChart.data.datasets[0] = {
      label: d.label,
      data: td.data,
      borderColor: d.color,
      backgroundColor: this.activeChartType === 'bar' ? d.color : d.bg,
      borderWidth: 2,
      fill: this.activeChartFill,
      tension: 0.4,
    };
    this.trendChart.update();
  }

  // ── Event Binding ──────────────────────────────────────────────────
  private bindChartControls(): void {
    // Time filter (trend chart)
    document
      .querySelectorAll<HTMLElement>('.time-filter-btn:not(.booking-filter)')
      .forEach((btn) => {
        btn.addEventListener('click', () => {
          document
            .querySelectorAll('.time-filter-btn:not(.booking-filter)')
            .forEach((b) => b.classList.remove('active'));
          btn.classList.add('active');
          this.activeTime = btn.getAttribute('data-time') || 'monthly';
          this.loadRevenueTrendData(this.activeTime);
        });
      });

    // Chart type
    document.querySelectorAll<HTMLElement>('.chart-type-btn').forEach((btn) => {
      btn.addEventListener('click', () => {
        document.querySelectorAll<HTMLElement>('.chart-type-btn').forEach((b) => {
          b.classList.remove('active');
          b.style.cssText =
            'padding:6px 16px;border-radius:20px;border:1px solid #E2E8F0;background:#F8FAFC;color:#64748B;cursor:pointer;font-weight:600;font-size:0.85rem;';
        });
        btn.classList.add('active');
        btn.style.cssText =
          'padding:6px 16px;border-radius:20px;border:1px solid #F59E0B;background:#FEF3C7;color:#D97706;cursor:pointer;font-weight:600;font-size:0.85rem;';
        const type = btn.getAttribute('data-type');
        if (type === 'area') {
          this.activeChartType = 'line';
          this.activeChartFill = true;
        } else if (type === 'bar') {
          this.activeChartType = 'bar';
          this.activeChartFill = false;
        } else {
          this.activeChartType = 'line';
          this.activeChartFill = false;
        }
        this.updateTrendChart();
      });
    });

    // KPI card selection
    document.querySelectorAll<HTMLElement>('.kpi-card[data-kpi]').forEach((card) => {
      card.addEventListener('click', () => {
        this.activeKPI = card.getAttribute('data-kpi') || 'revenue';
        this.updateTrendChart();
      });
    });

    // Booking time filter
    document.querySelectorAll<HTMLElement>('.booking-filter').forEach((btn) => {
      btn.addEventListener('click', () => {
        document.querySelectorAll('.booking-filter').forEach((b) => b.classList.remove('active'));
        btn.classList.add('active');
        this.activeBookingTime = btn.getAttribute('data-time') || 'monthly';
        this.loadBookingStatusData(this.activeBookingTime);
      });
    });
  }
}
