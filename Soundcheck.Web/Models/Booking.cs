using System.ComponentModel.DataAnnotations;

namespace Soundcheck.Web.Models;

public class Booking
{
    public int Id { get; init; }

    [Required]
    public string ClientId { get; init; } = string.Empty;

    public ApplicationUser Client { get; init; } = null!;

    [Required]
    [StringLength(200)]
    public string Topic { get; init; } = string.Empty;

    public BookingStatus Status { get; init; } = BookingStatus.Pending;

    [Range(0, 100000)]
    public decimal Price { get; init; }

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
