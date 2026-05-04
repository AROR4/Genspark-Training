using System.Globalization;
using System.Text;
using BusBookingApp.Models;

namespace BusBookingApp.Services;

public class TicketService
{
    public string GenerateBookingCode(Booking booking)
    {
        return $"BK{booking.CreatedAt:yyyyMMdd}{booking.Id:D6}";
    }

    public string GenerateTicketNumber(Booking booking)
    {
        return $"TKT-{booking.CreatedAt:yyyyMMdd}-{booking.Id:D6}";
    }

    public string BuildTicketContent(
        Booking booking,
        string bookingCode,
        string ticketNumber,
        string sourceCity,
        string destinationCity,
        string operatorName,
        string paymentStatus)
    {
        var schedule = booking.Schedule!;
        var passengers = booking.BookingPassengers
            .Select(passenger => $"{passenger.Name} | {passenger.Gender} | {passenger.Age} | Seat {passenger.SeatAvailability?.Seat.SeatNumber}")
            .ToList();

        var builder = new StringBuilder();
        builder.AppendLine("Bus Booking E-Ticket");
        builder.AppendLine($"Booking Code: {bookingCode}");
        builder.AppendLine($"Ticket Number: {ticketNumber}");
        builder.AppendLine($"Status: {booking.Status}");
        builder.AppendLine($"Operator: {operatorName}");
        builder.AppendLine($"Route: {sourceCity} -> {destinationCity}");
        builder.AppendLine($"Travel Date: {schedule.TravelDate:yyyy-MM-dd}");
        builder.AppendLine($"Departure Time: {schedule.DepartureTime}");
        builder.AppendLine($"Arrival: {schedule.ArrivalDate:yyyy-MM-dd} {schedule.ArrivalTime}");
        builder.AppendLine($"Booked At (UTC): {booking.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}");
        builder.AppendLine($"Total Amount: {booking.TotalAmount:0.00}");
        builder.AppendLine($"Payment Status: {paymentStatus}");
        builder.AppendLine();
        builder.AppendLine("Passengers:");

        foreach (var passenger in passengers)
        {
            builder.AppendLine(passenger);
        }

        return builder.ToString();
    }
}
