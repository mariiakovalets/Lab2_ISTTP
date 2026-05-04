namespace TripPlanner.Domain.Entities;

public class Review
{
    public int ReviewId { get; set; }
    public int UserId { get; set; }
    public int LocationId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual User User { get; set; } = null!;
    public virtual Location Location { get; set; } = null!;
}