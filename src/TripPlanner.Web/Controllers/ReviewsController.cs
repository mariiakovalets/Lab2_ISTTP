using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TripPlanner.Domain.Entities;
using TripPlanner.Infrastructure.Data;

namespace TripPlanner.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReviewsController : ControllerBase
{
    private readonly TripPlannerDbContext _context;

    public ReviewsController(TripPlannerDbContext context)
    {
        _context = context;
    }

    // GET: api/Reviews
    // GET: api/Reviews?locationId=1
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Review>>> GetReviews(
        [FromQuery] int? locationId)
    {
        var query = _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Location)
            .AsQueryable();

        if (locationId.HasValue)
        {
            query = query.Where(r => r.LocationId == locationId.Value);
        }

        return await query.ToListAsync();
    }

    // GET: api/Reviews/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Review>> GetReview(int id)
    {
        var review = await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Location)
            .FirstOrDefaultAsync(r => r.ReviewId == id);

        if (review == null)
        {
            return NotFound(new { message = $"Відгук з id={id} не знайдено" });
        }

        return review;
    }

    // GET: api/Reviews/location/5/average
    [HttpGet("location/{locationId}/average")]
    public async Task<ActionResult> GetAverageRating(int locationId)
    {
        var locationExists = await _context.Locations.AnyAsync(l => l.LocationId == locationId);
        if (!locationExists)
        {
            return NotFound(new { message = $"Локацію з id={locationId} не знайдено" });
        }

        var reviews = await _context.Reviews
            .Where(r => r.LocationId == locationId)
            .ToListAsync();

        if (!reviews.Any())
        {
            return Ok(new { locationId, averageRating = 0, totalReviews = 0 });
        }

        return Ok(new
        {
            locationId,
            averageRating = Math.Round(reviews.Average(r => r.Rating), 1),
            totalReviews = reviews.Count
        });
    }

    // POST: api/Reviews
    [HttpPost]
    public async Task<ActionResult<Review>> PostReview(Review review)
    {
        // Перевірка що юзер існує
        var userExists = await _context.Users.AnyAsync(u => u.UserId == review.UserId);
        if (!userExists)
        {
            return BadRequest(new { message = $"Користувача з id={review.UserId} не існує" });
        }

        // Перевірка що локація існує
        var locationExists = await _context.Locations.AnyAsync(l => l.LocationId == review.LocationId);
        if (!locationExists)
        {
            return BadRequest(new { message = $"Локацію з id={review.LocationId} не існує" });
        }

        // Перевірка що рейтинг в межах 1-5
        if (review.Rating < 1 || review.Rating > 5)
        {
            return BadRequest(new { message = "Рейтинг має бути від 1 до 5" });
        }

        // Перевірка що юзер ще не залишав відгук на цю локацію
        var duplicate = await _context.Reviews
            .AnyAsync(r => r.UserId == review.UserId && r.LocationId == review.LocationId);
        if (duplicate)
        {
            return Conflict(new { message = "Ви вже залишали відгук на цю локацію. Використайте PUT для оновлення" });
        }

        review.CreatedAt = DateTime.UtcNow;
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetReview), new { id = review.ReviewId }, review);
    }

    // PUT: api/Reviews/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutReview(int id, Review review)
    {
        if (id != review.ReviewId)
        {
            return BadRequest(new { message = "Id в URL не збігається з id в тілі запиту" });
        }

        if (review.Rating < 1 || review.Rating > 5)
        {
            return BadRequest(new { message = "Рейтинг має бути від 1 до 5" });
        }

        _context.Entry(review).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Reviews.AnyAsync(e => e.ReviewId == id))
            {
                return NotFound(new { message = $"Відгук з id={id} не знайдено" });
            }
            throw;
        }

        return NoContent();
    }

    // DELETE: api/Reviews/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReview(int id)
    {
        var review = await _context.Reviews.FindAsync(id);

        if (review == null)
        {
            return NotFound(new { message = $"Відгук з id={id} не знайдено" });
        }

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}