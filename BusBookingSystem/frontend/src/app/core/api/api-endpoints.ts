export const API_ENDPOINTS = {
  auth: {
    signup: '/api/Auth/signup',
    login: '/api/Auth/login',
    validateToken: '/api/Auth/validate-token'
  },
  bookings: {
    search: '/api/search/buses',
    legacySearch: '/api/Bookings/search',
    list: '/api/Bookings',
    cancelTicket: (bookingId: number) => `/api/bookings/${bookingId}/cancel`,
    seats: (scheduleId: number) => `/api/seats/${scheduleId}`,
    legacySeats: (scheduleId: number) => `/api/Bookings/schedules/${scheduleId}/seats`,
    hold: '/api/booking/hold',
    checkout: '/api/Payments/checkout',
    ticket: (bookingId: number) => `/api/Bookings/${bookingId}/ticket`,
    downloadTicket: (bookingId: number) => `/api/Bookings/${bookingId}/ticket/download`,
    emailTicket: (bookingId: number) => `/api/Bookings/${bookingId}/ticket/email`
  },
  payment: {
    init: '/api/payment/init',
    details: (paymentId: number) => `/api/Payments/${paymentId}`,
    confirm: (paymentId: number) => `/api/Payments/${paymentId}/confirm`,
    fail: (paymentId: number) => `/api/Payments/${paymentId}/fail`
  },
  common: {
    cities: '/api/cities'
  },
  user: {
    profile: '/api/user/profile',
    bookings: '/api/Bookings'
  },
  operators: {
    register: '/api/operator/register',
    buses: '/api/operator/buses',
    schedules: '/api/operator/schedules',
    trips: '/api/operator/trips',
    revenue: '/api/operator/revenue',
    cancelTrip: (scheduleId: number) => `/api/operator/trips/${scheduleId}/cancel`,
    routes: '/api/operator/routes',
    offices: '/api/operator/offices',
    addBus: '/api/operator/buses',
    addSchedule: '/api/operator/schedule',
    addOffice: '/api/operator/office'
  },
  admin: {
    operatorRequests: '/api/Admin/operator-requests',
    operatorRequest: (operatorId: number) => `/api/Admin/operator-requests/${operatorId}`,
    approveOperator: (operatorId: number) => `/api/Admin/operator-requests/${operatorId}/approve`,
    rejectOperator: (operatorId: number) => `/api/Admin/operator-requests/${operatorId}/reject`,
    revenue: '/api/admin/revenue',
    routes: '/api/admin/route',
    addRoute: '/api/admin/route',
    operators: '/api/admin/operators',
    disableOperator: (operatorId: number) => `/api/Admin/operator/${operatorId}/disable`
  }
} as const;
