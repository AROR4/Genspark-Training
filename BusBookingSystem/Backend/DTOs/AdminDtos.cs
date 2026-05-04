using System.ComponentModel.DataAnnotations;

namespace BusBookingApp.Dtos;

public record CreateRouteDto(
    [Required] int SourceCityId,
    [Required] int DestinationCityId);
