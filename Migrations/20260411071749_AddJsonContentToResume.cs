using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnCS.Migrations
{
    /// <inheritdoc />
    public partial class AddJsonContentToResume : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AILogs_Users_UserID",
                table: "AILogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Companies_CompanyID",
                table: "Jobs");

            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Users_RecruiterID",
                table: "Jobs");

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPro",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Templates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsProOnly",
                table: "Templates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "JsonContent",
                table: "Resumes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "RecruiterID",
                table: "Jobs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CompanyID",
                table: "Jobs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "UserID",
                table: "AILogs",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "GeminiConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApiKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChatbotApiKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModelName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Temperature = table.Column<double>(type: "float", nullable: false),
                    MaxOutputTokens = table.Column<int>(type: "int", nullable: false),
                    ProModelName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProTemperature = table.Column<double>(type: "float", nullable: false),
                    ProMaxOutputTokens = table.Column<int>(type: "int", nullable: false),
                    SystemInstruction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SkillTemplate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SummaryTemplate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GrammarTemplate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserRateLimit = table.Column<int>(type: "int", nullable: false),
                    ProUserRateLimit = table.Column<int>(type: "int", nullable: false),
                    TotalTokensUsed = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeminiConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UpgradeRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    EvidenceImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DecisionDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UpgradeRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UpgradeRequests_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "GeminiConfigs",
                columns: new[] { "Id", "ApiKey", "ChatbotApiKey", "GrammarTemplate", "MaxOutputTokens", "ModelName", "ProMaxOutputTokens", "ProModelName", "ProTemperature", "ProUserRateLimit", "SkillTemplate", "SummaryTemplate", "SystemInstruction", "Temperature", "TotalTokensUsed", "UserRateLimit" },
                values: new object[] { 1, "", null, null, 2048, "gemini-2.5-flash", 4096, "gemini-2.5-pro", 0.90000000000000002, 50, null, null, "Bạn là trợ lý ảo hỗ trợ đánh giá CV.", 0.69999999999999996, 0L, 10 });

            migrationBuilder.CreateIndex(
                name: "IX_UpgradeRequests_UserID",
                table: "UpgradeRequests",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_AILogs_Users_UserID",
                table: "AILogs",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Companies_CompanyID",
                table: "Jobs",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "CompanyID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Users_RecruiterID",
                table: "Jobs",
                column: "RecruiterID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AILogs_Users_UserID",
                table: "AILogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Companies_CompanyID",
                table: "Jobs");

            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Users_RecruiterID",
                table: "Jobs");

            migrationBuilder.DropTable(
                name: "GeminiConfigs");

            migrationBuilder.DropTable(
                name: "UpgradeRequests");

            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsPro",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "IsProOnly",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "JsonContent",
                table: "Resumes");

            migrationBuilder.AlterColumn<int>(
                name: "RecruiterID",
                table: "Jobs",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "CompanyID",
                table: "Jobs",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "UserID",
                table: "AILogs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AILogs_Users_UserID",
                table: "AILogs",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Companies_CompanyID",
                table: "Jobs",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "CompanyID");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Users_RecruiterID",
                table: "Jobs",
                column: "RecruiterID",
                principalTable: "Users",
                principalColumn: "UserID");
        }
    }
}
