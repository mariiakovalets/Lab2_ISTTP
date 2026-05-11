using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TripPlanner.Domain.Entities;
using TripPlanner.Infrastructure.Data;

namespace TripPlanner.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TripsController : ControllerBase
{
    private readonly TripPlannerDbContext _context;

    public TripsController(TripPlannerDbContext context)
    {
        _context = context;
    }

    // GET: api/Trips
    // GET: api/Trips?status=active&userId=1
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Trip>>> GetTrips(
        [FromQuery] string? status,
        [FromQuery] int? userId)
    {
        var query = _context.Trips
            .Include(t => t.User)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(t => t.Status == status);
        }

        if (userId.HasValue)
        {
            query = query.Where(t => t.UserId == userId.Value);
        }

        return await query.ToListAsync();
    }

    // GET: api/Trips/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Trip>> GetTrip(int id)
    {
        var trip = await _context.Trips
            .Include(t => t.User)
            .Include(t => t.TripLocations)
                .ThenInclude(tl => tl.Location)
            .FirstOrDefaultAsync(t => t.TripId == id);

        if (trip == null)
        {
            return NotFound(new { message = $"Подорож з id={id} не знайдено" });
        }

        return trip;
    }

    // POST: api/Trips
    [HttpPost]
    public async Task<ActionResult<Trip>> PostTrip(Trip trip)
    {
        // Перевірка що юзер існує
        var userExists = await _context.Users.AnyAsync(u => u.UserId == trip.UserId);
        if (!userExists)
        {
            return BadRequest(new { message = $"Користувача з id={trip.UserId} не існує" });
        }

        // Перевірка що назва подорожі не дублюється для цього юзера
        var duplicate = await _context.Trips
            .AnyAsync(t => t.Name == trip.Name && t.UserId == trip.UserId);
        if (duplicate)
        {
            return Conflict(new { message = $"У вас вже є подорож з назвою '{trip.Name}'" });
        }

        trip.CreatedAt = DateTime.UtcNow;
        _context.Trips.Add(trip);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTrip), new { id = trip.TripId }, trip);
    }

    // PUT: api/Trips/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutTrip(int id, Trip trip)
    {
        if (id != trip.TripId)
        {
            return BadRequest(new { message = "Id в URL не збігається з id в тілі запиту" });
        }

        // Перевірка що статус валідний
        var validStatuses = new[] { "active", "completed", "cancelled" };
        if (!validStatuses.Contains(trip.Status))
        {
            return BadRequest(new { message = "Статус може бути: active, completed, cancelled" });
        }

        _context.Entry(trip).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Trips.AnyAsync(e => e.TripId == id))
            {
                return NotFound(new { message = $"Подорож з id={id} не знайдено" });
            }
            throw;
        }

        return NoContent();
    }

    // DELETE: api/Trips/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTrip(int id)
    {
        var trip = await _context.Trips.FindAsync(id);

        if (trip == null)
        {
            return NotFound(new { message = $"Подорож з id={id} не знайдено" });
        }

        _context.Trips.Remove(trip);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}