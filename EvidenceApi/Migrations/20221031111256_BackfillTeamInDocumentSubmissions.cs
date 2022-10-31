using Microsoft.EntityFrameworkCore.Migrations;

namespace EvidenceApi.Migrations
{
    public partial class BackfillTeamInDocumentSubmissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE document_submissions
                SET team = subquery.team
                FROM (
                SELECT evidence_requests.id, evidence_requests.team
                FROM evidence_requests, document_submissions
                where document_submissions.evidence_request_id = evidence_requests.id
                    )
                AS subquery
                where document_submissions.team is null and document_submissions.evidence_request_id = subquery.id;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "team",
                table: "document_submissions");
            migrationBuilder.AddColumn<string>(
                name: "team",
                table: "document_submissions",
                nullable: true);

        }
    }
}
