export interface ReviewDto {
  id: number; buyerId: number;
  buyerName: string;
  reviewType: string;
  propertyId?: number;
  propertyName?: string;
  rating: number;
  tags?: string[];
  photos?: string[];
  comment: string;
  status: string;
  adminResponse?: string;
  responseDate?: string;
  createdAt: string;
}

export interface CreatePlatformReviewDto {
  rating: number;
  tags?: string[];
  comment: string;
}

export interface CreatePropertyReviewDto {
  bookingId: number;
  rating: number;
  photos?: string[];
  comment: string;
}
