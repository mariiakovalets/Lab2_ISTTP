using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TripPlanner.Domain.Entities;
using TripPlanner.Infrastructure.Data;

namespace TripPlanner.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CitiesController : ControllerBase
{
    private readonly TripPlannerDbContext _context;

    public CitiesController(TripPlannerDbContext context)
    {
        _context = context;
    }

    // GET: api/Cities
    // GET: api/Cities?category=Столиця
    [HttpGet]
    public async Task<ActionResult<IEnumerable<City>>> GetCities([FromQuery] string? category)
    {
        var query = _context.Cities.AsQueryable();

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(c => c.Category == category);
        }

        return await query.ToListAsync();
    }

    // GET: api/Cities/5
    [HttpGet("{id}")]
    public async Task<ActionResult<City>> GetCity(int id)
    {
        var city = await _context.Cities
            .Include(c => c.Locations)
            .FirstOrDefaultAsync(c => c.CityId == id);

        if (city == null)
        {
            return NotFound(new { message = $"Місто з id={id} не знайдено" });
        }

        return city;
    }

    // POST: api/Cities
    [HttpPost]
    public async Task<ActionResult<City>> PostCity(City city)
    {
        // Перевірка на дублікат назви
        var exists = await _context.Cities.AnyAsync(c => c.Name == city.Name);
        if (exists)
        {
            return Conflict(new { message = $"Місто '{city.Name}' вже існує" });
        }

        _context.Cities.Add(city);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCity), new { id = city.CityId }, city);
    }

    // PUT: api/Cities/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutCity(int id, City city)
    {
        if (id != city.CityId)
        {
            return BadRequest(new { message = "Id в URL не збігається з id в тілі запиту" });
        }

        // Перевірка на дублікат назви (крім поточного міста)
        var duplicate = await _context.Cities
            .AnyAsync(c => c.Name == city.Name && c.CityId != id);
        if (duplicate)
        {
            return Conflict(new { message = $"Місто '{city.Name}' вже існує" });
        }

        _context.Entry(city).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Cities.AnyAsync(e => e.CityId == id))
            {
                return NotFound(new { message = $"Місто з id={id} не знайдено" });
            }
            throw;
        }

        return NoContent();
    }

    // DELETE: api/Cities/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCity(int id)
    {
        var city = await _context.Cities
            .Include(c => c.Locations)
            .FirstOrDefaultAsync(c => c.CityId == id);

        if (city == null)
        {
            return NotFound(new { message = $"Місто з id={id} не знайдено" });
        }

        if (city.Locations.Any())
        {
            return BadRequest(new { message = $"Неможливо видалити місто '{city.Name}', бо в ньому є {city.Locations.Count} локацій" });
        }

        _context.Cities.Remove(city);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}