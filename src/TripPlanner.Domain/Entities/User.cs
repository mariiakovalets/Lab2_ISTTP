namespace TripPlanner.Domain.Entities;

public class User
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "user";

    public virtual ICollection<Trip> Trips { get; set; } = new List<Trip>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}