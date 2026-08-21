import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { PropertyService } from '../../../core/services/property.service';
import { WishlistService } from '../../../core/services/wishlist.service';
import { PropertyDto } from '../../../core/models/property.model';
import { environment } from '../../../../environments/environment';
import { NgIf, DecimalPipe } from '@angular/common';
import { AiCompanionService } from '../../../core/services/ai-companion.service';

@Component({
  selector: 'app-buyer-property-details',
  templateUrl: './buyer-property-details.component.html',
  styleUrls: ['./buyer-property-details.component.css'],
  imports: [RouterLink, NgIf, DecimalPipe],
})
export class BuyerPropertyDetailsComponent implements OnInit {
  property: PropertyDto | null = null;
  images: string[] = [];
  currentImageIndex = 0;
  isWishlisted = false;

  backendUrl = environment.apiUrl.replace('/api/v1', '');

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private propertyService: PropertyService,
    private wishlistService: WishlistService,
    private cdr: ChangeDetectorRef,
    private aiCompanionService: AiCompanionService
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe((params) => {
      const propId = params['id'];
      if (!propId) {
        alert('Property ID not found.');
        this.router.navigate(['/buyer-dashboard/properties']);
        return;
      }

      this.loadPropertyDetails(parseInt(propId, 10));
    });
  }

  loadPropertyDetails(id: number): void {
    this.propertyService.getPropertyById(id).subscribe({
      next: (res) => {
        if (res.data) {
          this.property = res.data;
          this.setupImages();
          this.updateRecentlyViewed();
          this.checkWishlistStatus(id);
          this.cdr.detectChanges();
        }
      },
      error: (err) => {
        alert('Property not found.');
        this.router.navigate(['/buyer-dashboard/properties']);
      },
    });
  }

  checkWishlistStatus(id: number): void {
    this.wishlistService.getWishlistedProperties().subscribe({
      next: (res) => {
        if (res.data) {
          const props = Array.isArray(res.data) ? res.data : (res.data as any).items || [];
          this.isWishlisted = props.some((p: any) => p.id === id);
        }
      },
      error: (err) => console.error('Failed to fetch wishlist status', err),
    });
  }

  setupImages(): void {
    if (!this.property) return;

    if (this.property.images && this.property.images.length > 0) {
      this.images = this.property.images.map((img) =>
        img.imageUrl.startsWith('http') ? img.imageUrl : `${this.backendUrl}${img.imageUrl}`,
      );
    } else {
      this.images = ['../../../assets/images/no_image_icon.png'];
    }
  }

  updateRecentlyViewed(): void {
    if (!this.property) return;

    try {
      let recent = JSON.parse(localStorage.getItem('recently_viewed_properties') || '[]');
      // Remove if exists
      recent = recent.filter((p: any) => p.id !== this.property?.id);

      // Add to front
      recent.unshift({
        id: this.property.id,
        title: this.property.title,
        address: this.property.address,
        price: this.property.price,
        images: this.property.images,
        category: this.property.category,
      });

      // Keep only up to 8
      if (recent.length > 8) {
        recent = recent.slice(0, 8);
      }

      localStorage.setItem('recently_viewed_properties', JSON.stringify(recent));
    } catch (e) {
      console.error('Error saving recently viewed', e);
    }
  }

  // Helper getters for the template
  get isPlot(): boolean {
    const cat = this.property?.category?.name || '';
    return cat.toLowerCase().includes('plot') || cat.toLowerCase().includes('land');
  }

  get taxBaseValue(): number {
    return this.property ? this.property.price : 0;
  }

  get gstRate(): number {
    return this.isPlot ? 0.11 : 0.12;
  }

  get gstValue(): number {
    return this.taxBaseValue * this.gstRate;
  }

  get regValue(): number {
    return this.taxBaseValue * 0.03;
  }

  get totalValue(): number {
    return this.taxBaseValue + this.gstValue + this.regValue;
  }

  nextImage(): void {
    if (this.images.length === 0) return;
    this.currentImageIndex = (this.currentImageIndex + 1) % this.images.length;
  }

  prevImage(): void {
    if (this.images.length === 0) return;
    this.currentImageIndex = (this.currentImageIndex - 1 + this.images.length) % this.images.length;
  }

  toggleWishlist(): void {
    if (!this.property) return;

    if (this.isWishlisted) {
      this.wishlistService.removeFromWishlist(this.property.id).subscribe({
        next: () => (this.isWishlisted = false),
        error: (err) => console.error('Error removing from wishlist', err),
      });
    } else {
      this.wishlistService.addToWishlist(this.property.id).subscribe({
        next: () => (this.isWishlisted = true),
        error: (err) => console.error('Error adding to wishlist', err),
      });
    }
  }

  goBack(): void {
    this.router.navigate(['/buyer-dashboard/properties']);
  }

  toggleAiDrawer() {
    this.aiCompanionService.toggle(this.property?.id || null);
  }
}
