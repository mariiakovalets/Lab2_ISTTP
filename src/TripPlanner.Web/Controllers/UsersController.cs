using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TripPlanner.Domain.Entities;
using TripPlanner.Infrastructure.Data;
using TripPlanner.Web.DTOs;

namespace TripPlanner.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly TripPlannerDbContext _context;

    public UsersController(TripPlannerDbContext context)
    {
        _context = context;
    }

// GET: api/Users
    // GET: api/Users?role=admin
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers([FromQuery] string? role)
    {
        var query = _context.Users
            .Include(u => u.Trips)
                .ThenInclude(t => t.TripLocations)
                    .ThenInclude(tl => tl.Location)
            .Include(u => u.Reviews)
                .ThenInclude(r => r.Location)
            .AsQueryable();

        if (!string.IsNullOrEmpty(role))
        {
            query = query.Where(u => u.Role == role);
        }

        var users = await query.ToListAsync();

        var result = users.Select(u => new UserDto
        {
            UserId = u.UserId,
            Username = u.Username,
            Email = u.Email,
            Role = u.Role,
            Trips = u.Trips.Select(t => new UserTripDto
            {
                TripId = t.TripId,
                Name = t.Name,
                Description = t.Description,
                CreatedAt = t.CreatedAt,
                Status = t.Status,
                TripLocations = t.TripLocations.Select(tl => new TripLocationDto
                {
                    TripLocationId = tl.TripLocationId,
                    LocationId = tl.LocationId,
                    LocationName = tl.Location?.Name ?? "",
                    LocationAddress = tl.Location?.Address,
                    LocationCategory = tl.Location?.Category,
                    VisitDatetime = tl.VisitDatetime
                }).ToList()
            }).ToList(),
            Reviews = u.Reviews.Select(r => new UserReviewDto
            {
                ReviewId = r.ReviewId,
                LocationId = r.LocationId,
                LocationName = r.Location?.Name ?? "",
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            }).ToList()
        });

        return Ok(result);
    }

    // GET: api/Users/5
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        var user = await _context.Users
            .Include(u => u.Trips)
            .Include(u => u.Reviews)
            .FirstOrDefaultAsync(u => u.UserId == id);

        if (user == null)
        {
            return NotFound(new { message = $"Користувача з id={id} не знайдено" });
        }

        return user;
    }

    // POST: api/Users
    [HttpPost]
    public async Task<ActionResult<User>> PostUser(User user)
    {
        // Перевірка що email унікальний
        var emailExists = await _context.Users.AnyAsync(u => u.Email == user.Email);
        if (emailExists)
        {
            return Conflict(new { message = $"Користувач з email '{user.Email}' вже існує" });
        }

        // Перевірка що username унікальний
        var usernameExists = await _context.Users.AnyAsync(u => u.Username == user.Username);
        if (usernameExists)
        {
            return Conflict(new { message = $"Нікнейм '{user.Username}' вже зайнятий" });
        }

        // Перевірка формату email
        if (!user.Email.Contains("@"))
        {
            return BadRequest(new { message = "Невірний формат email" });
        }

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, user);
    }

[HttpPut("{id}")]
public async Task<IActionResult> PutUser(int id, User user)
{
    if (id != user.UserId)
    {
        return BadRequest(new { message = "Id в URL не збігається з id в тілі запиту" });
    }

    var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
    if (existingUser == null)
    {
        return NotFound();
    }

    var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == user.CurrentUserId);

    if (currentUser != null && currentUser.Role == "admin")
    {
        // якщо адмін редагує НЕ себе
        if (currentUser.UserId != id)
        {
            user.Username = existingUser.Username;
            user.Email = existingUser.Email;
        }
    }

    existingUser.Username = user.Username;
    existingUser.Email = user.Email;
    existingUser.Role = user.Role;

    await _context.SaveChangesAsync();

    return NoContent();
}

[HttpDelete("{id}")]
public async Task<IActionResult> DeleteUser(int id)
{
    var currentUserId = 1; // тут у тебе зараз імітація авторизації, підстав своє
    var currentUser = await _context.Users.FindAsync(currentUserId);

    if (currentUser != null && currentUser.Role == "admin" && currentUser.UserId == id)
    {
        return BadRequest(new { message = "Адміністратор не може видалити сам себе" });
    }

    var user = await _context.Users.FindAsync(id);
    if (user == null) return NotFound();

    _context.Users.Remove(user);
    await _context.SaveChangesAsync();

    return NoContent();
}
}