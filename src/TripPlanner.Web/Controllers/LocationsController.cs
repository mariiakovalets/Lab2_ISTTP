using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TripPlanner.Domain.Entities;
using TripPlanner.Infrastructure.Data;

namespace TripPlanner.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LocationsController : ControllerBase
{
    private readonly TripPlannerDbContext _context;

    public LocationsController(TripPlannerDbContext context)
    {
        _context = context;
    }

    // GET: api/Locations

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Location>>> GetLocations(
        [FromQuery] int? cityId,
        [FromQuery] string? category)
    {
        var query = _context.Locations
            .Include(l => l.City)
            .AsQueryable();

        if (cityId.HasValue)
        {
            query = query.Where(l => l.CityId == cityId.Value);
        }

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(l => l.Category == category);
        }

        return await query.ToListAsync();
    }

    // GET: api/Locations/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Location>> GetLocation(int id)
    {
        var location = await _context.Locations
            .Include(l => l.City)
            .Include(l => l.Reviews)
            .FirstOrDefaultAsync(l => l.LocationId == id);

        if (location == null)
        {
            return NotFound(new { message = $"Локацію з id={id} не знайдено" });
        }

        return location;
    }

    // POST: api/Locations
    [HttpPost]
    public async Task<ActionResult<Location>> PostLocation(Location location)
    {
        // Перевірка що місто існує
        var cityExists = await _context.Cities.AnyAsync(c => c.CityId == location.CityId);
        if (!cityExists)
        {
            return BadRequest(new { message = $"Місто з id={location.CityId} не існує" });
        }

        // Перевірка на дублікат назви в межах міста
        var duplicate = await _context.Locations
            .AnyAsync(l => l.Name == location.Name && l.CityId == location.CityId);
        if (duplicate)
        {
            return Conflict(new { message = $"Локація '{location.Name}' вже існує в цьому місті" });
        }

        _context.Locations.Add(location);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetLocation), new { id = location.LocationId }, location);
    }

    // PUT: api/Locations/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutLocation(int id, Location location)
    {
        if (id != location.LocationId)
        {
            return BadRequest(new { message = "Id в URL не збігається з id в тілі запиту" });
        }

        var cityExists = await _context.Cities.AnyAsync(c => c.CityId == location.CityId);
        if (!cityExists)
        {
            return BadRequest(new { message = $"Місто з id={location.CityId} не існує" });
        }

        _context.Entry(location).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Locations.AnyAsync(e => e.LocationId == id))
            {
                return NotFound(new { message = $"Локацію з id={id} не знайдено" });
            }
            throw;
        }

        return NoContent();
    }

    // DELETE: api/Locations/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLocation(int id)
    {
        var location = await _context.Locations
            .Include(l => l.TripLocations)
            .FirstOrDefaultAsync(l => l.LocationId == id);

        if (location == null)
        {
            return NotFound(new { message = $"Локацію з id={id} не знайдено" });
        }

        if (location.TripLocations.Any())
        {
            return BadRequest(new { message = $"Неможливо видалити локацію '{location.Name}', бо вона додана в подорожі" });
        }

        _context.Locations.Remove(location);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}