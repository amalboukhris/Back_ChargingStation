using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ChargingStation.Migrations
{
    public partial class _45233 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. First ensure all Ids are unique and not null
            migrationBuilder.Sql(@"
                WITH numbered_rows AS (
                    SELECT ctid, 
                           ROW_NUMBER() OVER () + (SELECT COALESCE(MAX(""Id""), 0) FROM ""Connectors"") AS new_id
                    FROM ""Connectors""
                )
                UPDATE ""Connectors"" c
                SET ""Id"" = nr.new_id
                FROM numbered_rows nr
                WHERE c.ctid = nr.ctid
            ");

            // 2. Drop existing foreign key constraints that reference the old key
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Connectors_ChargePointId_ConnectorId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Connectors_ChargePointId_ConnectorId",
                table: "Transactions");

            // 3. Drop the existing primary key
            migrationBuilder.DropPrimaryKey(
                name: "PK_Connectors",
                table: "Connectors");

            // 4. Add the new primary key
            migrationBuilder.AddPrimaryKey(
                name: "PK_Connectors",
                table: "Connectors",
                column: "Id");

            // 5. Recreate foreign keys using the new key
            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Connectors_ConnectorId",
                table: "Reservations",
                column: "ConnectorId",
                principalTable: "Connectors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Connectors_ConnectorId",
                table: "Transactions",
                column: "ConnectorId",
                principalTable: "Connectors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Connectors_ConnectorId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Connectors_ConnectorId",
                table: "Transactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Connectors",
                table: "Connectors");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Connectors",
                table: "Connectors",
                columns: new[] { "ChargePointId", "ConnectorId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Connectors_ChargePointId_ConnectorId",
                table: "Reservations",
                columns: new[] { "ChargePointId", "ConnectorId" },
                principalTable: "Connectors",
                principalColumns: new[] { "ChargePointId", "ConnectorId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Connectors_ChargePointId_ConnectorId",
                table: "Transactions",
                columns: new[] { "ChargePointId", "ConnectorId" },
                principalTable: "Connectors",
                principalColumns: new[] { "ChargePointId", "ConnectorId" },
                onDelete: ReferentialAction.Restrict);
        }
    }
}