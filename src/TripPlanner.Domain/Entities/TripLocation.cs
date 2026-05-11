namespace TripPlanner.Domain.Entities;

public class TripLocation
{
    public int TripLocationId { get; set; }
    public int TripId { get; set; }
    public int LocationId { get; set; }
    public DateTime? VisitDatetime { get; set; }

    public virtual Trip? Trip { get; set; }
    public virtual Location? Location { get; set; }
}