using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HotelBooking.Modules.Property.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddRoomTypePersistence : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "room_types",
            schema: "property",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                hotel_id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                max_adults = table.Column<int>(type: "integer", nullable: false),
                max_children = table.Column<int>(type: "integer", nullable: false),
                max_occupancy = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_room_types", x => x.id);
                table.CheckConstraint("ck_room_types_occupancy_valid", "max_adults >= 1 AND max_children >= 0 AND max_occupancy >= 1 AND max_adults <= max_occupancy AND max_children <= max_occupancy AND max_occupancy <= max_adults + max_children");
                table.ForeignKey(
                    name: "fk_room_types_hotels_hotel_id",
                    column: x => x.hotel_id,
                    principalSchema: "property",
                    principalTable: "hotels",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "room_type_bed_compositions",
            schema: "property",
            columns: table => new
            {
                id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                bed_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                quantity = table.Column<int>(type: "integer", nullable: false),
                room_type_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_room_type_bed_compositions", x => x.id);
                table.CheckConstraint("ck_room_type_bed_compositions_quantity_positive", "quantity >= 1");
                table.ForeignKey(
                    name: "fk_room_type_bed_compositions_room_types_room_type_id",
                    column: x => x.room_type_id,
                    principalSchema: "property",
                    principalTable: "room_types",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_room_type_bed_compositions_room_type_id",
            schema: "property",
            table: "room_type_bed_compositions",
            column: "room_type_id");

        migrationBuilder.CreateIndex(
            name: "ux_room_types_hotel_id_code",
            schema: "property",
            table: "room_types",
            columns: new[] { "hotel_id", "code" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "room_type_bed_compositions",
            schema: "property");

        migrationBuilder.DropTable(
            name: "room_types",
            schema: "property");
    }
}
