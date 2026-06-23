using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaStaff.DataLayers.Migrations
{
    /// <inheritdoc />
    public partial class AddTerminationDateToEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "TerminationDate",
                table: "Employees",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TerminationDate",
                table: "Employees");
        }
    }
}
