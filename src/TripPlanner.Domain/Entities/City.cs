namespace TripPlanner.Domain.Entities;

public class City
{
    public int CityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }

    public virtual ICollection<Location> Locations { get; set; } = new List<Location>();
}