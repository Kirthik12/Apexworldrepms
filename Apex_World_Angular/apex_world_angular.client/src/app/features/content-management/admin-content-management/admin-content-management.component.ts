import { Component, OnInit, ViewEncapsulation, ChangeDetectorRef } from '@angular/core';
import { SystemService } from '../../../core/services/system.service';
import { AdminHeader } from '../../../shared/components/admin-header/admin-header';
import { ToastService } from '../../../core/services/toast.service';
import { FormsModule } from '@angular/forms';
import { NgFor, NgIf, NgClass, DatePipe } from '@angular/common';

// ── API DTOs ───────────────────────────────────────────────────────────────────

export interface ContentItem {
  id: number;
  section: string;
  key: string;
  value: string;
  contentType: 'text' | 'html' | 'image_url' | 'json';
  isActive: boolean;
  createdAt: string;
}

export type ContentItemDto = Omit<ContentItem, 'id' | 'createdAt'>;

// ── Component ──────────────────────────────────────────────────────────────────

@Component({
  selector: 'app-admin-content-management',
  templateUrl: './admin-content-management.component.html',
  styleUrls: ['./admin-content-management.component.css'],
  encapsulation: ViewEncapsulation.None,
  imports: [AdminHeader, FormsModule, NgFor, NgIf, NgClass, DatePipe],
})
export class AdminContentManagementComponent implements OnInit {
  // ── Data ────────────────────────────────────────────────────────────────────
  contentItems: ContentItem[] = [];
  filteredItems: ContentItem[] = [];

  // ── KPIs ────────────────────────────────────────────────────────────────────
  get totalItems(): number {
    return this.contentItems.length;
  }
  get activeItems(): number {
    return this.contentItems.filter((c) => c.isActive).length;
  }
  get totalSections(): number {
    return this.uniqueSections.length;
  }

  // ── Grouping ────────────────────────────────────────────────────────────────
  get uniqueSections(): string[] {
    return [...new Set(this.filteredItems.map((c) => c.section))];
  }

  getItemsBySection(section: string): ContentItem[] {
    return this.filteredItems.filter((c) => c.section === section);
  }

  // ── Filter state ────────────────────────────────────────────────────────────
  searchText: string = '';
  filterSection: string = '';
  filterType: string = '';

  get allSections(): string[] {
    return [...new Set(this.contentItems.map((c) => c.section))];
  }

  // ── Modal state ─────────────────────────────────────────────────────────────
  showModal: boolean = false;
  isEditing: boolean = false;
  isSaving: boolean = false;

  editForm: ContentItemDto = {
    section: '',
    key: '',
    value: '',
    contentType: 'text',
    isActive: true,
  };
  editingId: number | null = null;

  // ── Delete confirm ──────────────────────────────────────────────────────────
  showDeleteModal: boolean = false;
  deletingId: number | null = null;

  constructor(
    private systemService: SystemService,
    private cdr: ChangeDetectorRef,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.loadContent();
  }

  // ── Load ────────────────────────────────────────────────────────────────────

