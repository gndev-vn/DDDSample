using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatalogAPI.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddProductAttributeDefinitions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductVariantAttributes_ProductVariantId_Name",
                table: "ProductVariantAttributes");

            migrationBuilder.AddColumn<Guid>(
                name: "AttributeId",
                table: "ProductVariantAttributes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductAttributeDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductAttributeDefinitions", x => x.Id);
                });

            migrationBuilder.Sql(
                """
                INSERT INTO ProductAttributeDefinitions (Id, Name, UpdatedAtUtc, CreatedAtUtc)
                SELECT NEWID(), source.Name, SYSUTCDATETIME(), SYSUTCDATETIME()
                FROM (
                    SELECT DISTINCT Name
                    FROM ProductVariantAttributes
                    WHERE Name IS NOT NULL
                ) AS source
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM ProductAttributeDefinitions target
                    WHERE target.Name = source.Name
                );
                """);

            migrationBuilder.Sql(
                """
                UPDATE attributes
                SET AttributeId = definitions.Id
                FROM ProductVariantAttributes attributes
                INNER JOIN ProductAttributeDefinitions definitions ON definitions.Name = attributes.Name
                WHERE attributes.AttributeId IS NULL;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "AttributeId",
                table: "ProductVariantAttributes",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariantAttributes_ProductVariantId_AttributeId",
                table: "ProductVariantAttributes",
                columns: new[] { "ProductVariantId", "AttributeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributeDefinitions_Name",
                table: "ProductAttributeDefinitions",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductAttributeDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_ProductVariantAttributes_ProductVariantId_AttributeId",
                table: "ProductVariantAttributes");

            migrationBuilder.DropColumn(
                name: "AttributeId",
                table: "ProductVariantAttributes");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariantAttributes_ProductVariantId_Name",
                table: "ProductVariantAttributes",
                columns: new[] { "ProductVariantId", "Name" });
        }
    }
}
