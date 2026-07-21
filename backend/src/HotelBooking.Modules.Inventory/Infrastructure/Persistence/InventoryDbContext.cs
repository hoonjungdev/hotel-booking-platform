using HotelBooking.Modules.Inventory.Domain.InventoryDates;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Modules.Inventory.Infrastructure.Persistence;

/// <summary>
/// Owns Inventory module persistence within the module-specific PostgreSQL schema.
/// </summary>
internal sealed class InventoryDbContext : DbContext
{
    /// <summary>
    /// Initializes the Inventory module database context with configured options.
    /// </summary>
    /// <param name="options">The provider and connection options for Inventory persistence.</param>
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
        : base(options)
    {
    }

    /// <summary>Gets the persisted Inventory Date aggregates owned by the Inventory module.</summary>
    internal DbSet<InventoryDate> InventoryDates => Set<InventoryDate>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(InventoryPersistence.Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);
    }
}
