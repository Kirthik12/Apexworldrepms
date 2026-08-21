import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  OnInit,
  ViewEncapsulation,
} from '@angular/core';
import {
  SystemService,
  BackupHistoryDto,
  BackupConfigurationDto,
  BackupStatusDto,
  ManualBackupRequestDto,
  RestorePreviewDto,
} from '../../../core/services/system.service';
import { AdminHeader } from '../../../shared/components/admin-header/admin-header';
import { ToastService } from '../../../core/services/toast.service';
import { NgIf, NgFor, NgClass, DatePipe, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PaginationComponent } from '../../../shared/components/pagination/pagination';

// ── Component ──────────────────────────────────────────────────────────────────

@Component({
  selector: 'app-admin-backup-management',
  templateUrl: './admin-backup-management.component.html',
  styleUrls: ['./admin-backup-management.component.css'],
  encapsulation: ViewEncapsulation.None,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [AdminHeader, NgIf, NgFor, NgClass, FormsModule, DatePipe, DecimalPipe, PaginationComponent],
})
export class AdminBackupManagementComponent implements OnInit {
  // ── Data ────────────────────────────────────────────────────────────────────
  backups: BackupHistoryDto[] = [];
  filteredBackups: BackupHistoryDto[] = [];
  isLoading = false;

  // ── Status KPIs ─────────────────────────────────────────────────────────────
  statusKpi: BackupStatusDto = {
    lastBackupDate: null,
    nextScheduledBackup: null,
    status: 'Success',
    usedStorageGB: 0,
    totalStorageGB: 50,
    percentageUsed: 0,
  };

  // ── Settings ─────────────────────────────────────────────────────────────────
  settings: BackupConfigurationDto = {
    frequency: 'Daily',
    backupType: 'Full',
    retentionDays: 30,
    backupTime: '02:00',
    storagePath: 'C:\\ApexWorldBackups',
    isEnabled: true,
  };
  isSavingSettings = false;

  // ── Filters ─────────────────────────────────────────────────────────────────
  filterStatus = '';
  filterType = '';

  // ── Pagination ───────────────────────────────────────────────────────────────
  currentPage = 1;
  itemsPerPage = 8;

  // ── Tabs ─────────────────────────────────────────────────────────────────────
  activeTab = 'tab-overview';

  // ── KPIs (derived from loaded history) ───────────────────────────────────────
  get totalCount(): number { return this.backups.length; }
  get successCount(): number { return this.backups.filter(b => b.status === 'Success').length; }
  get failedCount(): number { return this.backups.filter(b => b.status === 'Failed').length; }
  get fullBackupCount(): number { return this.backups.filter(b => b.backupType === 'Full' && b.status === 'Success').length; }
  get diffBackupCount(): number { return this.backups.filter(b => b.backupType === 'Differential' && b.status === 'Success').length; }
  get logBackupCount(): number { return this.backups.filter(b => b.backupType === 'Log' && b.status === 'Success').length; }
  get totalSizeStr(): string {
    const bytes = this.backups.filter(b => b.status === 'Success').reduce((acc, b) => acc + b.fileSize, 0);
    const gb = bytes / 1024 / 1024 / 1024;
    return gb >= 1 ? `${gb.toFixed(2)} GB` : `${(bytes / 1024 / 1024).toFixed(1)} MB`;
  }
  get lastBackupDate(): string {
    const last = this.backups.find(b => b.status === 'Success');
    return last ? new Date(last.createdAt).toLocaleDateString() : 'None';
  }
  get healthScore(): string {
    if (!this.backups.length) return '0%';
    return Math.round((this.successCount / this.backups.length) * 100) + '%';
  }
  get storagePercent(): number {
    return Math.min(100, Math.round(this.statusKpi.percentageUsed));
  }

