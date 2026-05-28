using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaStaff.DataLayers.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserEmployeeDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Employees_EmployeeID",
                table: "Users");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Employees_EmployeeID",
                table: "Users",
                column: "EmployeeID",
                principalTable: "Employees",
                principalColumn: "EmployeeID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Employees_EmployeeID",
                table: "Users");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Employees_EmployeeID",
                table: "Users",
                column: "EmployeeID",
                principalTable: "Employees",
                principalColumn: "EmployeeID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
