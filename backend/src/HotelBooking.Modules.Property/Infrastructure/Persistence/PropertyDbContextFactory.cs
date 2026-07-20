using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HotelBooking.Modules.Property.Infrastructure.Persistence;

/// <summary>
/// Creates the Property context for EF Core migration commands without starting the API host.
/// </summary>
internal sealed class PropertyDbContextFactory : IDesignTimeDbContextFactory<PropertyDbContext>
{
    /// <summary>Initializes the design-time factory discovered by EF Core tooling.</summary>
    public PropertyDbContextFactory()
    {
    }

    /// <inheritdoc />
    public PropertyDbContext CreateDbContext(string[] args)
    {
        string connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__hotelbooking") ??
            "Host=localhost;Port=15432;Database=hotelbooking;Username=hotelbooking;Password=hotelbooking";

        var optionsBuilder = new DbContextOptionsBuilder<PropertyDbContext>();

        PropertyPersistence.Configure(optionsBuilder, connectionString);

        return new PropertyDbContext(optionsBuilder.Options);
    }
}
