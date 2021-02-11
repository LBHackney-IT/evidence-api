using Microsoft.EntityFrameworkCore.Migrations;

namespace EvidenceApi.Migrations
{
    public partial class AddStateColumnToEvidenceRequest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "state",
                table: "evidence_requests",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
                UPDATE communications
                SET reason =
                    CASE reason
                        WHEN 'EvidenceRejected' THEN 0
                        WHEN 'Reminder' THEN 1
                        WHEN 'EvidenceRequest' THEN 2
                    END;
                ALTER TABLE communications ALTER COLUMN reason TYPE integer USING reason::integer;
                ALTER TABLE communications ALTER COLUMN reason SET NOT NULL;
                ALTER TABLE communications ALTER COLUMN reason DROP DEFAULT;
            ");

            migrationBuilder.Sql(@"
                UPDATE communications
                SET delivery_method =
                    CASE delivery_method
                        WHEN 'Sms' THEN 0
                        WHEN 'Email' THEN 1
                    END;
                ALTER TABLE communications ALTER COLUMN delivery_method TYPE integer USING delivery_method::integer;
                ALTER TABLE communications ALTER COLUMN delivery_method SET NOT NULL;
                ALTER TABLE communications ALTER COLUMN delivery_method DROP DEFAULT;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "state",
                table: "evidence_requests");

            migrationBuilder.DropColumn(
                name: "user_requested_by",
                table: "evidence_requests");

            migrationBuilder.AlterColumn<string>(
                name: "reason",
                table: "communications",
                type: "text",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<string>(
                name: "delivery_method",
                table: "communications",
                type: "text",
                nullable: true,
                oldClrType: typeof(int));
        }
    }
}
