using HotelBooking.Modules.Property.Domain.Hotels;
using HotelBooking.Modules.Property.Domain.Hotels.ValueObjects;
using HotelBooking.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HotelBooking.Modules.Property.Infrastructure.Persistence.Configurations;

internal sealed class HotelConfiguration : IEntityTypeConfiguration<Hotel>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Hotel> builder)
    {
        builder.ToTable("hotels");

        builder.HasKey(hotel => hotel.Id)
            .HasName("pk_hotels");

        builder.Property(hotel => hotel.Id)
            .HasConversion(
                hotelId => hotelId.Value,
                value => HotelId.From(value))
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(hotel => hotel.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(hotel => hotel.Slug)
            .HasColumnName("slug")
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(hotel => hotel.Slug)
            .IsUnique()
            .HasDatabaseName("ux_hotels_slug");

        builder.Property(hotel => hotel.Status)
            .HasConversion<string>()
            .HasColumnName("status")
            .HasMaxLength(32)
            .IsRequired();

        var starRatingConverter = new ValueConverter<StarRating?, int?>(
            starRating => starRating == null ? null : starRating.Value,
            value => value == null ? null : StarRating.Create(value.Value));

        builder.Property(hotel => hotel.StarRating)
            .HasConversion(starRatingConverter)
            .HasColumnName("star_rating");

        builder.Property(hotel => hotel.TimeZoneId)
            .HasColumnName("time_zone_id")
            .IsRequired();

        builder.Property(hotel => hotel.SellingCurrency)
            .HasConversion(
                currency => currency.Code,
                code => Currency.FromCode(code))
            .HasColumnName("selling_currency")
            .HasMaxLength(3)
            .IsFixedLength()
            .IsRequired();

        builder.Property(hotel => hotel.DefaultLanguage)
            .HasColumnName("default_language")
            .IsRequired();

        builder.Property(hotel => hotel.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(hotel => hotel.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone");

        ConfigureAddress(builder);
        ConfigureGeoLocation(builder);
        ConfigureContactInfo(builder);
        ConfigureCheckInPolicy(builder);
        ConfigureHotelPolicy(builder);

        builder.Ignore(hotel => hotel.DomainEvents);
    }

    private static void ConfigureAddress(EntityTypeBuilder<Hotel> builder)
    {
        builder.ComplexProperty(hotel => hotel.Address, address =>
        {
            address.IsRequired(false);
            address.Property(value => value.CountryCode).HasColumnName("address_country_code");
            address.Property(value => value.Region).HasColumnName("address_region");
            address.Property(value => value.City).HasColumnName("address_city");
            address.Property(value => value.StreetAddress).HasColumnName("address_street");
            address.Property(value => value.DetailAddress).HasColumnName("address_detail");
            address.Property(value => value.PostalCode).HasColumnName("address_postal_code");
        });
    }

    private static void ConfigureGeoLocation(EntityTypeBuilder<Hotel> builder)
    {
        builder.ComplexProperty(hotel => hotel.GeoLocation, geoLocation =>
        {
            geoLocation.IsRequired(false);
            geoLocation.Property(value => value.Latitude).HasColumnName("latitude");
            geoLocation.Property(value => value.Longitude).HasColumnName("longitude");
        });
    }

    private static void ConfigureContactInfo(EntityTypeBuilder<Hotel> builder)
    {
        builder.ComplexProperty(hotel => hotel.ContactInfo, contactInfo =>
        {
            contactInfo.IsRequired(false);
            contactInfo.ToJson("contact_info");
        });
    }

    private static void ConfigureCheckInPolicy(EntityTypeBuilder<Hotel> builder)
    {
        builder.ComplexProperty(hotel => hotel.CheckInPolicy, checkInPolicy =>
        {
            checkInPolicy.IsRequired(false);
            checkInPolicy.Property(value => value.CheckInFrom).HasColumnName("check_in_from");
            checkInPolicy.Property(value => value.CheckInUntil).HasColumnName("check_in_until");
            checkInPolicy.Property(value => value.CheckOutUntil).HasColumnName("check_out_until");
            checkInPolicy.Property(value => value.AllowsEarlyCheckIn).HasColumnName("allows_early_check_in");
            checkInPolicy.Property(value => value.AllowsLateCheckOut).HasColumnName("allows_late_check_out");
        });
    }

    private static void ConfigureHotelPolicy(EntityTypeBuilder<Hotel> builder)
    {
        builder.ComplexProperty(hotel => hotel.HotelPolicy, hotelPolicy =>
        {
            hotelPolicy.IsRequired(false);
            hotelPolicy.Property(value => value.AllowsSmoking).HasColumnName("allows_smoking");
            hotelPolicy.Property(value => value.AllowsPets).HasColumnName("allows_pets");
            hotelPolicy.Property(value => value.AllowsChildren).HasColumnName("allows_children");
            hotelPolicy.Property(value => value.MinimumCheckInAge).HasColumnName("minimum_check_in_age");
            hotelPolicy.Property(value => value.RequiresDeposit).HasColumnName("requires_deposit");
        });
    }
}
