using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChargingStation.Migrations
{
    /// <inheritdoc />
    public partial class Initial72 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReservationUserId",
                table: "Bornes",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReservationUserId",
                table: "Bornes");
        }
    }
}
