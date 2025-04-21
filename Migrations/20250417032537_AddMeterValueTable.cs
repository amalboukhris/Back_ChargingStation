using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ChargingStation.Migrations
{
    /// <inheritdoc />
    public partial class AddMeterValueTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_ChargePoints_ChargePointId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Connectors_ConnectorId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Connectors_ConnectorId1",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_ChargePoints_ChargePointId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_ChargePointId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_ChargePointId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_ConnectorId",
                table: "Reservations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Connectors",
                table: "Connectors");

            migrationBuilder.DropIndex(
                name: "IX_Connectors_ChargePointId",
                table: "Connectors");

            migrationBuilder.DropColumn(
                name: "StartTimestamp",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "MeterValue",
                table: "Connectors");

            migrationBuilder.DropColumn(
                name: "MeterValueTimestamp",
                table: "Connectors");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "Connectors");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "ChargePoints");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "ChargePoints");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "ChargePoints");

            migrationBuilder.RenameColumn(
                name: "ConnectorId1",
                table: "Reservations",
                newName: "AdminId");

            migrationBuilder.RenameIndex(
                name: "IX_Reservations_ConnectorId1",
                table: "Reservations",
                newName: "IX_Reservations_AdminId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValueSql: "NOW() AT TIME ZONE 'UTC'");

            migrationBuilder.AddColumn<int>(
                name: "ParentUserId",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RfidExpiryDate",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RfidTag",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Transactions",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "MeterStop",
                table: "Transactions",
                type: "integer",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MeterStart",
                table: "Transactions",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IdTag",
                table: "Transactions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "ReservationId",
                table: "Transactions",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Reservations",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Reservations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Connectors",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdated",
                table: "Connectors",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<double>(
                name: "MaxPower",
                table: "Connectors",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Connectors",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "ChargePointId",
                table: "ChargePoints",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ChargePoints",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DiagnosticsStatus",
                table: "ChargePoints",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirmwareStatus",
                table: "ChargePoints",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastDiagnosticsTime",
                table: "ChargePoints",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastFirmwareUpdate",
                table: "ChargePoints",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "StationId",
                table: "ChargePoints",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Connectors",
                table: "Connectors",
                columns: new[] { "ChargePointId", "ConnectorId" });

            migrationBuilder.CreateTable(
                name: "MeterValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChargePointId = table.Column<int>(type: "integer", nullable: false),
                    ConnectorId = table.Column<int>(type: "integer", nullable: false),
                    TransactionId = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Value = table.Column<decimal>(type: "numeric", nullable: false),
                    Measurand = table.Column<string>(type: "text", nullable: false),
                    Unit = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeterValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeterValues_ChargePoints_ChargePointId",
                        column: x => x.ChargePointId,
                        principalTable: "ChargePoints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Stations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ChargePointId_ConnectorId",
                table: "Transactions",
                columns: new[] { "ChargePointId", "ConnectorId" });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ReservationId",
                table: "Transactions",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ChargePointId_ConnectorId",
                table: "Reservations",
                columns: new[] { "ChargePointId", "ConnectorId" });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ReservationCode",
                table: "Reservations",
                column: "ReservationCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChargePoints_ChargePointId",
                table: "ChargePoints",
                column: "ChargePointId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChargePoints_StationId",
                table: "ChargePoints",
                column: "StationId");

            migrationBuilder.CreateIndex(
                name: "IX_MeterValues_ChargePointId",
                table: "MeterValues",
                column: "ChargePointId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChargePoints_Stations_StationId",
                table: "ChargePoints",
                column: "StationId",
                principalTable: "Stations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_ChargePoints_ChargePointId",
                table: "Reservations",
                column: "ChargePointId",
                principalTable: "ChargePoints",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Connectors_ChargePointId_ConnectorId",
                table: "Reservations",
                columns: new[] { "ChargePointId", "ConnectorId" },
                principalTable: "Connectors",
                principalColumns: new[] { "ChargePointId", "ConnectorId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Users_AdminId",
                table: "Reservations",
                column: "AdminId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_ChargePoints_ChargePointId",
                table: "Transactions",
                column: "ChargePointId",
                principalTable: "ChargePoints",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Connectors_ChargePointId_ConnectorId",
                table: "Transactions",
                columns: new[] { "ChargePointId", "ConnectorId" },
                principalTable: "Connectors",
                principalColumns: new[] { "ChargePointId", "ConnectorId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Reservations_ReservationId",
                table: "Transactions",
                column: "ReservationId",
                principalTable: "Reservations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChargePoints_Stations_StationId",
                table: "ChargePoints");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_ChargePoints_ChargePointId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Connectors_ChargePointId_ConnectorId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Users_AdminId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_ChargePoints_ChargePointId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Connectors_ChargePointId_ConnectorId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Reservations_ReservationId",
                table: "Transactions");

            migrationBuilder.DropTable(
                name: "MeterValues");

            migrationBuilder.DropTable(
                name: "Stations");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_ChargePointId_ConnectorId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_ReservationId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_ChargePointId_ConnectorId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_ReservationCode",
                table: "Reservations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Connectors",
                table: "Connectors");

            migrationBuilder.DropIndex(
                name: "IX_ChargePoints_ChargePointId",
                table: "ChargePoints");

            migrationBuilder.DropIndex(
                name: "IX_ChargePoints_StationId",
                table: "ChargePoints");

            migrationBuilder.DropColumn(
                name: "ParentUserId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RfidExpiryDate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RfidTag",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ReservationId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "LastUpdated",
                table: "Connectors");

            migrationBuilder.DropColumn(
                name: "MaxPower",
                table: "Connectors");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Connectors");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ChargePoints");

            migrationBuilder.DropColumn(
                name: "DiagnosticsStatus",
                table: "ChargePoints");

            migrationBuilder.DropColumn(
                name: "FirmwareStatus",
                table: "ChargePoints");

            migrationBuilder.DropColumn(
                name: "LastDiagnosticsTime",
                table: "ChargePoints");

            migrationBuilder.DropColumn(
                name: "LastFirmwareUpdate",
                table: "ChargePoints");

            migrationBuilder.DropColumn(
                name: "StationId",
                table: "ChargePoints");

            migrationBuilder.RenameColumn(
                name: "AdminId",
                table: "Reservations",
                newName: "ConnectorId1");

            migrationBuilder.RenameIndex(
                name: "IX_Reservations_AdminId",
                table: "Reservations",
                newName: "IX_Reservations_ConnectorId1");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "NOW() AT TIME ZONE 'UTC'",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Transactions",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<double>(
                name: "MeterStop",
                table: "Transactions",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "MeterStart",
                table: "Transactions",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "IdTag",
                table: "Transactions",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartTimestamp",
                table: "Transactions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Reservations",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Connectors",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<double>(
                name: "MeterValue",
                table: "Connectors",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MeterValueTimestamp",
                table: "Connectors",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TransactionId",
                table: "Connectors",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ChargePointId",
                table: "ChargePoints",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ChargePoints",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "ChargePoints",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "ChargePoints",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Connectors",
                table: "Connectors",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ChargePointId",
                table: "Transactions",
                column: "ChargePointId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ChargePointId",
                table: "Reservations",
                column: "ChargePointId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ConnectorId",
                table: "Reservations",
                column: "ConnectorId");

            migrationBuilder.CreateIndex(
                name: "IX_Connectors_ChargePointId",
                table: "Connectors",
                column: "ChargePointId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_ChargePoints_ChargePointId",
                table: "Reservations",
                column: "ChargePointId",
                principalTable: "ChargePoints",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Connectors_ConnectorId",
                table: "Reservations",
                column: "ConnectorId",
                principalTable: "Connectors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Connectors_ConnectorId1",
                table: "Reservations",
                column: "ConnectorId1",
                principalTable: "Connectors",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_ChargePoints_ChargePointId",
                table: "Transactions",
                column: "ChargePointId",
                principalTable: "ChargePoints",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
