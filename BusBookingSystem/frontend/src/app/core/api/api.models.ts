export type BusType = 'ACSeater' | 'ACSleeper' | 'NonACSeater' | 'NonACSleeper';

export interface AuthResponse {
  userId: number;
  name: string | null;
  email: string | null;
  phoneNumber: string | null;
  role: string | null;
  isApproved: boolean;
  token: string | null;
  expiresAt: string;
}

export interface LoginRequest {
  email: string | null;
  password: string | null;
}

export interface SignUpRequest {
  name: string | null;
  email: string | null;
  phoneNumber: string | null;
  password: string | null;
}

export interface OperatorOfficeRequest {
  cityId: number;
  address: string | null;
}

export interface OperatorOfficeResponse {
  id: number;
  cityId?: number;
  cityName: string | null;
  address: string | null;
}

export interface OperatorRouteResponse {
  routeId: number;
  sourceCityId?: number | null;
  destinationCityId?: number | null;
  sourceCityName: string | null;
  destinationCityName: string | null;
  canCreateSchedule: boolean;
  missingCityId?: number | null;
  missingCityName?: string | null;
}

export interface OperatorRegisterRequest {
  ownerName: string | null;
  email: string | null;
  phoneNumber: string | null;
  password: string | null;
  companyName: string | null;
  legalName: string | null;
  contactEmail: string | null;
  contactPhone: string | null;
  registrationNumber: string | null;
  taxNumber: string | null;
  licenseNumber: string | null;
  offices: OperatorOfficeRequest[] | null;
}

export interface OperatorRegistrationResponse {
  operatorId: number;
  userId: number;
  companyName: string | null;
  approvalStatus: string | null;
  message: string | null;
}

export interface OperatorRequestResponse {
  operatorId: number;
  userId: number;
  ownerName: string | null;
  email: string | null;
  phoneNumber: string | null;
  companyName: string | null;
  legalName: string | null;
  contactEmail: string | null;
  contactPhone: string | null;
  registrationNumber: string | null;
  taxNumber: string | null;
  licenseNumber: string | null;
  approvalStatus: string | null;
  adminNotes: string | null;
  createdAt: string;
  offices: OperatorOfficeResponse[] | null;
}

export interface OperatorDecisionRequest {
  adminNotes: string | null;
}

export interface AdminRouteResponse {
  routeId: number;
  sourceCityName: string | null;
  destinationCityName: string | null;
  isActive?: boolean;
}

export interface AddRouteRequest {
  sourceCityId: number;
  destinationCityId: number;
}

export interface AdminOperatorResponse {
  id: number;
  userId: number;

  ownerName: string | null;
  companyName: string | null;
  legalName: string | null;

  contactEmail: string | null;
  contactPhone: string | null;

  registrationNumber: string | null;
  taxNumber: string | null;
  licenseNumber: string | null;

  approvalStatus: 'PENDING' | 'APPROVED' | 'REJECTED';

  isActive: boolean;
  adminNotes: string | null;

  createdAt: string;

  user: {
    id: number;
    name: string | null;
    email: string | null;
    phoneNumber: string | null;
    role: string | null;
    isApproved: boolean;
    createdAt: string;
  };

  offices: OperatorOfficeResponse[];
  buses: BusResponse[];
}

export interface CreateBusRequest {
  totalSeats: number;
  registrationNumber: string | null;
  company: string | null;
  type: BusType;
  isActive: boolean;
}

export interface BusResponse {
  busId: number;
  operatorId: number;
  totalSeats: number;
  registrationNumber?: string | null;
  company?: string | null;
  type?: BusType;
  layoutJson: string | null;
  isActive: boolean;
  createdAt: string;
  seats: string[] | null;
}

export interface CreateBusScheduleRequest {
  busId: number;
  routeId: number;
  sourceOfficeId: number;
  destinationOfficeId: number;
  travelDate: string;
  departureTime: string;
  durationMinutes: number;
  basePrice: number;
}

export interface BusScheduleResponse {
  scheduleId: number;
  busId: number;
  routeId: number;
  sourceCityName: string | null;
  destinationCityName: string | null;
  travelDate: string;
  departureTime: string;
  durationMinutes: number;
  arrivalDate: string;
  arrivalTime: string;
  basePrice: number;
  createdAt: string;
}

export interface OperatorTripPassengerResponse {
  name: string | null;
  age: number;
  gender: string | null;
  seatNumber: string | null;
}

export interface OperatorTripBookingResponse {
  bookingId: number;
  status: string | null;
  totalAmount: number;
  customerName: string | null;
  customerEmail: string | null;
  customerPhone: string | null;
  contactEmail: string | null;
  contactPhone: string | null;
  bookedAt: string;
  passengerCount: number;
  passengers: OperatorTripPassengerResponse[];
}

export interface OperatorTripResponse {
  scheduleId: number;
  busId: number;
  registrationNumber: string | null;
  company: string | null;
  type: BusType | string | null;
  sourceCityName: string | null;
  destinationCityName: string | null;
  travelDate: string;
  departureTime: string;
  arrivalDate: string;
  arrivalTime: string;
  durationMinutes: number;
  basePrice: number;
  isCancelled: boolean;
  totalSeats: number;
  bookedSeats: number;
  onHoldSeats: number;
  availableSeats: number;
  activeBookingCount: number;
  currentBookings: OperatorTripBookingResponse[];
}

export interface CancelOperatorTripRequest {
  reason?: string | null;
}

