using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnCS.Migrations
{
    /// <inheritdoc />
    public partial class AddProExpirationDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ProExpirationDate",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChatbotSystemInstruction",
                table: "GeminiConfigs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GroqApiKey",
                table: "GeminiConfigs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Industry",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApiProvider",
                table: "AILogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "GeminiConfigs",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ChatbotSystemInstruction", "GroqApiKey" },
                values: new object[] { null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProExpirationDate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ChatbotSystemInstruction",
                table: "GeminiConfigs");

            migrationBuilder.DropColumn(
                name: "GroqApiKey",
                table: "GeminiConfigs");

            migrationBuilder.DropColumn(
                name: "Industry",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "ApiProvider",
                table: "AILogs");
        }
    }
}
