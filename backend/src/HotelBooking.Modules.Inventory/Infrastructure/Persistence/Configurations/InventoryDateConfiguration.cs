using HotelBooking.Modules.Inventory.Domain.InventoryDates;
using HotelBooking.Modules.Inventory.Domain.References;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBooking.Modules.Inventory.Infrastructure.Persistence.Configurations;

internal sealed class InventoryDateConfiguration : IEntityTypeConfiguration<InventoryDate>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<InventoryDate> builder)
    {
        builder.ToTable("inventory_dates", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "ck_inventory_dates_quantities_non_negative",
                "total_quantity >= 0 AND held_quantity >= 0 " +
                "AND booked_quantity >= 0 AND closed_quantity >= 0");
            tableBuilder.HasCheckConstraint(
                "ck_inventory_dates_committed_quantity_within_total",
                "held_quantity + booked_quantity + closed_quantity <= total_quantity");
        });

        builder.HasKey(inventoryDate => inventoryDate.Id)
            .HasName("pk_inventory_dates");

        builder.Property(inventoryDate => inventoryDate.Id)
            .HasConversion(
                inventoryDateId => inventoryDateId.Value,
                value => InventoryDateId.From(value))
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(inventoryDate => inventoryDate.HotelId)
            .HasConversion(
                hotelId => hotelId.Value,
                value => HotelId.From(value))
            .HasColumnName("hotel_id")
            .IsRequired();

        builder.Property(inventoryDate => inventoryDate.RoomTypeId)
            .HasConversion(
                roomTypeId => roomTypeId.Value,
                value => RoomTypeId.From(value))
            .HasColumnName("room_type_id")
            .IsRequired();

        builder.Property(inventoryDate => inventoryDate.OccupiedDate)
            .HasColumnName("occupied_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(inventoryDate => inventoryDate.TotalQuantity)
            .HasColumnName("total_quantity")
            .IsRequired();

        builder.Property(inventoryDate => inventoryDate.HeldQuantity)
            .HasColumnName("held_quantity")
            .IsRequired();

        builder.Property(inventoryDate => inventoryDate.BookedQuantity)
            .HasColumnName("booked_quantity")
            .IsRequired();

        builder.Property(inventoryDate => inventoryDate.ClosedQuantity)
            .HasColumnName("closed_quantity")
            .IsRequired();

        builder.HasIndex(inventoryDate => new
        {
            inventoryDate.HotelId,
            inventoryDate.RoomTypeId,
            inventoryDate.OccupiedDate
        })
            .IsUnique()
            .HasDatabaseName("ux_inventory_dates_hotel_id_room_type_id_occupied_date");

        builder.Ignore(inventoryDate => inventoryDate.AvailableQuantity);
        builder.Ignore(inventoryDate => inventoryDate.DomainEvents);
    }
}