export interface ScheduleSearchResponse {
  scheduleId: number;
  busId: number;
  operatorName: string | null;
  sourceCity?: string | null;
  destinationCity?: string | null;
  sourceCityName: string | null;
  destinationCityName: string | null;
  travelDate: string;
  departureTime: string;
  arrivalDate: string;
  arrivalTime: string;
  durationMinutes: number;
  duration?: number;
  basePrice: number;
  price?: number;
  totalSeats: number;
  availableSeats: number;
  busType?: string | null;
}

export interface SeatAvailabilityResponse {
  seatId: number;
  seatAvailabilityId?: number;
  seatNumber: string | null;
  status: 'Available' | 'Booked' | 'Held' | string;
  holdExpiry?: string | null;
}

export interface LockSeatsRequest {
  scheduleId: number;
  seatIds: number[];
}

export interface LockSeatsResponse {
  lockedSeatIds: number[];
  expiresAt: string | null;
  message: string | null;
}

export interface BookingPassengerRequest {
  name: string | null;
  age: number;
  gender: string | null;
  seatAvailabilityId: number;
}

export interface CreateBookingRequest {
  scheduleId: number;
  passengers: BookingPassengerRequest[] | null;
  contactEmail: string | null;
  contactPhone: string | null;
  paymentMethod: string | null;
  paymentReference: string | null;
}

export interface CreateCheckoutResponse {
  bookingId: number;
  paymentId: number;
  bookingStatus: string | null;
  paymentStatus: string | null;
  gatewayOrderId: string | null;
  amount: number;
  baseAmount?: number;
  gstAmount?: number;
  convenienceFee?: number;
  totalAmount?: number;
  paymentMethod: string | null;
  expiresAtUtc: string;
}

export interface PaymentResponse {
  paymentId: number;
  bookingId: number;
  amount: number;
  paymentMethod: string | null;
  gatewayOrderId: string | null;
  gatewayPaymentId: string | null;
  paymentReference: string | null;
  status: string | null;
  refundStatus: string | null;
  failureReason: string | null;
  message: string | null;
  reference: string | null;
  createdAt: string;
  paidAt: string | null;
  expiresAtUtc: string;
}

export interface InitPaymentRequest {
  bookingId: number;
}

export interface ConfirmPaymentRequest {
  gatewayPaymentId: string;
  paymentReference?: string | null;
}

export interface FailPaymentRequest {
  failureReason: string;
  paymentReference?: string | null;
}

export interface BookingJourneyResponse {
  scheduleId: number;
  busId: number;
  registrationNumber?: string | null;
  operatorName: string | null;
  sourceCityName: string | null;
  destinationCityName: string | null;
  travelDate: string;
  departureTime: string;
  arrivalDate: string;
  arrivalTime: string;
  durationMinutes: number;
  basePrice: number;
  totalSeats: number;
}

export interface TicketPassengerResponse {
  name: string | null;
  age: number;
  gender: string | null;
  seatNumber: string | null;
}

export interface BookingResponse {
  bookingId: number;
  bookingCode: string | null;
  ticketNumber: string | null;
  status: string | null;
  baseAmount: number;
  gstAmount: number;
  convenienceFee: number;
  totalAmount: number;
  refundAmount?: number | null;
  operatorLoss?: number | null;
  adminRevenue?: number | null;
  cancellationType?: 'ADMIN' | 'OPERATOR_BUS' | 'OPERATOR_TRIP' | string | null;
  paymentStatus: string | null;
  bookedAt: string;
  contactEmail: string | null;
  contactPhone: string | null;
  journey: BookingJourneyResponse | null;
  passengers: TicketPassengerResponse[] | null;
  emailSent: boolean;
}

export interface TicketEmailResponse {
  bookingId: number;
  email: string | null;
  sent: boolean;
  message: string | null;
}

export interface UserCancelBookingResponse {
  message: string | null;
  refundPercentage: number;
  refundAmount: number;
  booking: {
    bookingId: number;
    status: string | null;
    refundAmount?: number | null;
    cancellationType?: 'USER' | 'ADMIN' | 'OPERATOR_BUS' | 'OPERATOR_TRIP' | string | null;
    journey?: {
      scheduleId: number;
      busId: number;
      registrationNumber?: string | null;
    } | null;
  } | null;
}

export interface UserProfileResponse {
  name: string | null;
  email: string | null;
  phone: string | null;
}

export interface UserBookingSummary {
  bookingId: number;
  status: string | null;
  paymentStatus: string | null;
  travelDate: string;
  departureTime: string;
  sourceCity: string | null;
  destinationCity: string | null;
  operatorName: string | null;
  busId: number | null;
  registrationNumber: string | null;
  contactPhone: string | null;
  seats: string[];
  baseAmount: number;
  gstAmount: number;
  convenienceFee: number;
  totalAmount: number;
  refundAmount?: number | null;
  operatorLoss?: number | null;
  adminRevenue?: number | null;
  cancellationType?: 'USER' | 'ADMIN' | 'OPERATOR_BUS' | 'OPERATOR_TRIP' | string | null;
  ticketNumber: string | null;
  bookingCode: string | null;
}

export interface OperatorRevenueResponse {
  totalRevenue: number;
  totalTickets: number;
  totalBookings: number;
  totalCancelled: number;
}

export interface AdminRevenueResponse {
  adminRevenue: number;
  totalBookings: number;
  totalCancelled: number;
}
