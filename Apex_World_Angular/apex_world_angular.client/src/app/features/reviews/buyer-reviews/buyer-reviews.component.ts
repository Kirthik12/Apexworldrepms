import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ReviewService } from '../../../core/services/review.service';
import { BookingService } from '../../../core/services/booking.service';
import { BookingDto } from '../../../core/models/booking.model';
import {
  CreatePlatformReviewDto,
  CreatePropertyReviewDto,
} from '../../../core/models/review.model';
import { ToastService } from '../../../core/services/toast.service';
import { NgFor, NgIf } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-buyer-reviews',
  templateUrl: './buyer-reviews.component.html',
  styleUrls: ['./buyer-reviews.component.css'],
  encapsulation: ViewEncapsulation.None,
  imports: [NgFor, FormsModule, NgIf, RouterLink],
})
export class BuyerReviewsComponent implements OnInit {
  activeTab: 'platform-review' | 'property-review' = 'platform-review';

  // Platform Review State
  pfRating: number = 0;
  pfComment: string = '';
  pfChips: string[] = [
    'UI / Design',
    'Fast Performance',
    'Property Details',
    'Customer Support',
    'Easy Booking',
  ];
  selectedChips: Set<string> = new Set();

  // Properties Review State
  bookings: BookingDto[] = [];
  selectedBooking: BookingDto | null = null;
  prRating: number = 0;
  prComment: string = '';
  isReviewModalOpen = false;

  constructor(
    private reviewService: ReviewService,
    private bookingService: BookingService,
    private router: Router,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.loadBookings();
  }

  setTab(tab: 'platform-review' | 'property-review') {
    this.activeTab = tab;
  }

  // --- Platform Review ---

  setPfRating(val: number) {
    this.pfRating = val;
  }

  getPfStars(): number[] {
    return [1, 2, 3, 4, 5];
  }

  toggleChip(chip: string) {
    if (this.selectedChips.has(chip)) {
      this.selectedChips.delete(chip);
    } else {
      this.selectedChips.add(chip);
    }
  }

  submitPlatformReview() {
    if (this.pfRating === 0) {
      alert('⚠️ Please select a star rating first.');
      return;
    }

    // Append chips to the comment if any are selected
    let finalComment = this.pfComment;
    if (this.selectedChips.size > 0) {
      const tags = Array.from(this.selectedChips).join(', ');
      finalComment = finalComment ? `[Loved: ${tags}] ${finalComment}` : `[Loved: ${tags}]`;
    }

    const dto: CreatePlatformReviewDto = {
      rating: this.pfRating,
      tags: Array.from(this.selectedChips),
      comment: finalComment,
    };

    this.reviewService.submitPlatformReview(dto).subscribe({
      next: () => {
        this.toastService.success('Review submitted successfully! Thank you.');
        this.pfRating = 0;
        this.pfComment = '';
        this.selectedChips.clear();
      },
      error: (err) => {
        alert('Failed to submit review.');
        console.error(err);
      },
    });
  }

  // --- Property Review ---

  loadBookings() {
    this.bookingService.getBuyerBookings().subscribe((res) => {
      if (res && res.data) {
        // Filter out Cancelled/Failed bookings
        this.bookings = res.data.filter((b) => b.status !== 'Cancelled' && b.status !== 'Failed');
      }
    });
  }

  openReviewModal(booking: BookingDto) {
    this.selectedBooking = booking;
    this.prRating = 0;
    this.prComment = '';
    this.isReviewModalOpen = true;
  }

  closeReviewModal() {
    this.isReviewModalOpen = false;
    this.selectedBooking = null;
  }

  setPrRating(val: number) {
    this.prRating = val;
  }

  submitPropertyReview() {
    if (this.prRating === 0) {
      alert('⚠️ Please select a star rating for this property.');
      return;
    }
    if (!this.selectedBooking) return;

    const dto: CreatePropertyReviewDto = {
      bookingId: this.selectedBooking.id,
      rating: this.prRating,
      comment: this.prComment,
    };

    this.reviewService.submitPropertyReview(dto).subscribe({
      next: () => {
        this.toastService.success('Review submitted successfully! Thank you.');
        this.closeReviewModal();
      },
      error: (err) => {
        alert(err.error?.message || 'Failed to submit property review.');
        console.error(err);
      },
    });
  }

  // --- Utils ---

  getPropImg(booking: BookingDto): string {
    const images = (booking as any)?.property?.images;
    if (images && images.length > 0 && images[0].imageUrl) {
      return images[0].imageUrl;
    }
    return '/assets/images/logo.png';
  }
}
