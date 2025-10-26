using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampusEvents.Migrations
{
    /// <inheritdoc />
    public partial class AddExtendedUserFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "Users",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Position",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Program",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StudentId",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "YearOfStudy",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_OrganizationId",
                table: "Users",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Organizations_OrganizationId",
                table: "Users",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Organizations_OrganizationId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_OrganizationId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Position",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Program",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "YearOfStudy",
                table: "Users");
        }
    }
}
