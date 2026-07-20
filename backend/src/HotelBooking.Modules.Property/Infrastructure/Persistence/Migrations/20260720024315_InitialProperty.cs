using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelBooking.Modules.Property.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class InitialProperty : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "property");

        migrationBuilder.CreateTable(
            name: "hotels",
            schema: "property",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                star_rating = table.Column<int>(type: "integer", nullable: true),
                time_zone_id = table.Column<string>(type: "text", nullable: false),
                selling_currency = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                default_language = table.Column<string>(type: "text", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                address_city = table.Column<string>(type: "text", nullable: true),
                address_country_code = table.Column<string>(type: "text", nullable: true),
                address_detail = table.Column<string>(type: "text", nullable: true),
                address_postal_code = table.Column<string>(type: "text", nullable: true),
                address_region = table.Column<string>(type: "text", nullable: true),
                address_street = table.Column<string>(type: "text", nullable: true),
                allows_early_check_in = table.Column<bool>(type: "boolean", nullable: true),
                allows_late_check_out = table.Column<bool>(type: "boolean", nullable: true),
                check_in_from = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                check_in_until = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                check_out_until = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                latitude = table.Column<decimal>(type: "numeric", nullable: true),
                longitude = table.Column<decimal>(type: "numeric", nullable: true),
                allows_children = table.Column<bool>(type: "boolean", nullable: true),
                allows_pets = table.Column<bool>(type: "boolean", nullable: true),
                allows_smoking = table.Column<bool>(type: "boolean", nullable: true),
                minimum_check_in_age = table.Column<int>(type: "integer", nullable: true),
                requires_deposit = table.Column<bool>(type: "boolean", nullable: true),
                contact_info = table.Column<string>(type: "jsonb", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_hotels", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ux_hotels_slug",
            schema: "property",
            table: "hotels",
            column: "slug",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "hotels",
            schema: "property");
    }
}
