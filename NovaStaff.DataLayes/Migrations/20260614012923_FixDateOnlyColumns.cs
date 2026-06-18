using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaStaff.DataLayers.Migrations
{
    /// <inheritdoc />
    public partial class FixDateOnlyColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ===== PayrollPeriods =====
            migrationBuilder.AlterColumn<DateOnly>(
                name: "StartDate",
                table: "PayrollPeriods",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "EndDate",
                table: "PayrollPeriods",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            // ===== LeaveRequests =====
            migrationBuilder.AlterColumn<DateOnly>(
                name: "ToDate",
                table: "LeaveRequests",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "FromDate",
                table: "LeaveRequests",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            // ===== Employees =====
            migrationBuilder.AlterColumn<DateOnly>(
                name: "JoinDate",
                table: "Employees",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "BirthDate",
                table: "Employees",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            // ===== AttendanceRecords =====
            migrationBuilder.AlterColumn<DateOnly>(
                name: "WorkDate",
                table: "AttendanceRecords",
                type: "date",
                nullable: false,
                defaultValueSql: "CURRENT_DATE",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_DATE");

            // Bước 1: Xóa WorkHours trước (vì nó phụ thuộc CheckIn/CheckOut)
            migrationBuilder.DropColumn(
                name: "WorkHours",
                table: "AttendanceRecords");

            // Bước 2: Đổi kiểu CheckIn / CheckOut
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CheckOut",
                table: "AttendanceRecords",
                type: "timestamptz",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CheckIn",
                table: "AttendanceRecords",
                type: "timestamptz",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            // Bước 3: Tạo lại WorkHours
            migrationBuilder.AddColumn<decimal>(
                name: "WorkHours",
                table: "AttendanceRecords",
                type: "numeric(5,2)",
                nullable: true,
                computedColumnSql: "CASE WHEN \"CheckIn\" IS NOT NULL AND \"CheckOut\" IS NOT NULL " +
                                   "THEN (EXTRACT(EPOCH FROM (\"CheckOut\" - \"CheckIn\")) / 3600.0)::numeric(5,2) " +
                                   "ELSE NULL END",
                stored: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ===== PayrollPeriods =====
            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "PayrollPeriods",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDate",
                table: "PayrollPeriods",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            // ===== LeaveRequests =====
            migrationBuilder.AlterColumn<DateTime>(
                name: "ToDate",
                table: "LeaveRequests",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FromDate",
                table: "LeaveRequests",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            // ===== Employees =====
            migrationBuilder.AlterColumn<DateTime>(
                name: "JoinDate",
                table: "Employees",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "BirthDate",
                table: "Employees",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            // ===== AttendanceRecords =====
            migrationBuilder.AlterColumn<DateTime>(
                name: "WorkDate",
                table: "AttendanceRecords",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_DATE",
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldDefaultValueSql: "CURRENT_DATE");

            // Bước 1: Xóa WorkHours
            migrationBuilder.DropColumn(
                name: "WorkHours",
                table: "AttendanceRecords");

            // Bước 2: Revert CheckIn / CheckOut
            migrationBuilder.AlterColumn<DateTime>(
                name: "CheckOut",
                table: "AttendanceRecords",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamptz",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CheckIn",
                table: "AttendanceRecords",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamptz",
                oldNullable: true);

            // Bước 3: Tạo lại WorkHours với kiểu cũ
            migrationBuilder.AddColumn<decimal>(
                name: "WorkHours",
                table: "AttendanceRecords",
                type: "numeric(5,2)",
                nullable: true,
                computedColumnSql: "CASE WHEN \"CheckIn\" IS NOT NULL AND \"CheckOut\" IS NOT NULL " +
                                   "THEN (EXTRACT(EPOCH FROM (\"CheckOut\" - \"CheckIn\")) / 3600.0)::numeric(5,2) " +
                                   "ELSE NULL END",
                stored: true);
        }
    }
}