  loadContent(): void {
    this.systemService.getAllContents().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.contentItems = res.data.map((c) => ({
            id: c.id,
            section: c.section || 'General',
            key: c.key,
            value: c.value,
            contentType: (c.contentType as any) || 'text',
            isActive: c.isActive !== undefined ? c.isActive : true,
            createdAt: c.updatedAt,
          }));
          this.applyFilters();
          this.cdr.detectChanges();
        }
      },
      error: (err) => console.error('Error loading contents', err),
    });
  }

  filterContent(section?: string, key?: string): ContentItem[] {
    // Future: GET /api/Contents/filter?section=...&key=...
    return this.contentItems.filter(
      (c) =>
        (!section || c.section === section) &&
        (!key || c.key.toLowerCase().includes(key.toLowerCase())),
    );
  }

  applyFilters(): void {
    this.filteredItems = this.contentItems.filter((c) => {
      const matchSearch =
        !this.searchText ||
        c.key.toLowerCase().includes(this.searchText.toLowerCase()) ||
        c.value.toLowerCase().includes(this.searchText.toLowerCase()) ||
        c.section.toLowerCase().includes(this.searchText.toLowerCase());
      const matchSection = !this.filterSection || c.section === this.filterSection;
      const matchType = !this.filterType || c.contentType === this.filterType;
      return matchSearch && matchSection && matchType;
    });
    this.cdr.detectChanges();
  }

  // ── CRUD ────────────────────────────────────────────────────────────────────

  openCreateModal(): void {
    this.isEditing = false;
    this.editingId = null;
    this.editForm = { section: '', key: '', value: '', contentType: 'text', isActive: true };
    this.showModal = true;
  }

  openEditModal(item: ContentItem): void {
    this.isEditing = true;
    this.editingId = item.id;
    this.editForm = {
      section: item.section,
      key: item.key,
      value: item.value,
      contentType: item.contentType,
      isActive: item.isActive,
    };
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.isSaving = false;
  }

  saveContent(): void {
    if (!this.editForm.section || !this.editForm.key || !this.editForm.value) {
      this.toastService.warning('⚠️ Section, Key, and Value are required.');
      return;
    }

    this.isSaving = true;

    if (this.isEditing && this.editingId !== null) {
      this.systemService.updateContent(this.editingId, this.editForm).subscribe({
        next: (res) => {
          if (res.success) {
            this.toastService.success('✅ Content updated successfully.');
            this.loadContent();
          } else {
            this.toastService.warning('⚠️ Failed to update content.');
          }
          this.closeModal();
        },
        error: (err) => {
          console.error('Error updating content', err);
          this.toastService.warning('⚠️ Failed to update content.');
          this.closeModal();
        }
      });
    } else {
      this.systemService.createContent(this.editForm).subscribe({
        next: (res) => {
          if (res.success) {
            this.toastService.success('✅ Content item created successfully.');
            this.loadContent();
          } else {
            this.toastService.warning('⚠️ Failed to create content.');
          }
          this.closeModal();
        },
        error: (err) => {
          console.error('Error creating content', err);
          this.toastService.warning('⚠️ Failed to create content.');
          this.closeModal();
        }
      });
    }
  }

  // ── Toggle Active ────────────────────────────────────────────────────────────

  toggleActive(item: ContentItem): void {
    const updatedStatus = !item.isActive;
    const dto = {
      section: item.section,
      key: item.key,
      value: item.value,
      contentType: item.contentType,
      isActive: updatedStatus
    };
    this.systemService.updateContent(item.id, dto).subscribe({
      next: (res) => {
        if (res.success) {
          item.isActive = updatedStatus;
          this.toastService.success(`${item.isActive ? '✅ Activated' : '⚪ Deactivated'}: "${item.key}"`);
          this.cdr.detectChanges();
        } else {
          this.toastService.warning('⚠️ Failed to update content status.');
        }
      },
      error: (err) => {
        console.error('Error toggling active status', err);
        this.toastService.warning('⚠️ Failed to update content status.');
      }
    });
  }

  // ── Delete ───────────────────────────────────────────────────────────────────

  promptDelete(id: number): void {
    this.deletingId = id;
    this.showDeleteModal = true;
  }

  closeDeleteModal(): void {
    this.deletingId = null;
    this.showDeleteModal = false;
  }

  confirmDelete(): void {
    if (this.deletingId !== null) {
      this.systemService.deleteContent(this.deletingId).subscribe({
        next: (res) => {
          if (res.success) {
            this.contentItems = this.contentItems.filter((c) => c.id !== this.deletingId);
            this.applyFilters();
            this.toastService.success('✅ Content deleted successfully.');
          } else {
            this.toastService.warning('⚠️ Failed to delete content.');
          }
          this.closeDeleteModal();
        },
        error: (err) => {
          console.error('Error deleting content', err);
          this.toastService.warning('⚠️ Failed to delete content.');
          this.closeDeleteModal();
        }
      });
    } else {
      this.closeDeleteModal();
    }
  }

  // ── Helpers ──────────────────────────────────────────────────────────────────

  getContentTypeLabel(type: string): string {
    const map: Record<string, string> = {
      text: 'Text',
      html: 'HTML',
      image_url: 'Image URL',
      json: 'JSON',
    };
    return map[type] || type;
  }

  getContentTypeBadgeClass(type: string): string {
    const map: Record<string, string> = {
      text: 'badge-text',
      html: 'badge-html',
      image_url: 'badge-image',
      json: 'badge-json',
    };
    return map[type] || '';
  }

}
