using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelBooking.Modules.Inventory.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class InitialInventory : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "inventory");

        migrationBuilder.CreateTable(
            name: "inventory_dates",
            schema: "inventory",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                hotel_id = table.Column<Guid>(type: "uuid", nullable: false),
                room_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                occupied_date = table.Column<DateOnly>(type: "date", nullable: false),
                total_quantity = table.Column<int>(type: "integer", nullable: false),
                held_quantity = table.Column<int>(type: "integer", nullable: false),
                booked_quantity = table.Column<int>(type: "integer", nullable: false),
                closed_quantity = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_inventory_dates", x => x.id);
                table.CheckConstraint("ck_inventory_dates_committed_quantity_within_total", "held_quantity + booked_quantity + closed_quantity <= total_quantity");
                table.CheckConstraint("ck_inventory_dates_quantities_non_negative", "total_quantity >= 0 AND held_quantity >= 0 AND booked_quantity >= 0 AND closed_quantity >= 0");
            });

        migrationBuilder.CreateIndex(
            name: "ux_inventory_dates_hotel_id_room_type_id_occupied_date",
            schema: "inventory",
            table: "inventory_dates",
            columns: new[] { "hotel_id", "room_type_id", "occupied_date" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "inventory_dates",
            schema: "inventory");
    }
}
