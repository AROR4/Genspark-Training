import { Routes } from '@angular/router';
import { roleGuard } from './core/auth/role.guard';
import { AdminDashboardPageComponent } from './features/admin/pages/admin-dashboard-page.component';
import { AdminLoginPageComponent } from './features/auth/pages/admin-login-page.component';
import { HomePageComponent } from './features/home/pages/home-page.component';
import { LoginPageComponent } from './features/auth/pages/login-page.component';
import { OperatorRegisterPageComponent } from './features/auth/pages/operator-register-page.component';
import { SignupPageComponent } from './features/auth/pages/signup-page.component';
import { OperatorDashboardPageComponent } from './features/operator/pages/operator-dashboard-page.component';
import { UserBookingPageComponent } from './features/user/pages/user-booking-page.component';
import { UserSearchPageComponent } from './features/user/pages/user-search-page.component';
import { UserTicketsPageComponent } from './features/user/pages/user-tickets-page.component';

export const routes: Routes = [
  { path: '', component: HomePageComponent },
  { path: 'login', component: LoginPageComponent },
  { path: 'signup', component: SignupPageComponent },
  { path: 'operator/register', component: OperatorRegisterPageComponent },
  { path: 'admin/login', component: AdminLoginPageComponent },
  { path: 'user/search', component: UserSearchPageComponent, canActivate: [roleGuard('user', '/login')] },
  { path: 'user/book/:scheduleId', component: UserBookingPageComponent, canActivate: [roleGuard('user', '/login')] },
  { path: 'user/tickets', component: UserTicketsPageComponent, canActivate: [roleGuard('user', '/login')] },
  { path: 'user/tickets/:bookingId', component: UserTicketsPageComponent, canActivate: [roleGuard('user', '/login')] },
  { path: 'operator/dashboard', component: OperatorDashboardPageComponent, canActivate: [roleGuard('operator', '/login')] },
  { path: 'admin/dashboard', component: AdminDashboardPageComponent, canActivate: [roleGuard('admin', '/admin/login')] },
  { path: '**', redirectTo: 'login' }
];
