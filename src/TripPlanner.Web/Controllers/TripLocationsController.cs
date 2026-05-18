using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TripPlanner.Domain.Entities;
using TripPlanner.Infrastructure.Data;

namespace TripPlanner.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TripLocationsController : ControllerBase
{
    private readonly TripPlannerDbContext _context;

    public TripLocationsController(TripPlannerDbContext context)
    {
        _context = context;
    }

    // GET: api/TripLocations
    // GET: api/TripLocations?tripId=1
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TripLocation>>> GetTripLocations(
        [FromQuery] int? tripId)
    {
        var query = _context.TripLocations
            .Include(tl => tl.Trip)
            .Include(tl => tl.Location)
            .AsQueryable();

        if (tripId.HasValue)
        {
            query = query.Where(tl => tl.TripId == tripId.Value);
        }

        return await query.ToListAsync();
    }

    // GET: api/TripLocations/5
    [HttpGet("{id}")]
    public async Task<ActionResult<TripLocation>> GetTripLocation(int id)
    {
        var tripLocation = await _context.TripLocations
            .Include(tl => tl.Trip)
            .Include(tl => tl.Location)
            .FirstOrDefaultAsync(tl => tl.TripLocationId == id);

        if (tripLocation == null)
        {
            return NotFound(new { message = $"Запис з id={id} не знайдено" });
        }

        return tripLocation;
    }

// POST: api/TripLocations
    [HttpPost]
    public async Task<ActionResult<TripLocation>> PostTripLocation(TripLocation tripLocation)
    {
        // Перевірка що подорож існує
        var trip = await _context.Trips.FindAsync(tripLocation.TripId);
        if (trip == null)
        {
            return BadRequest(new { message = $"Подорож з id={tripLocation.TripId} не існує" });
        }

        // Перевірка що локація існує
        var locationExists = await _context.Locations.AnyAsync(l => l.LocationId == tripLocation.LocationId);
        if (!locationExists)
        {
            return BadRequest(new { message = $"Локацію з id={tripLocation.LocationId} не існує" });
        }

        // Перевірка що дата візиту не раніше за дату створення подорожі
        if (tripLocation.VisitDatetime.HasValue && tripLocation.VisitDatetime.Value < trip.CreatedAt)
        {
            return BadRequest(new { message = "Дата візиту не може бути раніше за дату створення подорожі" });
        }

        // Перевірка що локація ще не додана в цю подорож
        var duplicate = await _context.TripLocations
            .AnyAsync(tl => tl.TripId == tripLocation.TripId && tl.LocationId == tripLocation.LocationId);
        if (duplicate)
        {
            return Conflict(new { message = "Ця локація вже додана в цю подорож" });
        }

        _context.TripLocations.Add(tripLocation);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTripLocation), new { id = tripLocation.TripLocationId }, tripLocation);
    }

    // PUT: api/TripLocations/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutTripLocation(int id, TripLocation tripLocation)
    {
        if (id != tripLocation.TripLocationId)
        {
            return BadRequest(new { message = "Id в URL не збігається з id в тілі запиту" });
        }

        _context.Entry(tripLocation).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.TripLocations.AnyAsync(e => e.TripLocationId == id))
            {
                return NotFound(new { message = $"Запис з id={id} не знайдено" });
            }
            throw;
        }

        return NoContent();
    }

    // DELETE: api/TripLocations/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTripLocation(int id)
    {
        var tripLocation = await _context.TripLocations.FindAsync(id);

        if (tripLocation == null)
        {
            return NotFound(new { message = $"Запис з id={id} не знайдено" });
        }

        _context.TripLocations.Remove(tripLocation);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}