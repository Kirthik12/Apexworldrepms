export interface BookingRequestDto {
  propertyId: number;
  buyerId?: number;
  scheduledDate?: string | null;
  firstName?: string | null;
  lastName?: string | null;
  email?: string | null;
  phoneNumber?: string | null;
  permanentAddress?: string | null;
}

export interface BookingDto {
  id: number;
  propertyId: number;
  propertyName?: string | null;
  buyerId?: number;
  buyerName?: string | null;
  buyerEmail?: string | null;
  bookingDate: string;
  scheduledDate?: string | null;
  status: string;
  paymentStatus: string;
  property?: any;
  paymentMethod?: string | null;
}
