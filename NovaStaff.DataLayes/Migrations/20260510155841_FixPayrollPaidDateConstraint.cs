using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaStaff.DataLayers.Migrations
{
    /// <inheritdoc />
    public partial class FixPayrollPaidDateConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_PayrollDetail_PaidDate",
                table: "PayrollDetails");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PayrollDetail_PaidDate",
                table: "PayrollDetails",
                sql: "PaidDate IS NULL OR Status = 4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_PayrollDetail_PaidDate",
                table: "PayrollDetails");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PayrollDetail_PaidDate",
                table: "PayrollDetails",
                sql: "PaidDate IS NULL OR Status = 2");
        }
    }
}
