using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaStaff.DataLayers.Migrations
{
    /// <inheritdoc />
    public partial class FixLeaveRequestCheckConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_LeaveRequest_ApprovedBy",
                table: "LeaveRequests");

            migrationBuilder.DropCheckConstraint(
                name: "CK_LeaveRequest_ApprovedDate",
                table: "LeaveRequests");

            migrationBuilder.AddCheckConstraint(
                name: "CK_LeaveRequest_ApprovedBy",
                table: "LeaveRequests",
                sql: "ApprovedBy IS NULL OR Status <> 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_LeaveRequest_ApprovedDate",
                table: "LeaveRequests",
                sql: "ApprovedDate IS NULL OR Status <> 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_LeaveRequest_ApprovedBy",
                table: "LeaveRequests");

            migrationBuilder.DropCheckConstraint(
                name: "CK_LeaveRequest_ApprovedDate",
                table: "LeaveRequests");

            migrationBuilder.AddCheckConstraint(
                name: "CK_LeaveRequest_ApprovedBy",
                table: "LeaveRequests",
                sql: "ApprovedBy IS NULL OR Status IN (1, 2)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_LeaveRequest_ApprovedDate",
                table: "LeaveRequests",
                sql: "ApprovedDate IS NULL OR Status IN (1, 2)");
        }
    }
}
