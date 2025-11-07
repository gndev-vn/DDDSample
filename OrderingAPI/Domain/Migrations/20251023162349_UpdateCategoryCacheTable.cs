using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderingAPI.Database.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCategoryCacheTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Parent",
                schema: "ordering",
                table: "CategoryCaches");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Parent",
                schema: "ordering",
                table: "CategoryCaches",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
