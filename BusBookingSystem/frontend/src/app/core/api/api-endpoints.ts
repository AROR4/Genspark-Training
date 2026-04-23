export const API_ENDPOINTS = {
  auth: {
    signup: '/api/Auth/signup',
    login: '/api/Auth/login'
  },
  bookings: {
    search: '/api/Bookings/search',
    list: '/api/Bookings',
    seats: (scheduleId: number) => `/api/Bookings/schedules/${scheduleId}/seats`,
    ticket: (bookingId: number) => `/api/Bookings/${bookingId}/ticket`,
    downloadTicket: (bookingId: number) => `/api/Bookings/${bookingId}/ticket/download`,
    emailTicket: (bookingId: number) => `/api/Bookings/${bookingId}/ticket/email`
  },
  operators: {
    register: '/api/Operators/register',
    buses: '/api/Operators/buses',
    schedules: '/api/Operators/schedules'
  },
  admin: {
    operatorRequests: '/api/Admin/operator-requests',
    operatorRequest: (operatorId: number) => `/api/Admin/operator-requests/${operatorId}`,
    approveOperator: (operatorId: number) => `/api/Admin/operator-requests/${operatorId}/approve`,
    rejectOperator: (operatorId: number) => `/api/Admin/operator-requests/${operatorId}/reject`
  }
} as const;