  // ── Pagination computed ───────────────────────────────────────────────────────
  get paginatedBackups(): BackupHistoryDto[] {
    const start = (this.currentPage - 1) * this.itemsPerPage;
    return this.filteredBackups.slice(start, start + this.itemsPerPage);
  }
  get totalPages(): number[] {
    const total = Math.ceil(this.filteredBackups.length / this.itemsPerPage);
    return Array.from({ length: total }, (_, i) => i + 1);
  }

  // ── Modal: Manual Backup ─────────────────────────────────────────────────────
  showBackupModal = false;
  isSaving = false;
  newBackup: ManualBackupRequestDto = {
    backupName: '',
    backupType: 'Full',
    backupDestination: 'LocalStorage',
    backupDescription: '',
    includeData: 'DatabaseOnly',
  };

  // ── Modal: View Details ───────────────────────────────────────────────────────
  showViewModal = false;
  selectedBackup: BackupHistoryDto | null = null;

  // ── Modal: Delete Confirm ─────────────────────────────────────────────────────
  showDeleteModal = false;
  deletingId: number | null = null;
  isDeleting = false;

  // ── Modal: Restore Preview ────────────────────────────────────────────────────
  showRestorePreviewModal = false;
  isLoadingPreview = false;
  isRestoring = false;
  restorePreview: RestorePreviewDto | null = null;
  restoreTargetId: number | null = null;

  // ── Modal: Generic info ───────────────────────────────────────────────────────
  showModal = false;
  modalTitle = '';
  modalDesc = '';
  showModalConfirm = false;

  constructor(
    private systemService: SystemService,
    private cdr: ChangeDetectorRef,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.loadAll();
  }

  loadAll(): void {
    this.loadBackups();
    this.loadStatus();
    this.loadSettings();
  }

