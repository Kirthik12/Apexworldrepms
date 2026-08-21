import { Component, Input, Output, EventEmitter, ViewEncapsulation } from '@angular/core';
import { NgFor, NgIf, NgClass } from '@angular/common';

@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [NgFor, NgIf],
  templateUrl: './pagination.html',
  styleUrls: ['./pagination.css'],
  encapsulation: ViewEncapsulation.None,
})
export class PaginationComponent {
  @Input() currentPage: number = 1;
  @Input() totalItems: number = 0;
  @Input() pageSize: number = 10;
  @Input() disabled: boolean = false;

  @Output() pageChange = new EventEmitter<number>();

  get totalPages(): number {
    return Math.ceil(this.totalItems / this.pageSize) || 1;
  }

  get pages(): (number | string)[] {
    const total = this.totalPages;
    const current = this.currentPage;
    const pages: (number | string)[] = [];

    if (total <= 7) {
      for (let i = 1; i <= total; i++) {
        pages.push(i);
      }
    } else {
      pages.push(1);

      const start = Math.max(2, current - 1);
      const end = Math.min(total - 1, current + 1);

      if (start > 2) {
        pages.push('...');
      }

      for (let i = start; i <= end; i++) {
        pages.push(i);
      }

      if (end < total - 1) {
        pages.push('...');
      }

      pages.push(total);
    }

    return pages;
  }

  onPageChange(page: number | string): void {
    if (this.disabled || typeof page === 'string' || page === this.currentPage) return;
    if (page < 1 || page > this.totalPages) return;
    this.pageChange.emit(page);
  }

  get showSummaryStart(): number {
    if (this.totalItems === 0) return 0;
    return (this.currentPage - 1) * this.pageSize + 1;
  }

  get showSummaryEnd(): number {
    return Math.min(this.currentPage * this.pageSize, this.totalItems);
  }
}
