import { Routes } from '@angular/router';
import { roleGuard } from './core/auth/role.guard';

// Layouts
import { AdminLayoutComponent } from './features/admin/admin-layout/admin-layout.component';
import { OperatorLayoutComponent } from './features/operator/operator-layout/operator-layout.component';

// Auth Pages
import { LoginPageComponent } from './features/auth/pages/login-page.component';
import { SignupPageComponent } from './features/auth/pages/signup-page.component';
import { AdminLoginPageComponent } from './features/auth/pages/admin-login-page.component';
import { OperatorRegisterPageComponent } from './features/auth/pages/operator-register-page.component';

// Home
import { HomePageComponent } from './features/home/pages/home-page.component';

// User Pages
import { UserSearchPageComponent } from './features/user/pages/user-search-page.component';
import { UserBookingPageComponent } from './features/user/pages/user-booking-page.component';
import { UserBookingDetailsPageComponent } from './features/user/pages/user-booking-details-page.component';
import { UserPaymentPageComponent } from './features/user/pages/user-payment-page.component';
import { UserTicketsPageComponent } from './features/user/pages/user-tickets-page.component';

// Admin Pages
import { ApprovalRequestsPageComponent } from './features/admin/pages/approval-requests-page.component';
import { RoutesPageComponent } from './features/admin/pages/routes-page.component';
import { ManageOperatorsPageComponent } from './features/admin/pages/manage-operators-page.component';
import { AdminRevenuePageComponent } from './features/admin/pages/admin-revenue-page.component';

export const routes: Routes = [

  // 🌐 PUBLIC
  { path: '', component: HomePageComponent },

  // 🔐 AUTH
  { path: 'login', component: LoginPageComponent },
  { path: 'signup', component: SignupPageComponent },
  { path: 'admin/login', component: AdminLoginPageComponent },
  { path: 'operator/register', component: OperatorRegisterPageComponent },

  // 👤 USER FLOW
  { path: 'search-results', component: UserSearchPageComponent },

  {
    path: 'seat-selection/:scheduleId',
    component: UserBookingPageComponent,
    canActivate: [roleGuard('user', '/login')]
  },
  {
    path: 'booking',
    component: UserBookingDetailsPageComponent,
    canActivate: [roleGuard('user', '/login')]
  },
  {
    path: 'payment',
    component: UserPaymentPageComponent,
    canActivate: [roleGuard('user', '/login')]
  },
  {
    path: 'ticket',
    component: UserTicketsPageComponent,
    canActivate: [roleGuard('user', '/login')]
  },
  {
    path: 'ticket/:bookingId',
    component: UserTicketsPageComponent,
    canActivate: [roleGuard('user', '/login')]
  },

  // 🔁 USER REDIRECTS
  { path: 'user/search', redirectTo: 'search-results', pathMatch: 'full' },
  { path: 'user/book/:scheduleId', redirectTo: 'seat-selection/:scheduleId', pathMatch: 'full' },
  { path: 'user/tickets', redirectTo: 'ticket', pathMatch: 'full' },

  // 👑 ADMIN PANEL
  {
    path: 'admin',
    component: AdminLayoutComponent,
    canActivate: [roleGuard('admin', '/admin/login')],
    children: [
      { path: '', redirectTo: 'approvals', pathMatch: 'full' },
      { path: 'approvals', component: ApprovalRequestsPageComponent },
      { path: 'routes', component: RoutesPageComponent },
      { path: 'operators', component: ManageOperatorsPageComponent },
      { path: 'revenue', component: AdminRevenuePageComponent }
    ]
  },

  // 🚌 OPERATOR PANEL (NO DASHBOARD)
  {
    path: 'operator',
    component: OperatorLayoutComponent,
    canActivate: [roleGuard('operator', '/login')],
    children: [
      { path: '', redirectTo: 'buses', pathMatch: 'full' },
      {
        path: 'buses',
        loadComponent: () => import('./features/operator/pages/operator-buses-page.component').then(m => m.OperatorBusesPageComponent)
      },
      {
        path: 'offices',
        loadComponent: () => import('./features/operator/pages/operator-offices-page.component').then(m => m.OperatorOfficesPageComponent)
      },
      {
        path: 'schedules',
        loadComponent: () => import('./features/operator/pages/operator-schedules-page.component').then(m => m.OperatorSchedulesPageComponent)
      },
      {
        path: 'bookings',
        loadComponent: () => import('./features/operator/pages/operator-bookings-page.component').then(m => m.OperatorBookingsPageComponent)
      },
      {
        path: 'revenue',
        loadComponent: () => import('./features/operator/pages/operator-revenue-page.component').then(m => m.OperatorRevenuePageComponent)
      }
    ]
  },

  // 🔁 FALLBACK
  { path: '**', redirectTo: '' }
];