  // ── Load History (GET /api/v1/admin/Backup) ───────────────────────────────
  loadBackups(): void {
    this.isLoading = true;
    // markForCheck so the spinner renders immediately under OnPush
    this.cdr.markForCheck();
    this.systemService.getBackupHistory().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.backups = res.data;
          this.applyFilters();
        }
        this.isLoading = false;
        // Data arrived from async source — tell OnPush to re-evaluate
        this.cdr.markForCheck();
      },
      error: (err) => {
        console.error('Error loading backups', err);
        this.isLoading = false;
        this.toastService.warning('⚠️ Failed to load backup history.');
        this.cdr.markForCheck();
      },
    });
  }

  // ── Load Dashboard KPIs (GET /api/v1/admin/Backup/status) ────────────────
  loadStatus(): void {
    this.systemService.getBackupStatus().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.statusKpi = res.data;
          this.cdr.markForCheck();
        }
      },
      error: (err) => {
        console.error('Error loading status', err);
        this.cdr.markForCheck();
      },
    });
  }

  // ── Load Settings (GET /api/v1/admin/Backup/settings) ────────────────────
  loadSettings(): void {
    this.systemService.getBackupSettings().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.settings = res.data;
          this.cdr.markForCheck();
        }
      },
      error: (err) => {
        console.error('Error loading settings', err);
        this.cdr.markForCheck();
      },
    });
  }

  // ── Save Settings (PUT /api/v1/admin/Backup/settings) ────────────────────
  saveSettings(): void {
    if (!this.settings.storagePath || !this.settings.backupTime) {
      this.toastService.warning('⚠️ Storage path and backup time are required.');
      return;
    }
    this.isSavingSettings = true;
    this.cdr.markForCheck();
    this.systemService.saveBackupSettings(this.settings).subscribe({
      next: (res) => {
        if (res.success) {
          this.toastService.success('✅ Backup settings saved successfully.');
        }
        this.isSavingSettings = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        console.error('Error saving settings', err);
        this.toastService.warning('❌ Failed to save backup settings.');
        this.isSavingSettings = false;
        this.cdr.markForCheck();
      },
    });
  }

  // ── Filter ────────────────────────────────────────────────────────────────
  applyFilters(): void {
    this.filteredBackups = this.backups.filter(b => {
      const matchStatus = !this.filterStatus || b.status === this.filterStatus;
      const matchType = !this.filterType || b.backupType === this.filterType;
      return matchStatus && matchType;
    });
    this.currentPage = 1;
    // Filter is triggered by user events — detectChanges for instant table refresh
    this.cdr.detectChanges();
  }

  resetFilters(): void {
    this.filterStatus = '';
    this.filterType = '';
    this.applyFilters();
  }

  // ── Manual Backup Modal ───────────────────────────────────────────────────
  openBackupModal(): void {
    this.newBackup = {
      backupName: `REPMS_Manual_${new Date().toISOString().slice(0, 10).replace(/-/g, '')}`,
      backupType: 'Full',
      backupDestination: 'LocalStorage',
      backupDescription: '',
      includeData: 'DatabaseOnly',
    };
    this.showBackupModal = true;
    // detectChanges so the modal opens instantly without waiting for next CD cycle
    this.cdr.detectChanges();
  }

  closeBackupModal(): void {
    this.showBackupModal = false;
    this.isSaving = false;
    this.cdr.detectChanges();
  }

  initiateBackup(): void {
    if (!this.newBackup.backupName || !this.newBackup.backupType || !this.newBackup.includeData) {
      this.toastService.warning('⚠️ Backup Name, Type and Include Data are required.');
      return;
    }
    this.isSaving = true;
    // Immediately show "Running Backup..." state in the button
    this.cdr.markForCheck();
    this.systemService.createBackup(this.newBackup).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.backups = [res.data, ...this.backups];
          this.applyFilters();
          this.toastService.success('✅ Backup completed successfully.');
          this.loadStatus();
        }
        this.closeBackupModal();
        this.cdr.markForCheck();
      },
      error: (err) => {
        console.error('Error creating backup', err);
        this.toastService.warning('❌ Backup failed: ' + (err?.error?.message || 'Unknown error'));
        this.closeBackupModal();
        this.cdr.markForCheck();
      },
    });
  }

  // ── View Details Modal ────────────────────────────────────────────────────
  viewBackup(b: BackupHistoryDto): void {
    this.selectedBackup = b;
    this.showViewModal = true;
    this.cdr.detectChanges();
  }

  closeViewModal(): void {
    this.showViewModal = false;
    this.selectedBackup = null;
    this.cdr.detectChanges();
  }

  // ── Download Backup ───────────────────────────────────────────────────────
  downloadBackup(id: number): void {
    this.systemService.downloadBackup(id).subscribe({
      next: (blob) => {
        const backup = this.backups.find(b => b.id === id);
        const filename = backup ? backup.backupName : `Backup_${id}`;
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = filename;
        a.click();
        window.URL.revokeObjectURL(url);
        this.toastService.success('✅ Download started.');
        // No view state changed — no CD needed
      },
      error: (err) => {
        console.error('Error downloading backup', err);
        this.toastService.warning('❌ File not available for download.');
      },
    });
  }

  // ── Restore Flow: Preview ─────────────────────────────────────────────────
  restoreBackup(b: BackupHistoryDto): void {
    this.restoreTargetId = b.id;
    this.isLoadingPreview = true;
    this.restorePreview = null;
    this.showRestorePreviewModal = true;
    // Open modal immediately with loading spinner
    this.cdr.detectChanges();
    this.systemService.getRestorePreview(b.id).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.restorePreview = res.data;
        }
        this.isLoadingPreview = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        console.error('Error getting restore preview', err);
        this.toastService.warning('❌ Failed to load restore preview.');
        this.showRestorePreviewModal = false;
        this.isLoadingPreview = false;
        this.cdr.markForCheck();
      },
    });
  }

  closeRestorePreviewModal(): void {
    this.showRestorePreviewModal = false;
    this.restorePreview = null;
    this.restoreTargetId = null;
    this.cdr.detectChanges();
  }

  // ── Restore Flow: Execute ─────────────────────────────────────────────────
  confirmRestore(): void {
    if (this.restoreTargetId === null) return;
    this.isRestoring = true;
    this.cdr.markForCheck();
    this.systemService.executeRestore(this.restoreTargetId).subscribe({
      next: (res) => {
        if (res.success) {
          this.toastService.success('✅ Database restoration completed successfully!');
          this.loadAll();
        }
        this.closeRestorePreviewModal();
        this.isRestoring = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        console.error('Error executing restore', err);
        this.toastService.warning('❌ Restore failed: ' + (err?.error?.message || 'Unknown error'));
        this.closeRestorePreviewModal();
        this.isRestoring = false;
        this.cdr.markForCheck();
      },
    });
  }

  // ── Delete Confirm Modal ──────────────────────────────────────────────────
  promptDelete(id: number): void {
    this.deletingId = id;
    this.showDeleteModal = true;
    this.cdr.detectChanges();
  }

  closeDeleteModal(): void {
    this.deletingId = null;
    this.showDeleteModal = false;
    this.isDeleting = false;
    this.cdr.detectChanges();
  }

  confirmDelete(): void {
    if (this.deletingId === null) return;
    this.isDeleting = true;
    this.cdr.markForCheck();
    this.systemService.deleteBackup(this.deletingId).subscribe({
      next: (res) => {
        if (res.success) {
          this.backups = this.backups.filter(b => b.id !== this.deletingId);
          this.applyFilters();
          this.toastService.success('✅ Backup deleted successfully.');
          this.loadStatus();
        }
        this.closeDeleteModal();
        this.cdr.markForCheck();
      },
      error: (err) => {
        console.error('Error deleting backup', err);
        const msg = err?.error?.message || 'Cannot delete this backup.';
        this.toastService.warning('❌ ' + msg);
        this.closeDeleteModal();
        this.cdr.markForCheck();
      },
    });
  }

  // ── Retention Policy tab (quick save via settings) ────────────────────────
  saveRetentionPolicy(): void {
    this.saveSettings();
  }

  // ── Helpers ───────────────────────────────────────────────────────────────
  setTab(tab: string): void {
    this.activeTab = tab;
    this.cdr.detectChanges();
  }
  prevPage(): void { if (this.currentPage > 1) { this.currentPage--; this.cdr.markForCheck(); } }
  nextPage(): void { if (this.currentPage < this.totalPages.length) { this.currentPage++; this.cdr.markForCheck(); } }
  setPage(p: number): void { this.currentPage = p; this.cdr.markForCheck(); }
  onPageChange(page: number): void { this.currentPage = page; this.cdr.markForCheck(); }

  getStatusClass(status: string): string {
    if (status === 'Failed') return 'badge-failed';
    if (status === 'In Progress') return 'badge-progress';
    return 'badge-success';
  }

  getTypeClass(type: string): string {
    if (type === 'Differential') return 'badge-differential';
    if (type === 'Log') return 'badge-log';
    return 'badge-full';
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '—';
    const mb = bytes / 1024 / 1024;
    if (mb >= 1024) return (mb / 1024).toFixed(2) + ' GB';
    return mb.toFixed(1) + ' MB';
  }

  openModal(title: string, desc: string, confirmCb: any = null): void {
    this.modalTitle = title;
    this.modalDesc = desc;
    this.showModalConfirm = !!confirmCb;
    this.showModal = true;
    this.cdr.detectChanges();
  }
  closeModal(): void {
    this.showModal = false;
    this.cdr.detectChanges();
  }
  onConfirmModal(): void {
    this.showModal = false;
    this.cdr.detectChanges();
  }
  manageStorage(): void {
    this.toastService.info('ℹ️ Storage path is configured in Backup Settings tab.');
  }

  genericSave(action: string): void {
    if (action === 'settings' || action === 'policies') {
      this.saveSettings();
    }
  }
}
