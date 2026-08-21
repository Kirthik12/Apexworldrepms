import { Component, OnInit, ViewEncapsulation, ChangeDetectorRef } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { WishlistService } from '../../../core/services/wishlist.service';
import { PropertyDto } from '../../../core/models/property.model';
import { environment } from '../../../../environments/environment';
import { NgIf, NgFor, DecimalPipe } from '@angular/common';

@Component({
  selector: 'app-buyer-wishlist',
  templateUrl: './buyer-wishlist.component.html',
  styleUrls: ['./buyer-wishlist.component.css'],
  encapsulation: ViewEncapsulation.None,
  imports: [RouterLink, NgIf, NgFor, DecimalPipe],
})
export class BuyerWishlistComponent implements OnInit {
  wishlistProperties: PropertyDto[] = [];
  filteredProperties: PropertyDto[] = [];
  selectedIds = new Set<number>();

  backendUrl = environment.apiUrl.replace('/api/v1', '');

  pendingRemoveId: number | null = null;
  pendingRemoveTitle: string = '';

  constructor(
    private router: Router,
    private wishlistService: WishlistService,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.loadWishlist();
  }

  loadWishlist(): void {
    this.wishlistService.getWishlistedProperties().subscribe({
      next: (res) => {
        if (res.data) {
          // Depending on API structure, it could be an array or paged response
          this.wishlistProperties = Array.isArray(res.data)
            ? res.data
            : (res.data as any).items || [];
          this.filteredProperties = [...this.wishlistProperties];
          this.cdr.detectChanges();
        }
      },
      error: (err) => {
        console.error('Failed to load wishlist', err);
      },
    });
  }

  get totalValue(): number {
    return this.wishlistProperties.reduce((acc, prop) => acc + (prop.price || 0), 0);
  }

  get averageValue(): number {
    if (this.wishlistProperties.length === 0) return 0;
    return this.totalValue / this.wishlistProperties.length;
  }

  getImageUrl(images: any[]): string {
    if (images && images.length > 0 && images[0].imageUrl) {
      const url = images[0].imageUrl;
      return url.startsWith('http') ? url : `${this.backendUrl}${url}`;
    }
    return '/assets/images/prop_park_villa.png';
  }

  onSearch(event: Event): void {
    const q = (event.target as HTMLInputElement).value.toLowerCase().trim();
    if (!q) {
      this.filteredProperties = [...this.wishlistProperties];
      return;
    }
    this.filteredProperties = this.wishlistProperties.filter(
      (p) =>
        (p.title && p.title.toLowerCase().includes(q)) ||
        (p.address && p.address.toLowerCase().includes(q)),
    );
  }

  toggleSelection(id: number, event: Event): void {
    const checked = (event.target as HTMLInputElement).checked;
    if (checked) {
      this.selectedIds.add(id);
    } else {
      this.selectedIds.delete(id);
    }
  }

  promptRemove(id: number, title: string): void {
    this.pendingRemoveId = id;
    this.pendingRemoveTitle = title;
  }

  cancelRemove(): void {
    this.pendingRemoveId = null;
    this.pendingRemoveTitle = '';
  }

  confirmRemove(): void {
    if (this.pendingRemoveId) {
      this.wishlistService.removeFromWishlist(this.pendingRemoveId).subscribe({
        next: () => {
          this.wishlistProperties = this.wishlistProperties.filter(
            (p) => p.id !== this.pendingRemoveId,
          );
          this.filteredProperties = this.filteredProperties.filter(
            (p) => p.id !== this.pendingRemoveId,
          );
          this.selectedIds.delete(this.pendingRemoveId!);
          this.cancelRemove();
          this.cdr.detectChanges();
        },
        error: (err) => console.error('Failed to remove from wishlist', err),
      });
    }
  }

  deleteSelected(): void {
    const ids = Array.from(this.selectedIds);
    if (ids.length === 0) return;
    this.wishlistService.bulkRemoveFromWishlist(ids).subscribe({
      next: () => {
        this.wishlistProperties = this.wishlistProperties.filter((p) => !this.selectedIds.has(p.id));
        this.filteredProperties = this.filteredProperties.filter((p) => !this.selectedIds.has(p.id));
        this.selectedIds.clear();
        this.cdr.detectChanges();
      },
      error: (err) => console.error('Failed to bulk remove from wishlist', err)
    });
  }

  clearAll(): void {
    if (confirm('Clear your entire wishlist?')) {
      const ids = this.wishlistProperties.map((p) => p.id);
      if (ids.length === 0) return;
      this.wishlistService.bulkRemoveFromWishlist(ids).subscribe({
        next: () => {
          this.wishlistProperties = [];
          this.filteredProperties = [];
          this.selectedIds.clear();
          this.cdr.detectChanges();
        },
        error: (err) => console.error('Failed to clear wishlist', err)
      });
    }
  }

  viewDetails(id: number): void {
    this.router.navigate(['/buyer-dashboard/property-details'], { queryParams: { id } });
  }

  shareWishlist(): void {
    const url =
      window.location.href.split('?')[0] + '?shared=' + Math.random().toString(36).substring(2, 10);
    navigator.clipboard
      .writeText(url)
      .then(() => alert('Wishlist Link Copied!\n\n' + url))
      .catch(() => alert('Copy manually:\n\n' + url));
  }

  dismissBanner(): void {
    const banner = document.getElementById('wl-top-offers-banner');
    if (banner) {
      banner.style.display = 'none';
    }
  }
}
