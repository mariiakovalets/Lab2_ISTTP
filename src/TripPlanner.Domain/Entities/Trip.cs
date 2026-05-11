using System.ComponentModel.DataAnnotations;

namespace TripPlanner.Domain.Entities;

public class Trip
{
    public int TripId { get; set; }
    public int UserId { get; set; }

    [Required(ErrorMessage = "Поле не повинно бути порожнім")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(20)]
    public string Status { get; set; } = "active";

    public virtual User? User { get; set; }
    public virtual ICollection<TripLocation> TripLocations { get; set; } = new List<TripLocation>();
}