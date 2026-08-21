export interface BaseNotificationDto {
  id: number;
  title: string;
  message: string;
  category: string;
  actionText?: string | null;
  actionUrl?: string | null;
  relatedEntityType?: string | null;
  relatedEntityId?: number | null;
  isRead: boolean;
  createdAt: string;
  readAt?: string | null;
}

export interface BuyerNotificationDto extends BaseNotificationDto {}
export interface AdminNotificationDto extends BaseNotificationDto {
  adminId: number;
}

export interface BaseNotificationListDto<T> {
  totalItems: number;
  unreadCount: number;
  pageNumber: number;
  pageSize: number;
  items: T[];
}

export interface BuyerNotificationListDto extends BaseNotificationListDto<BuyerNotificationDto> {}
export interface AdminNotificationListDto extends BaseNotificationListDto<AdminNotificationDto> {}

export interface BroadcastNotificationDto {
  title: string;
  message: string;
  category: string;
  targetAudience: string;
  targetRole?: string | null;
  targetUserId?: number | null;
}
