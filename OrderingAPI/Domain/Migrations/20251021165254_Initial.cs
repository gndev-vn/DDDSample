using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderingAPI.Database.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ordering");

            migrationBuilder.CreateTable(
                name: "CategoryCaches",
                schema: "ordering",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Parent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryCaches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                schema: "ordering",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ShippingAddressLine1 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ShippingAddressLine2 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ShippingAddressCity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ShippingAddressProvince = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ShippingAddressDistrict = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ShippingAddressWard = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BillingAddressLine1 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BillingAddressLine2 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BillingAddressCity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BillingAddressProvince = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BillingAddressDistrict = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BillingAddressWard = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductCaches",
                schema: "ordering",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Sku = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    CurrentPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastUpdatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCaches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderLines",
                schema: "ordering",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SkuValue = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    PriceAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriceCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderLines_Orders_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "ordering",
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CategoryCaches_Name",
                schema: "ordering",
                table: "CategoryCaches",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryCaches_ParentId",
                schema: "ordering",
                table: "CategoryCaches",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLines_OrderId",
                schema: "ordering",
                table: "OrderLines",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Status",
                schema: "ordering",
                table: "Orders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCaches_Name",
                schema: "ordering",
                table: "ProductCaches",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCaches_Sku",
                schema: "ordering",
                table: "ProductCaches",
                column: "Sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductCaches_UpdatedAtUtc",
                schema: "ordering",
                table: "ProductCaches",
                column: "UpdatedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CategoryCaches",
                schema: "ordering");

            migrationBuilder.DropTable(
                name: "OrderLines",
                schema: "ordering");

            migrationBuilder.DropTable(
                name: "ProductCaches",
                schema: "ordering");

            migrationBuilder.DropTable(
                name: "Orders",
                schema: "ordering");
        }
    }
}
