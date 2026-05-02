using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PuppetFestAPP.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddIsPickedUpToTransferBox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPickedUp",
                table: "StockTransferBoxes",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPickedUp",
                table: "StockTransferBoxes");
        }
    }
}
