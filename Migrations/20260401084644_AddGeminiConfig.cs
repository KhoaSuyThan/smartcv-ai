using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnCS.Migrations
{
    /// <inheritdoc />
    public partial class AddGeminiConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateTable(
                name: "GeminiConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    ApiKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModelName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Temperature = table.Column<double>(type: "float", nullable: false),
                    MaxOutputTokens = table.Column<int>(type: "int", nullable: false),
                    SystemInstruction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SkillPromptTemplate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SummaryPromptTemplate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GrammarPromptTemplate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserRateLimit = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeminiConfigs", x => x.Id);
                });

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
                name: "FK_Jobs_Companies_CompanyID",
                table: "Jobs");

            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Users_RecruiterID",
                table: "Jobs");

            migrationBuilder.DropTable(
                name: "GeminiConfigs");

            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "Users");

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
