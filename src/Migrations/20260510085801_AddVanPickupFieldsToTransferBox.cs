using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PuppetFestAPP.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddVanPickupFieldsToTransferBox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PickedUpAt",
                table: "StockTransferBoxes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PickedUpByDriverName",
                table: "StockTransferBoxes",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PickedUpByVanId",
                table: "StockTransferBoxes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockTransferBoxes_PickedUpByVanId",
                table: "StockTransferBoxes",
                column: "PickedUpByVanId");

            migrationBuilder.AddForeignKey(
                name: "FK_StockTransferBoxes_Locations_PickedUpByVanId",
                table: "StockTransferBoxes",
                column: "PickedUpByVanId",
                principalTable: "Locations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockTransferBoxes_Locations_PickedUpByVanId",
                table: "StockTransferBoxes");

            migrationBuilder.DropIndex(
                name: "IX_StockTransferBoxes_PickedUpByVanId",
                table: "StockTransferBoxes");

            migrationBuilder.DropColumn(
                name: "PickedUpAt",
                table: "StockTransferBoxes");

            migrationBuilder.DropColumn(
                name: "PickedUpByDriverName",
                table: "StockTransferBoxes");

            migrationBuilder.DropColumn(
                name: "PickedUpByVanId",
                table: "StockTransferBoxes");
        }
    }
}
