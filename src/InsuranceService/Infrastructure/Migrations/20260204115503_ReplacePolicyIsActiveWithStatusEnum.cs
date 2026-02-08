using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReplacePolicyIsActiveWithStatusEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Policies");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Policies",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Policies");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Policies",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
