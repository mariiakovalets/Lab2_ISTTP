namespace TripPlanner.Domain.Entities;

public class Trip
{
    public int TripId { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "active";

    public virtual User User { get; set; } = null!;
    public virtual ICollection<TripLocation> TripLocations { get; set; } = new List<TripLocation>();
}