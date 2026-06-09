using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaStaff.DataLayers.Migrations
{
    /// <inheritdoc />
    public partial class FixEmployeeStatusConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Employee_Status",
                table: "Employees");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Employee_Status",
                table: "Employees",
                sql: "\"Status\" IN (1, 2, 3, 4, 5, 6, 7)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Employee_Status",
                table: "Employees");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Employee_Status",
                table: "Employees",
                sql: "\"Status\" IN (0, 1, 2, 3, 4)");
        }
    }
}
