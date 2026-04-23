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
  cityName: string | null;
  address: string | null;
}

export interface OperatorOfficeResponse {
  id: number;
  cityName: string | null;
  address: string | null;
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

export interface CreateBusRequest {
  totalSeats: number;
  layoutJson: string | null;
  isActive: boolean;
}

export interface BusResponse {
  busId: number;
  operatorId: number;
  totalSeats: number;
  layoutJson: string | null;
  isActive: boolean;
  createdAt: string;
  seats: string[] | null;
}

export interface CreateBusScheduleRequest {
  busId: number;
  sourceCityName: string | null;
  destinationCityName: string | null;
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

export interface ScheduleSearchResponse {
  scheduleId: number;
  busId: number;
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
  availableSeats: number;
}

export interface SeatAvailabilityResponse {
  seatId: number;
  seatNumber: string | null;
  isBooked: boolean;
}

export interface BookingPassengerRequest {
  name: string | null;
  age: number;
  gender: string | null;
  seatId: number;
}

export interface CreateBookingRequest {
  scheduleId: number;
  passengers: BookingPassengerRequest[] | null;
  contactEmail: string | null;
  contactPhone: string | null;
  paymentMethod: string | null;
  paymentReference: string | null;
}

export interface BookingJourneyResponse {
  scheduleId: number;
  busId: number;
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
  totalAmount: number;
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
