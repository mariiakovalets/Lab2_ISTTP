using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TripPlanner.Domain.Entities;
using TripPlanner.Infrastructure.Data;

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
    public async Task<ActionResult<IEnumerable<User>>> GetUsers([FromQuery] string? role)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrEmpty(role))
        {
            query = query.Where(u => u.Role == role);
        }

        return await query.ToListAsync();
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

    // PUT: api/Users/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutUser(int id, User user)
    {
        if (id != user.UserId)
        {
            return BadRequest(new { message = "Id в URL не збігається з id в тілі запиту" });
        }

        // Перевірка унікальності email (крім поточного юзера)
        var emailDuplicate = await _context.Users
            .AnyAsync(u => u.Email == user.Email && u.UserId != id);
        if (emailDuplicate)
        {
            return Conflict(new { message = $"Користувач з email '{user.Email}' вже існує" });
        }

        _context.Entry(user).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Users.AnyAsync(e => e.UserId == id))
            {
                return NotFound(new { message = $"Користувача з id={id} не знайдено" });
            }
            throw;
        }

        return NoContent();
    }

    // DELETE: api/Users/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users
            .Include(u => u.Trips)
            .FirstOrDefaultAsync(u => u.UserId == id);

        if (user == null)
        {
            return NotFound(new { message = $"Користувача з id={id} не знайдено" });
        }

        if (user.Trips.Any(t => t.Status == "active"))
        {
            return BadRequest(new { message = "Неможливо видалити користувача з активними подорожами" });
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}