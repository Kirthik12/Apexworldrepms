import { Component, OnInit, ViewEncapsulation, ChangeDetectorRef } from '@angular/core';
import { PropertyService } from '../../../core/services/property.service';
import { WishlistService } from '../../../core/services/wishlist.service';
import { PropertyDto } from '../../../core/models/property.model';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { NgIf, NgFor, DecimalPipe } from '@angular/common';
import { PaginationComponent } from '../../../shared/components/pagination/pagination';

@Component({
  selector: 'app-buyer-properties',
  templateUrl: './buyer-properties.html',
  styleUrls: ['./buyer-properties.css'],
  standalone: true,
  encapsulation: ViewEncapsulation.None,
  imports: [FormsModule, NgIf, NgFor, DecimalPipe, PaginationComponent],
})
export class BuyerPropertiesComponent implements OnInit {
  properties: PropertyDto[] = [];
  filteredProperties: PropertyDto[] = [];
  wishlistedIds = new Set<number>();

  // Stats
  totalProperties = 0;
  verifiedProperties = 0;

  // Search and Filter
  searchTerm = '';
  showAdvancedFilters = false;

  // Pagination
  currentPage = 1;
  pageSize = 12;
  totalItems = 0;
  Math = Math;

  constructor(
    private propertyService: PropertyService,
    private wishlistService: WishlistService,
    private router: Router,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit() {
    this.loadProperties();
    this.loadWishlist();
  }

  loadWishlist() {
    this.wishlistService.getWishlistedProperties().subscribe({
      next: (res) => {
        if (res.data) {
          const items = Array.isArray(res.data) ? res.data : (res.data as any).items || [];
          this.wishlistedIds = new Set(items.map((p: any) => p.id));
          this.cdr.detectChanges();
        }
      },
      error: (err) => console.error('Failed to load wishlist', err)
    });
  }

  toggleWishlist(propId: number, event: Event) {
    event.stopPropagation();
    if (this.wishlistedIds.has(propId)) {
      this.wishlistService.removeFromWishlist(propId).subscribe({
        next: () => {
          this.wishlistedIds.delete(propId);
          this.cdr.detectChanges();
        },
        error: (err) => console.error('Failed to remove from wishlist', err)
      });
    } else {
      this.wishlistService.addToWishlist(propId).subscribe({
        next: () => {
          this.wishlistedIds.add(propId);
          this.cdr.detectChanges();
        },
        error: (err) => console.error('Failed to add to wishlist', err)
      });
    }
  }

  loadProperties() {
    this.propertyService.getListedProperties(undefined, this.currentPage, this.pageSize).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.properties = res.data.items;
          this.filteredProperties = [...this.properties];
          this.totalItems = res.data.totalItems;
          this.totalProperties = this.totalItems;
          this.verifiedProperties = this.totalItems; // Assuming all listed are verified
          this.cdr.detectChanges();
        }
      },
      error: (err) => {
        console.error('Failed to load properties', err);
      },
    });
  }

  getPrimaryImage(prop: PropertyDto): string {
    if (prop.images && prop.images.length > 0) {
      const imgUrl = prop.images[0].imageUrl;
      // Handle absolute vs relative URLs if necessary
      if (imgUrl.startsWith('http') || imgUrl.startsWith('data:')) return imgUrl;
      return `/assets/images/${imgUrl.split('/').pop()}`; // Fallback to assets if it's just a filename
    }
    return '/assets/images/placeholder.jpg';
  }

  onSearchChange() {
    // Simple client-side search for demo purposes.
    // In a real app, this should call the backend search API.
    const term = this.searchTerm.toLowerCase();
    this.filteredProperties = this.properties.filter(
      (p) =>
        p.title.toLowerCase().includes(term) ||
        p.address?.toLowerCase().includes(term) ||
        p.category?.name.toLowerCase().includes(term),
    );
  }

  clearFilters() {
    this.searchTerm = '';
    this.filteredProperties = [...this.properties];
  }

  viewDetails(id: number) {
    this.router.navigate(['/buyer-dashboard/property-details'], { queryParams: { id: id } });
  }

  toggleAdvancedFilters() {
    this.showAdvancedFilters = !this.showAdvancedFilters;
  }

  closeFilters() {
    this.showAdvancedFilters = false;
  }

  changePage(delta: number) {
    const newPage = this.currentPage + delta;
    if (newPage > 0 && newPage <= Math.ceil(this.totalItems / this.pageSize)) {
      this.currentPage = newPage;
      this.loadProperties();
    }
  }

  onPageChange(page: number) {
    this.currentPage = page;
    this.loadProperties();
  }
}
