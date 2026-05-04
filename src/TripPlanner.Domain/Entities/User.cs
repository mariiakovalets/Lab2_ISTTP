using System.ComponentModel.DataAnnotations;

namespace TripPlanner.Domain.Entities;

public class User
{
    public int UserId { get; set; }

    [Required(ErrorMessage = "Поле не повинно бути порожнім")]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Поле не повинно бути порожнім")]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Поле не повинно бути порожнім")]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(10)]
    public string Role { get; set; } = "user";

    public virtual ICollection<Trip> Trips { get; set; } = new List<Trip>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}