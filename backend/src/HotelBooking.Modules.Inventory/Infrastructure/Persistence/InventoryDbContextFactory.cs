using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HotelBooking.Modules.Inventory.Infrastructure.Persistence;

/// <summary>
/// Creates the Inventory context for EF Core migration commands without starting the API host.
/// </summary>
internal sealed class InventoryDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
{
    /// <summary>Initializes the design-time factory discovered by EF Core tooling.</summary>
    public InventoryDbContextFactory()
    {
    }

    /// <inheritdoc />
    public InventoryDbContext CreateDbContext(string[] args)
    {
        string connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__hotelbooking") ??
            "Host=localhost;Port=15432;Database=hotelbooking;Username=hotelbooking;Password=hotelbooking";

        var optionsBuilder = new DbContextOptionsBuilder<InventoryDbContext>();

        InventoryPersistence.Configure(optionsBuilder, connectionString);

        return new InventoryDbContext(optionsBuilder.Options);
    }
}
