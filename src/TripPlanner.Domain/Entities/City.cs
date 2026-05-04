using System.ComponentModel.DataAnnotations;

namespace TripPlanner.Domain.Entities;

public class City
{
    public int CityId { get; set; }

    [Required(ErrorMessage = "Поле не повинно бути порожнім")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    public virtual ICollection<Location> Locations { get; set; } = new List<Location>();
}