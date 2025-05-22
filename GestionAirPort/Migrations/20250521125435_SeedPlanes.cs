using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GestionAirPort.Migrations
{
    /// <inheritdoc />
    public partial class SeedPlanes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "MyPlanes",
                columns: new[] { "PlaneId", "PlaneCapacity", "ManufactureDate", "PlaneType" },
                values: new object[,]
                {
                    { 1, 150, new DateTime(2015, 5, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Commercial" },
                    { 2, 20, new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Private" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MyPlanes",
                keyColumn: "PlaneId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "MyPlanes",
                keyColumn: "PlaneId",
                keyValue: 2);
        }
    }
}
