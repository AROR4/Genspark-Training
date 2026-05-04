using System.Text.Json;
using BusBookingApp.Models;

namespace BusBookingApp.Services;

public class SeatLayoutService
{
    private const int SeatsPerRow = 4;

    public string GenerateLayoutJson(int totalSeats)
    {
        var layout = new List<object>();
        var remainingSeats = totalSeats;
        var rowIndex = 0;

        while (remainingSeats > 0)
        {
            var rowLetter = (char)('A' + rowIndex);
            var seatsInRow = Math.Min(SeatsPerRow, remainingSeats);
            var seatNumbers = new List<string>(seatsInRow);

            for (var seatIndex = 1; seatIndex <= seatsInRow; seatIndex++)
            {
                seatNumbers.Add($"{rowLetter}{seatIndex}");
            }

            layout.Add(new
            {
                row = rowLetter.ToString(),
                seats = seatNumbers
            });

            remainingSeats -= seatsInRow;
            rowIndex++;
        }

        return JsonSerializer.Serialize(layout);
    }

    public List<Seat> GenerateSeats(int busId, int totalSeats)
    {
        var seats = new List<Seat>(totalSeats);

        for (var index = 0; index < totalSeats; index++)
        {
            var rowLetter = (char)('A' + (index / SeatsPerRow));
            var seatNumber = index % SeatsPerRow + 1;

            seats.Add(new Seat
            {
                BusId = busId,
                SeatNumber = $"{rowLetter}{seatNumber}"
            });
        }

        return seats;
    }
}
