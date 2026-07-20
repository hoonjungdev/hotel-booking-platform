using HotelBooking.Modules.Property.Domain.Hotels;
using HotelBooking.Modules.Property.Domain.RoomTypes;
using HotelBooking.Modules.Property.Domain.RoomTypes.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBooking.Modules.Property.Infrastructure.Persistence.Configurations;

internal sealed class RoomTypeConfiguration : IEntityTypeConfiguration<RoomType>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<RoomType> builder)
    {
        builder.ToTable("room_types", tableBuilder =>
            tableBuilder.HasCheckConstraint(
                "ck_room_types_occupancy_valid",
                "max_adults >= 1 AND max_children >= 0 AND max_occupancy >= 1 " +
                "AND max_adults <= max_occupancy AND max_children <= max_occupancy " +
                "AND max_occupancy <= max_adults + max_children"));

        builder.HasKey(roomType => roomType.Id)
            .HasName("pk_room_types");

        builder.Property(roomType => roomType.Id)
            .HasConversion(
                roomTypeId => roomTypeId.Value,
                value => RoomTypeId.From(value))
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(roomType => roomType.HotelId)
            .HasConversion(
                hotelId => hotelId.Value,
                value => HotelId.From(value))
            .HasColumnName("hotel_id")
            .IsRequired();

        builder.HasOne<Hotel>()
            .WithMany()
            .HasForeignKey(roomType => roomType.HotelId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_room_types_hotels_hotel_id");

        builder.Property(roomType => roomType.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(roomType => roomType.Code)
            .HasConversion(
                code => code.Value,
                value => RoomTypeCode.Create(value))
            .HasColumnName("code")
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(roomType => new { roomType.HotelId, roomType.Code })
            .IsUnique()
            .HasDatabaseName("ux_room_types_hotel_id_code");

        builder.Property(roomType => roomType.Status)
            .HasConversion<string>()
            .HasColumnName("status")
            .HasMaxLength(32)
            .IsRequired();

        builder.ComplexProperty(roomType => roomType.Occupancy, occupancy =>
        {
            occupancy.Property(value => value.MaxAdults).HasColumnName("max_adults");
            occupancy.Property(value => value.MaxChildren).HasColumnName("max_children");
            occupancy.Property(value => value.MaxOccupancy).HasColumnName("max_occupancy");
        });

        builder.Property(roomType => roomType.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(roomType => roomType.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone");

        builder.OwnsMany<BedComposition>("_bedCompositions", bedComposition =>
        {
            bedComposition.ToTable("room_type_bed_compositions", tableBuilder =>
                tableBuilder.HasCheckConstraint(
                    "ck_room_type_bed_compositions_quantity_positive",
                    "quantity >= 1"));

            bedComposition.WithOwner()
                .HasForeignKey("room_type_id")
                .HasConstraintName("fk_room_type_bed_compositions_room_types_room_type_id");

            bedComposition.Property<long>("id")
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            bedComposition.HasKey("id")
                .HasName("pk_room_type_bed_compositions");

            bedComposition.Property(value => value.BedType)
                .HasConversion<string>()
                .HasColumnName("bed_type")
                .HasMaxLength(32)
                .IsRequired();

            bedComposition.Property(value => value.Quantity)
                .HasColumnName("quantity")
                .IsRequired();

            bedComposition.HasIndex("room_type_id")
                .HasDatabaseName("ix_room_type_bed_compositions_room_type_id");
        });

        builder.Navigation("_bedCompositions")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(roomType => roomType.BedCompositions);
        builder.Ignore(roomType => roomType.IsSellable);
        builder.Ignore(roomType => roomType.DomainEvents);
    }
}
