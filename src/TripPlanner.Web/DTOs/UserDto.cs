namespace TripPlanner.Web.DTOs;

public class UserDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public List<UserTripDto> Trips { get; set; } = new();
    public List<UserReviewDto> Reviews { get; set; } = new();
}

public class UserTripDto
{
    public int TripId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<TripLocationDto> TripLocations { get; set; } = new();
}

public class TripLocationDto
{
    public int TripLocationId { get; set; }
    public int LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public string? LocationAddress { get; set; }
    public string? LocationCategory { get; set; }
    public DateTime? VisitDatetime { get; set; }
}

public class UserReviewDto
{
    public int ReviewId { get; set; }
    public int LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}