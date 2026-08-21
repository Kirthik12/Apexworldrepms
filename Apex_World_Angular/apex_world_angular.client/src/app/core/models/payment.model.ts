export interface PaymentInitiateRequestDto {
  bookingId: number;
  propertyId?: number;
  buyerId?: number;
  paymentMethod?: string | null;
  paymentDetails?: string | null;
  buyerName?: string | null;
  phoneNumber?: string | null;
}

export interface GenericPaymentPayload {
  bookingId?: string | null;
  amount: number;
  paymentStatus?: string | null;
}
