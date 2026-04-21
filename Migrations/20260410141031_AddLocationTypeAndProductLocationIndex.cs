using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PuppetFestAPP.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationTypeAndProductLocationIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductLocations_ProductId",
                table: "ProductLocations");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Locations",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Locations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "StockTransferBoxes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FromLocationId = table.Column<int>(type: "INTEGER", nullable: false),
                    ToLocationId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockTransferBoxes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockTransferBoxes_Locations_FromLocationId",
                        column: x => x.FromLocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StockTransferBoxes_Locations_ToLocationId",
                        column: x => x.ToLocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockTransferBoxItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StockTransferBoxId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockTransferBoxItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockTransferBoxItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StockTransferBoxItems_StockTransferBoxes_StockTransferBoxId",
                        column: x => x.StockTransferBoxId,
                        principalTable: "StockTransferBoxes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductLocations_ProductId_LocationId",
                table: "ProductLocations",
                columns: new[] { "ProductId", "LocationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockTransferBoxes_FromLocationId",
                table: "StockTransferBoxes",
                column: "FromLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransferBoxes_ToLocationId",
                table: "StockTransferBoxes",
                column: "ToLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransferBoxItems_ProductId",
                table: "StockTransferBoxItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransferBoxItems_StockTransferBoxId",
                table: "StockTransferBoxItems",
                column: "StockTransferBoxId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockTransferBoxItems");

            migrationBuilder.DropTable(
                name: "StockTransferBoxes");

            migrationBuilder.DropIndex(
                name: "IX_ProductLocations_ProductId_LocationId",
                table: "ProductLocations");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Locations");

            migrationBuilder.CreateIndex(
                name: "IX_ProductLocations_ProductId",
                table: "ProductLocations",
                column: "ProductId");
        }
    }
}
