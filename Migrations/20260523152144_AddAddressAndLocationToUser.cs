using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnCS.Migrations
{
    /// <inheritdoc />
    public partial class AddAddressAndLocationToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExpectedLocation",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            // migrationBuilder.CreateTable(
            //     name: "CVEmbeddings",
            //     columns: table => new
            //     {
            //         Id = table.Column<int>(type: "int", nullable: false)
            //             .Annotation("SqlServer:Identity", "1, 1"),
            //         ResumeID = table.Column<int>(type: "int", nullable: false),
            //         VectorJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //         UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_CVEmbeddings", x => x.Id);
            //         table.ForeignKey(
            //             name: "FK_CVEmbeddings_Resumes_ResumeID",
            //             column: x => x.ResumeID,
            //             principalTable: "Resumes",
            //             principalColumn: "ResumeID",
            //             onDelete: ReferentialAction.Cascade);
            //     });

            // migrationBuilder.CreateTable(
            //     name: "CVMatchResults",
            //     columns: table => new
            //     {
            //         Id = table.Column<int>(type: "int", nullable: false)
            //             .Annotation("SqlServer:Identity", "1, 1"),
            //         JobID = table.Column<int>(type: "int", nullable: false),
            //         ResumeID = table.Column<int>(type: "int", nullable: false),
            //         MatchScore = table.Column<int>(type: "int", nullable: false),
            //         MatchedSkills = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //         MissingSkills = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //         Suggestions = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //         Strengths = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //         Summary = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //         Recommendation = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //         AnalyzedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_CVMatchResults", x => x.Id);
            //         table.ForeignKey(
            //             name: "FK_CVMatchResults_Jobs_JobID",
            //             column: x => x.JobID,
            //             principalTable: "Jobs",
            //             principalColumn: "JobID",
            //             onDelete: ReferentialAction.Cascade);
            //         table.ForeignKey(
            //             name: "FK_CVMatchResults_Resumes_ResumeID",
            //             column: x => x.ResumeID,
            //             principalTable: "Resumes",
            //             principalColumn: "ResumeID",
            //             onDelete: ReferentialAction.Cascade);
            //     });

            // migrationBuilder.CreateIndex(
            //     name: "IX_CVEmbeddings_ResumeID",
            //     table: "CVEmbeddings",
            //     column: "ResumeID");

            // migrationBuilder.CreateIndex(
            //     name: "IX_CVMatchResults_JobID",
            //     table: "CVMatchResults",
            //     column: "JobID");

            // migrationBuilder.CreateIndex(
            //     name: "IX_CVMatchResults_ResumeID",
            //     table: "CVMatchResults",
            //     column: "ResumeID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CVEmbeddings");

            migrationBuilder.DropTable(
                name: "CVMatchResults");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ExpectedLocation",
                table: "Users");
        }
    }
}
