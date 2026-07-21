using HotelBooking.Modules.Inventory.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace HotelBooking.Modules.Inventory;

/// <summary>Registers Inventory module runtime services.</summary>
public static class InventoryModuleServiceCollectionExtensions
{
    /// <summary>
    /// Registers Inventory persistence against the shared PostgreSQL database and module-owned schema.
    /// </summary>
    /// <param name="services">The application service collection.</param>
    /// <param name="connectionString">The shared PostgreSQL database connection string.</param>
    /// <returns>The same service collection for registration chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when the connection string is missing.</exception>
    public static IServiceCollection AddInventoryModule(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<InventoryDbContext>(options =>
            InventoryPersistence.Configure(options, connectionString));

        return services;
    }
}
