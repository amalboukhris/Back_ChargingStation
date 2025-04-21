using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ChargingStation.Migrations
{
    /// <inheritdoc />
    public partial class _452335 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Drop existing constraints if they exist
            migrationBuilder.Sql(@"
            DO $$
            BEGIN
                IF EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_Reservations_Connectors_ChargePointId_ConnectorId') THEN
                    ALTER TABLE ""Reservations"" DROP CONSTRAINT ""FK_Reservations_Connectors_ChargePointId_ConnectorId"";
                END IF;
                
                IF EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_Transactions_Connectors_ChargePointId_ConnectorId') THEN
                    ALTER TABLE ""Transactions"" DROP CONSTRAINT ""FK_Transactions_Connectors_ChargePointId_ConnectorId"";
                END IF;
                
                IF EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_Connectors') THEN
                    ALTER TABLE ""Connectors"" DROP CONSTRAINT ""PK_Connectors"";
                END IF;
            END $$;
        ");

            // 2. Ensure the sequence exists and is properly configured
            migrationBuilder.Sql(@"
            CREATE SEQUENCE IF NOT EXISTS ""Connectors_Id_seq"";
            
            -- Reset the sequence to current max Id + 1
            SELECT setval('""Connectors_Id_seq""', COALESCE((SELECT MAX(""Id"") FROM ""Connectors""), 0) + 1;
            
            -- Set the column default to use the sequence
            ALTER TABLE ""Connectors"" 
            ALTER COLUMN ""Id"" SET DEFAULT nextval('""Connectors_Id_seq""'),
            ALTER COLUMN ""Id"" SET NOT NULL;
        ");

            // 3. Add the new primary key
            migrationBuilder.AddPrimaryKey(
                name: "PK_Connectors",
                table: "Connectors",
                column: "Id");

            // 4. Update foreign key references in Reservations
            migrationBuilder.Sql(@"
            -- Add temporary column
            ALTER TABLE ""Reservations"" ADD COLUMN IF NOT EXISTS ""ConnectorId_new"" integer;
            
            -- Populate with correct Ids
            UPDATE ""Reservations"" r
            SET ""ConnectorId_new"" = c.""Id""
            FROM ""Connectors"" c
            WHERE r.""ChargePointId"" = c.""ChargePointId"" AND r.""ConnectorId"" = c.""ConnectorId"";
            
            -- Drop old column and rename
            ALTER TABLE ""Reservations"" DROP COLUMN IF EXISTS ""ConnectorId"";
            ALTER TABLE ""Reservations"" RENAME COLUMN ""ConnectorId_new"" TO ""ConnectorId"";
            
            -- Add foreign key constraint
            ALTER TABLE ""Reservations""
            ADD CONSTRAINT ""FK_Reservations_Connectors_ConnectorId""
            FOREIGN KEY (""ConnectorId"") REFERENCES ""Connectors"" (""Id"") ON DELETE RESTRICT;
        ");

            // 5. Update foreign key references in Transactions
            migrationBuilder.Sql(@"
            -- Add temporary column
            ALTER TABLE ""Transactions"" ADD COLUMN IF NOT EXISTS ""ConnectorId_new"" integer;
            
            -- Populate with correct Ids
            UPDATE ""Transactions"" t
            SET ""ConnectorId_new"" = c.""Id""
            FROM ""Connectors"" c
            WHERE t.""ChargePointId"" = c.""ChargePointId"" AND t.""ConnectorId"" = c.""ConnectorId"";
            
            -- Drop old column and rename
            ALTER TABLE ""Transactions"" DROP COLUMN IF EXISTS ""ConnectorId"";
            ALTER TABLE ""Transactions"" RENAME COLUMN ""ConnectorId_new"" TO ""ConnectorId"";
            
            -- Add foreign key constraint
            ALTER TABLE ""Transactions""
            ADD CONSTRAINT ""FK_Transactions_Connectors_ConnectorId""
            FOREIGN KEY (""ConnectorId"") REFERENCES ""Connectors"" (""Id"") ON DELETE RESTRICT;
        ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse the changes if needed
            migrationBuilder.Sql(@"
            -- Drop new foreign keys
            ALTER TABLE ""Reservations"" DROP CONSTRAINT IF EXISTS ""FK_Reservations_Connectors_ConnectorId"";
            ALTER TABLE ""Transactions"" DROP CONSTRAINT IF EXISTS ""FK_Transactions_Connectors_ConnectorId"";
            
            -- Restore old columns
            ALTER TABLE ""Reservations"" ADD COLUMN IF NOT EXISTS ""ConnectorId_old"" integer;
            ALTER TABLE ""Transactions"" ADD COLUMN IF NOT EXISTS ""ConnectorId_old"" integer;
            
            -- Populate with old values
            UPDATE ""Reservations"" r
            SET ""ConnectorId_old"" = c.""ConnectorId""
            FROM ""Connectors"" c
            WHERE r.""ConnectorId"" = c.""Id"";
            
            UPDATE ""Transactions"" t
            SET ""ConnectorId_old"" = c.""ConnectorId""
            FROM ""Connectors"" c
            WHERE t.""ConnectorId"" = c.""Id"";
            
            -- Drop and rename columns
            ALTER TABLE ""Reservations"" DROP COLUMN IF EXISTS ""ConnectorId"";
            ALTER TABLE ""Reservations"" RENAME COLUMN ""ConnectorId_old"" TO ""ConnectorId"";
            
            ALTER TABLE ""Transactions"" DROP COLUMN IF EXISTS ""ConnectorId"";
            ALTER TABLE ""Transactions"" RENAME COLUMN ""ConnectorId_old"" TO ""ConnectorId"";
            
            -- Restore primary key
            ALTER TABLE ""Connectors"" DROP CONSTRAINT IF EXISTS ""PK_Connectors"";
            ALTER TABLE ""Connectors"" ADD PRIMARY KEY (""ChargePointId"", ""ConnectorId"");
            
            -- Restore foreign keys
            ALTER TABLE ""Reservations""
            ADD CONSTRAINT ""FK_Reservations_Connectors_ChargePointId_ConnectorId""
            FOREIGN KEY (""ChargePointId"", ""ConnectorId"") REFERENCES ""Connectors"" (""ChargePointId"", ""ConnectorId"") ON DELETE RESTRICT;
            
            ALTER TABLE ""Transactions""
            ADD CONSTRAINT ""FK_Transactions_Connectors_ChargePointId_ConnectorId""
            FOREIGN KEY (""ChargePointId"", ""ConnectorId"") REFERENCES ""Connectors"" (""ChargePointId"", ""ConnectorId"") ON DELETE RESTRICT;
            
            -- Remove sequence default
            ALTER TABLE ""Connectors"" ALTER COLUMN ""Id"" DROP DEFAULT;
        ");
        }
    }
}
