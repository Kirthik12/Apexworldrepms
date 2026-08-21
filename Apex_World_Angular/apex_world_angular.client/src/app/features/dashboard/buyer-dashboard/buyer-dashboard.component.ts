import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { PropertyService } from '../../../core/services/property.service';
import { PropertyDto } from '../../../core/models/property.model';
import { environment } from '../../../../environments/environment';
import { RouterLink } from '@angular/router';
import { NgIf, NgFor } from '@angular/common';

@Component({
  selector: 'app-buyer-dashboard',
  templateUrl: './buyer-dashboard.component.html',
  styleUrls: ['./buyer-dashboard.component.css'],
  imports: [RouterLink, NgIf, NgFor],
})
export class BuyerDashboardComponent implements OnInit {
  recommendedProperties: PropertyDto[] = [];
  recentlyViewed: PropertyDto[] = [];

  backendUrl = environment.apiUrl.replace('/api/v1', '');

  constructor(
    private propertyService: PropertyService,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    console.log('Buyer Dashboard Initialized.');
    this.loadRecommended();
    this.loadRecentlyViewed();
  }

  loadRecommended(): void {
    // Load 4 available properties
    this.propertyService.getListedProperties(undefined, 1, 4).subscribe({
      next: (res) => {
        if (res && res.data) {
          this.recommendedProperties = res.data.items;
          this.cdr.detectChanges();
        }
      },
      error: (err) => console.error('Failed to load recommended properties', err),
    });
  }

  loadRecentlyViewed(): void {
    try {
      const recentIds = JSON.parse(localStorage.getItem('recently_viewed_properties') || '[]');

      // Keep only up to 8
      if (recentIds.length > 8) {
        recentIds.splice(8);
        localStorage.setItem('recently_viewed_properties', JSON.stringify(recentIds));
      }

      this.recentlyViewed = recentIds;
    } catch (e) {
      console.error('Error loading recently viewed properties', e);
    }
  }

  getImageUrl(images: any[]): string {
    if (images && images.length > 0 && images[0].imageUrl) {
      const url = images[0].imageUrl;
      return url.startsWith('http') ? url : `${this.backendUrl}${url}`;
    }
    return '../../../assets/images/no_image_icon.png';
  }

  scrollRecentlyViewed(direction: number): void {
    const container = document.getElementById('recently-viewed-track');
    const firstCard = container?.querySelector('.property-card') as HTMLElement;
    if (container && firstCard) {
      const step = firstCard.offsetWidth + 24;
      container.scrollBy({ left: step * 3 * direction, behavior: 'smooth' });
    }
  }

  dismissTopBanner(): void {
    const banner = document.getElementById('top-offers-banner');
    if (banner) {
      banner.style.opacity = '0';
      banner.style.transform = 'translateY(-100%)';
      setTimeout(() => {
        banner.style.display = 'none';
        document.body.style.paddingTop = '70px';
      }, 300);
    }
  }
}
