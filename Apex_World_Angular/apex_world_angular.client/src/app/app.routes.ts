import { Routes } from '@angular/router';
import { LandingComponent } from './features/landing/landing.component';
import { AuthGuard } from './core/guards/auth.guard';
import { AdminLayoutComponent } from './shared/layouts/admin-layout/admin-layout.component';
import { AdminDashboardComponent } from './features/dashboard/admin-dashboard/admin-dashboard.component';
import { AdminPropertyManagement } from './features/property-management/admin-property-management/admin-property-management';
import { AdminCustomerManagementComponent } from './features/user-management/admin-users/admin-customer-management/admin-customer-management';
import { AdminSiteVisitManagement } from './features/site-visits/admin-site-visits/admin-site-visit-management/admin-site-visit-management';
import { AdminBookingManagementComponent } from './features/bookings/admin-bookings/admin-booking-management/admin-booking-management.component';
import { AdminPaymentManagement } from './features/payment-management/admin-payments/admin-payment-management/admin-payment-management';
import { AdminLoanManagementComponent } from './features/loan-management/admin-loans/admin-loan-management/admin-loan-management.component';
import { AdminReviewManagementComponent } from './features/reviews/admin-reviews/admin-review-management/admin-review-management.component';
import { AdminReportManagementComponent } from './features/reports/admin-reports/admin-report-management/admin-report-management.component';
import { AdminContentManagementComponent } from './features/content-management/admin-content-management/admin-content-management.component';
import { AdminBackupManagementComponent } from './features/backup-recovery/admin-backup-management/admin-backup-management.component';
import { AdminNotification } from './features/notification-management/admin-notifications/admin-notification/admin-notification';
import { UserProfileComponent } from './features/user-profile/user-profile.component';
import { ResetPasswordComponent } from './features/user-profile/reset-password/reset-password.component';
import { BuyerLayoutComponent } from './shared/layouts/buyer-layout/buyer-layout.component';
import { BuyerDashboardComponent } from './features/dashboard/buyer-dashboard/buyer-dashboard.component';
import { BuyerPropertiesComponent } from './features/property-management/buyer-properties/buyer-properties';
import { BuyerPropertyDetailsComponent } from './features/property-management/buyer-property-details/buyer-property-details.component';
import { BuyerWishlistComponent } from './features/wishlist/buyer-wishlist/buyer-wishlist.component';
import { BuyerBookingsComponent } from './features/bookings/buyer-bookings/buyer-bookings.component';
import { BuyerLoansComponent } from './features/loan-management/buyer-loans/buyer-loans.component';
import { LoanApplicationComponent } from './features/loan-management/loan-application/loan-application.component';
import { BuyerReviewsComponent } from './features/reviews/buyer-reviews/buyer-reviews.component';
import { BuyerProfileComponent } from './features/user-profile/buyer-profile/buyer-profile.component';
import { BuyerSiteVisitsComponent } from './features/site-visits/buyer-site-visits/buyer-site-visits.component';
import { BuyerNotificationsComponent } from './features/notification-management/buyer-notifications/buyer-notifications.component';
import { BuyerHelpCenterComponent } from './features/help-center/buyer-help-center/buyer-help-center.component';
import { BuyerPaymentComponent } from './features/payment-management/buyer-payments/buyer-payments.component';
import { PaymentSuccessComponent } from './features/payment-management/buyer-payments/payment-success.component';

export const routes: Routes = [
  { path: '', component: LandingComponent },
  
  // Auth Routes
  { path: 'login', loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent) },
  { path: 'signup', loadComponent: () => import('./features/auth/signup/signup.component').then(m => m.SignupComponent) },
  { path: 'register', loadComponent: () => import('./features/auth/signup/signup.component').then(m => m.SignupComponent) },
  { path: 'forgot-password', loadComponent: () => import('./features/auth/forgot-password/forgot-password.component').then(m => m.ForgotPasswordComponent) },
  { path: 'change-password', loadComponent: () => import('./features/auth/change-password/change-password.component').then(m => m.ChangePasswordComponent) },
  { path: 'logout', loadComponent: () => import('./features/auth/logout/logout.component').then(m => m.LogoutComponent) },

  // Admin Dashboard Routes
  {
    path: 'admin-dashboard',
    component: AdminLayoutComponent,
    canActivate: [AuthGuard],
    data: { role: 'Admin' },
    children: [
      { path: '', component: AdminDashboardComponent },
      { path: 'admin-property-management', component: AdminPropertyManagement },
      { path: 'customer-management', component: AdminCustomerManagementComponent },
      { path: 'site-visit-management', component: AdminSiteVisitManagement },
      { path: 'booking-management', component: AdminBookingManagementComponent },
      { path: 'payment-management', component: AdminPaymentManagement },
      { path: 'loan-management', component: AdminLoanManagementComponent },
      { path: 'review-management', component: AdminReviewManagementComponent },
      { path: 'report-management', component: AdminReportManagementComponent },
      { path: 'content-management', component: AdminContentManagementComponent },
      { path: 'backup-management', component: AdminBackupManagementComponent },
      { path: 'help-center', loadComponent: () => import('./features/help-center/admin-help-center/admin-help-center.component').then(m => m.AdminHelpCenterComponent) },
      { path: 'notifications', component: AdminNotification },
      { path: 'user-profile', component: UserProfileComponent },
      { path: 'reset-password', component: ResetPasswordComponent },
    ],
  },

  // Buyer Dashboard Routes
  {
    path: 'buyer-dashboard',
    component: BuyerLayoutComponent,
    canActivate: [AuthGuard],
    data: { role: 'Buyer' },
    children: [
      { path: '', component: BuyerDashboardComponent },
      { path: 'properties', component: BuyerPropertiesComponent },
      { path: 'property-details', component: BuyerPropertyDetailsComponent },
      { path: 'wishlist', component: BuyerWishlistComponent },
      { path: 'bookings', component: BuyerBookingsComponent },
      { path: 'site-visits', component: BuyerSiteVisitsComponent },
      { path: 'loans', component: BuyerLoansComponent },
      { path: 'loan-application', component: LoanApplicationComponent },
      { path: 'reviews', component: BuyerReviewsComponent },
      { path: 'profile', component: BuyerProfileComponent },
      { path: 'notifications', component: BuyerNotificationsComponent },
      { path: 'help-center', component: BuyerHelpCenterComponent },
      { path: 'payment-management', component: BuyerPaymentComponent },
      { path: 'reset-password', component: ResetPasswordComponent }
    ],
  },

  // Public payment callback route — no AuthGuard so Razorpay redirect works in a new tab
  { path: 'buyer-dashboard/payment-success', component: PaymentSuccessComponent },
];

