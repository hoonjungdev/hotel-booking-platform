using HotelBooking.Modules.Property.Domain.Hotels;
using HotelBooking.Modules.Property.Domain.RoomTypes;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Modules.Property.Infrastructure.Persistence;

/// <summary>
/// Owns Property module persistence within the module-specific PostgreSQL schema.
/// </summary>
internal sealed class PropertyDbContext : DbContext
{
    /// <summary>
    /// Initializes the Property module database context with configured options.
    /// </summary>
    /// <param name="options">The provider and connection options for Property persistence.</param>
    public PropertyDbContext(DbContextOptions<PropertyDbContext> options)
        : base(options)
    {
    }

    /// <summary>Gets the persisted Hotel aggregates owned by the Property module.</summary>
    internal DbSet<Hotel> Hotels => Set<Hotel>();

    /// <summary>Gets the persisted Room Type aggregates owned by the Property module.</summary>
    internal DbSet<RoomType> RoomTypes => Set<RoomType>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(PropertyPersistence.Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PropertyDbContext).Assembly);
    }
}
