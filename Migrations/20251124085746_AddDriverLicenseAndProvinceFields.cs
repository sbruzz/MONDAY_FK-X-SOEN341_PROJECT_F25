using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampusEvents.Migrations
{
    /// <inheritdoc />
    public partial class AddDriverLicenseAndProvinceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DriverLicenseNumber",
                table: "Drivers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Province",
                table: "Drivers",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DriverLicenseNumber",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "Province",
                table: "Drivers");
        }
    }
}
