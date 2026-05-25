using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnCS.Migrations
{
    /// <inheritdoc />
    public partial class AddExpectedSalary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExpectedSalary",
                table: "Users",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpectedSalary",
                table: "Users");
        }
    }
}
