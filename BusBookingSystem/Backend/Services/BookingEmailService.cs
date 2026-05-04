using System.Net;
using System.Net.Mail;
using BusBookingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BusBookingApp.Services;

public class BookingEmailService
{
    private readonly IConfiguration _configuration;
    private readonly TicketService _ticketService;

    public BookingEmailService(IConfiguration configuration, TicketService ticketService)
    {
        _configuration = configuration;
        _ticketService = ticketService;
    }

    public async Task<bool> TrySendTicketAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        var smtpHost = _configuration["Smtp:Host"];
        var fromEmail = _configuration["Smtp:FromEmail"];

        if (string.IsNullOrWhiteSpace(smtpHost) || string.IsNullOrWhiteSpace(fromEmail))
        {
            return false;
        }

        var recipient = booking.ContactEmail ?? booking.User?.Email;
        if (string.IsNullOrWhiteSpace(recipient))
        {
            return false;
        }

        var route = booking.Schedule?.Route;
        var operatorName = booking.Schedule?.Bus?.Operator?.CompanyName ?? "Bus Operator";
        var bookingCode = _ticketService.GenerateBookingCode(booking);
        var ticketNumber = _ticketService.GenerateTicketNumber(booking);
        var paymentStatus = booking.Payments
            .OrderByDescending(payment => payment.CreatedAt)
            .Select(payment => payment.Status)
            .FirstOrDefault() ?? "SUCCESS";

        var ticketContent = _ticketService.BuildTicketContent(
            booking,
            bookingCode,
            ticketNumber,
            route?.SourceCity?.Name ?? string.Empty,
            route?.DestinationCity?.Name ?? string.Empty,
            operatorName,
            paymentStatus);

        using var mailMessage = new MailMessage
        {
            From = new MailAddress(fromEmail, _configuration["Smtp:FromName"] ?? "Bus Booking"),
            Subject = $"Your Ticket {ticketNumber}",
            Body = $"Your booking is confirmed.\n\n{ticketContent}",
            IsBodyHtml = false
        };
        mailMessage.To.Add(recipient);
        mailMessage.Attachments.Add(new Attachment(
            new MemoryStream(System.Text.Encoding.UTF8.GetBytes(ticketContent)),
            $"{ticketNumber}.txt",
            "text/plain"));

        using var smtpClient = new SmtpClient(smtpHost, _configuration.GetValue("Smtp:Port", 587))
        {
            EnableSsl = _configuration.GetValue("Smtp:EnableSsl", true)
        };

        var username = _configuration["Smtp:Username"];
        var password = _configuration["Smtp:Password"];
        if (!string.IsNullOrWhiteSpace(username))
        {
            smtpClient.Credentials = new NetworkCredential(username, password);
        }

        cancellationToken.ThrowIfCancellationRequested();
        await smtpClient.SendMailAsync(mailMessage, cancellationToken);
        return true;
    }

    public async Task<bool> TrySendCancellationAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        var smtpHost = _configuration["Smtp:Host"];
        var fromEmail = _configuration["Smtp:FromEmail"];

        if (string.IsNullOrWhiteSpace(smtpHost) || string.IsNullOrWhiteSpace(fromEmail))
        {
            return false;
        }

        var recipient = booking.ContactEmail ?? booking.User?.Email;
        if (string.IsNullOrWhiteSpace(recipient))
        {
            return false;
        }

        var route = booking.Schedule?.Route;
        var operatorName = booking.Schedule?.Bus?.Operator?.CompanyName ?? "Bus Operator";
        var bookingCode = _ticketService.GenerateBookingCode(booking);
        var ticketNumber = _ticketService.GenerateTicketNumber(booking);
        var cancellationLabel = booking.CancellationType switch
        {
            CancellationService.AdminCancellation => "Cancelled by admin",
            CancellationService.OperatorBusCancellation => "Cancelled because the bus was disabled",
            CancellationService.OperatorTripCancellation => "Cancelled by operator",
            CancellationService.UserCancellation => "Cancelled by user",
            _ => "Cancelled"
        };

        var body = $"""
Your booking has been cancelled.

Booking Code: {bookingCode}
Ticket Number: {ticketNumber}
Operator: {operatorName}
Route: {route?.SourceCity?.Name ?? "N/A"} -> {route?.DestinationCity?.Name ?? "N/A"}
Travel Date: {booking.Schedule?.TravelDate}
Departure Time: {booking.Schedule?.DepartureTime}
Cancellation Type: {cancellationLabel}
Status: {booking.Status}
Refund Initiated: {(booking.RefundAmount.HasValue && booking.RefundAmount.Value > 0 ? "Yes" : "No")}
Refund Amount: {booking.RefundAmount?.ToString("0.00") ?? "0.00"}

If the refund has been initiated, it will be processed to your original payment method.
""";

        using var mailMessage = new MailMessage
        {
            From = new MailAddress(fromEmail, _configuration["Smtp:FromName"] ?? "Bus Booking"),
            Subject = $"Booking Cancelled - Refund Initiated for {ticketNumber}",
            Body = body,
            IsBodyHtml = false
        };
        mailMessage.To.Add(recipient);

        using var smtpClient = new SmtpClient(smtpHost, _configuration.GetValue("Smtp:Port", 587))
        {
            EnableSsl = _configuration.GetValue("Smtp:EnableSsl", true)
        };

        var username = _configuration["Smtp:Username"];
        var password = _configuration["Smtp:Password"];
        if (!string.IsNullOrWhiteSpace(username))
        {
            smtpClient.Credentials = new NetworkCredential(username, password);
        }

        cancellationToken.ThrowIfCancellationRequested();
        await smtpClient.SendMailAsync(mailMessage, cancellationToken);
        return true;
    }
}
