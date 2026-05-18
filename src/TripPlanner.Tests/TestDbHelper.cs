using Microsoft.EntityFrameworkCore;
using TripPlanner.Infrastructure.Data;
using TripPlanner.Domain.Entities;

namespace TripPlanner.Tests;

public static class TestDbHelper
{
    public static TripPlannerDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<TripPlannerDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        var context = new TripPlannerDbContext(options);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        return context;
    }

    public static void SeedBasicData(TripPlannerDbContext context)
    {
        var user1 = new User { UserId = 1, Username = "polina", Email = "polina@gmail.com", PasswordHash = "hash123", Role = "user" };
        var user2 = new User { UserId = 2, Username = "dmytro", Email = "dmytro@gmail.com", PasswordHash = "hash456", Role = "admin" };

        var city1 = new City { CityId = 1, Name = "Київ", Description = "Столиця", Category = "Столиця" };
        var city2 = new City { CityId = 2, Name = "Львів", Description = "Культурна столиця", Category = "Туристичне" };

        var loc1 = new Location { LocationId = 1, CityId = 1, Name = "Хрещатик", Address = "Хрещатик, 1", Category = "Вулиця" };
        var loc2 = new Location { LocationId = 2, CityId = 2, Name = "Площа Ринок", Address = "Площа Ринок, 1", Category = "Визначне місце" };

        var trip1 = new Trip { TripId = 1, UserId = 1, Name = "Вікенд у Львові", Status = "active", CreatedAt = DateTime.UtcNow };

        context.Users.AddRange(user1, user2);
        context.Cities.AddRange(city1, city2);
        context.Locations.AddRange(loc1, loc2);
        context.Trips.Add(trip1);
        context.SaveChanges();
    }
}
