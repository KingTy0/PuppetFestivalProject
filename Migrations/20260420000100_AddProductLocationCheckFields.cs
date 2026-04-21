using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PuppetFestAPP.Web.Data;

#nullable disable

namespace PuppetFestAPP.Web.Migrations
{
	[DbContext(typeof(ApplicationDbContext))]
	[Migration("20260420000100_AddProductLocationCheckFields")]
	public partial class AddProductLocationCheckFields : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<bool>(
				name: "IsBoxChecked",
				table: "ProductLocations",
				type: "INTEGER",
				nullable: false,
				defaultValue: false);

			migrationBuilder.AddColumn<DateTime>(
				name: "BoxCheckedAt",
				table: "ProductLocations",
				type: "TEXT",
				nullable: true);

			migrationBuilder.AddColumn<bool>(
				name: "IsDeliveryChecked",
				table: "ProductLocations",
				type: "INTEGER",
				nullable: false,
				defaultValue: false);

			migrationBuilder.AddColumn<DateTime>(
				name: "DeliveryCheckedAt",
				table: "ProductLocations",
				type: "TEXT",
				nullable: true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "IsBoxChecked",
				table: "ProductLocations");

			migrationBuilder.DropColumn(
				name: "BoxCheckedAt",
				table: "ProductLocations");

			migrationBuilder.DropColumn(
				name: "IsDeliveryChecked",
				table: "ProductLocations");

			migrationBuilder.DropColumn(
				name: "DeliveryCheckedAt",
				table: "ProductLocations");
		}
	}
}
