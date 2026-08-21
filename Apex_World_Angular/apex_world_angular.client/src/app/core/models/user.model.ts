export interface User {
  id: number;
  username: string;
  email: string;
  fullName: string;
  phoneNumber: string;
  city: string;
  resetToken?: string;
  resetTokenExpiry?: Date | string;
  isActive: boolean;
}

export interface PagedResponse<T> {
  items: T[];
  totalCount: number;
}