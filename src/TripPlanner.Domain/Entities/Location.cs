using System.ComponentModel.DataAnnotations;

namespace TripPlanner.Domain.Entities;

public class Location
{
    public int LocationId { get; set; }
    public int CityId { get; set; }

    [Required(ErrorMessage = "Поле не повинно бути порожнім")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Address { get; set; }

    public string? Description { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    public virtual City City { get; set; } = null!;
    public virtual ICollection<TripLocation> TripLocations { get; set; } = new List<TripLocation>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}