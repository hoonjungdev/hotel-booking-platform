using HotelBooking.Modules.Property.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace HotelBooking.Modules.Property;

/// <summary>Registers Property module runtime services.</summary>
public static class PropertyModuleServiceCollectionExtensions
{
    /// <summary>
    /// Registers Property persistence against the shared PostgreSQL database and module-owned schema.
    /// </summary>
    /// <param name="services">The application service collection.</param>
    /// <param name="connectionString">The shared PostgreSQL database connection string.</param>
    /// <returns>The same service collection for registration chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when the connection string is missing.</exception>
    public static IServiceCollection AddPropertyModule(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<PropertyDbContext>(options =>
            PropertyPersistence.Configure(options, connectionString));

        return services;
    }
}
