using Microsoft.EntityFrameworkCore;
using TripPlanner.Domain.Entities;

namespace TripPlanner.Infrastructure.Data;

public class TripPlannerDbContext : DbContext
{
    public TripPlannerDbContext(DbContextOptions<TripPlannerDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<City> Cities { get; set; }
    public virtual DbSet<Location> Locations { get; set; }
    public virtual DbSet<Trip> Trips { get; set; }
    public virtual DbSet<TripLocation> TripLocations { get; set; }
    public virtual DbSet<Review> Reviews { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Location -> City
        modelBuilder.Entity<Location>()
            .HasOne(e => e.City)
            .WithMany(c => c.Locations)
            .HasForeignKey(e => e.CityId)
            .OnDelete(DeleteBehavior.Cascade);

        // Trip -> User
        modelBuilder.Entity<Trip>()
            .HasOne(e => e.User)
            .WithMany(u => u.Trips)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // TripLocation -> Trip
        modelBuilder.Entity<TripLocation>()
            .HasOne(e => e.Trip)
            .WithMany(t => t.TripLocations)
            .HasForeignKey(e => e.TripId)
            .OnDelete(DeleteBehavior.Cascade);

        // TripLocation -> Location
        modelBuilder.Entity<TripLocation>()
            .HasOne(e => e.Location)
            .WithMany(l => l.TripLocations)
            .HasForeignKey(e => e.LocationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Review -> User
        modelBuilder.Entity<Review>()
            .HasOne(e => e.User)
            .WithMany(u => u.Reviews)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Review -> Location
        modelBuilder.Entity<Review>()
            .HasOne(e => e.Location)
            .WithMany(l => l.Reviews)
            .HasForeignKey(e => e.LocationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}