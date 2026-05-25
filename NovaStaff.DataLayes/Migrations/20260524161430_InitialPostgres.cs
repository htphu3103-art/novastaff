using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NovaStaff.DataLayers.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "EmployeeCodeSequence",
                startValue: 1000L);

            migrationBuilder.CreateTable(
                name: "AuditLog",
                columns: table => new
                {
                    AuditID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TableName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Action = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)0),
                    RecordID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    OldData = table.Column<string>(type: "text", nullable: true),
                    NewData = table.Column<string>(type: "text", nullable: true),
                    ChangedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    IPAddress = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLog", x => x.AuditID);
                    table.CheckConstraint("CK_AuditLog_Action", "\"Action\" IN (0, 1, 2, 3, 4)");
                });

            migrationBuilder.CreateTable(
                name: "ChatChannels",
                columns: table => new
                {
                    ChatChannelID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    CreatedByName = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<int>(type: "integer", nullable: true),
                    ModifiedByName = table.Column<string>(type: "text", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatChannels", x => x.ChatChannelID);
                });

            migrationBuilder.CreateTable(
                name: "PayrollPeriods",
                columns: table => new
                {
                    PeriodID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)1),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedByName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedByName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollPeriods", x => x.PeriodID);
                    table.CheckConstraint("CK_PayrollPeriod_DateRange", "\"EndDate\" >= \"StartDate\"");
                    table.CheckConstraint("CK_PayrollPeriod_Month", "\"Month\" BETWEEN 1 AND 12");
                    table.CheckConstraint("CK_PayrollPeriod_Status", "\"Status\" IN (0, 1, 2, 3, 4)");
                });

            migrationBuilder.CreateTable(
                name: "AttendanceRecords",
                columns: table => new
                {
                    RecordID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeID = table.Column<int>(type: "int", nullable: true),
                    WorkDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_DATE"),
                    CheckIn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CheckOut = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WorkHours = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true, computedColumnSql: "CASE WHEN \"CheckIn\" IS NOT NULL AND \"CheckOut\" IS NOT NULL THEN (EXTRACT(EPOCH FROM (\"CheckOut\" - \"CheckIn\")) / 3600.0)::numeric(5,2) ELSE NULL END", stored: true),
                    Status = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)0),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedByName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedByName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceRecords", x => x.RecordID);
                    table.CheckConstraint("CK_Attendance_ValidTime", "\"CheckIn\" IS NOT NULL OR \"CheckOut\" IS NULL");
                    table.CheckConstraint("CK_AttendanceRecord_Status", "\"Status\" IN (0, 1, 2, 3, 4, 5)");
                });

            migrationBuilder.CreateTable(
                name: "ChatMembers",
                columns: table => new
                {
                    ChatMemberID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChatChannelID = table.Column<int>(type: "integer", nullable: false),
                    UserID = table.Column<int>(type: "integer", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    CreatedByName = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<int>(type: "integer", nullable: true),
                    ModifiedByName = table.Column<string>(type: "text", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMembers", x => x.ChatMemberID);
                    table.ForeignKey(
                        name: "FK_ChatMembers_ChatChannels_ChatChannelID",
                        column: x => x.ChatChannelID,
                        principalTable: "ChatChannels",
                        principalColumn: "ChatChannelID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    ChatMessageID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChatChannelID = table.Column<int>(type: "integer", nullable: false),
                    SenderUserID = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    ReplyToMessageID = table.Column<int>(type: "integer", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    CreatedByName = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<int>(type: "integer", nullable: true),
                    ModifiedByName = table.Column<string>(type: "text", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.ChatMessageID);
                    table.ForeignKey(
                        name: "FK_ChatMessages_ChatChannels_ChatChannelID",
                        column: x => x.ChatChannelID,
                        principalTable: "ChatChannels",
                        principalColumn: "ChatChannelID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChatMessages_ChatMessages_ReplyToMessageID",
                        column: x => x.ReplyToMessageID,
                        principalTable: "ChatMessages",
                        principalColumn: "ChatMessageID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MessageAttachments",
                columns: table => new
                {
                    MessageAttachmentID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChatMessageID = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    CreatedByName = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<int>(type: "integer", nullable: true),
                    ModifiedByName = table.Column<string>(type: "text", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageAttachments", x => x.MessageAttachmentID);
                    table.ForeignKey(
                        name: "FK_MessageAttachments_ChatMessages_ChatMessageID",
                        column: x => x.ChatMessageID,
                        principalTable: "ChatMessages",
                        principalColumn: "ChatMessageID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    DepartmentID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DepartmentName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Tên phòng ban/bộ phận"),
                    Code = table.Column<string>(type: "character varying(20)", unicode: false, maxLength: 20, nullable: true, comment: "Mã định danh phòng ban"),
                    OrgPath = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "Đường dẫn phân cấp phòng ban (Materialized Path)"),
                    OrgLevel = table.Column<short>(type: "smallint", nullable: false, comment: "Cấp bậc phòng ban trong cây"),
                    ManagerEmployeeID = table.Column<int>(type: "int", nullable: true, comment: "ID nhân viên đang giữ chức vụ trưởng phòng"),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedByName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedByName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.DepartmentID);
                    table.CheckConstraint("CK_Department_Code", "\"Code\" IS NULL OR LENGTH(TRIM(\"Code\")) > 0");
                    table.CheckConstraint("CK_Department_Name", "LENGTH(TRIM(\"DepartmentName\")) > 0");
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    EmployeeID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "nextval('\"EmployeeCodeSequence\"')"),
                    FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Gender = table.Column<byte>(type: "smallint", nullable: false),
                    BirthDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    DepartmentID = table.Column<int>(type: "integer", nullable: true),
                    SupervisorID = table.Column<int>(type: "int", nullable: true),
                    Position = table.Column<string>(type: "text", nullable: true),
                    JobLevel = table.Column<int>(type: "integer", nullable: true),
                    BaseSalary = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false, comment: "Lương cơ bản hàng tháng"),
                    JoinDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ContractType = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<byte>(type: "smallint", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedByName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedByName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.EmployeeID);
                    table.CheckConstraint("CK_Employee_BaseSalary", "\"BaseSalary\" >= 0");
                    table.CheckConstraint("CK_Employee_Gender", "\"Gender\" IN (0, 1, 2)");
                    table.CheckConstraint("CK_Employee_Status", "\"Status\" IN (0, 1, 2, 3, 4)");
                    table.ForeignKey(
                        name: "FK_Employees_Departments_DepartmentID",
                        column: x => x.DepartmentID,
                        principalTable: "Departments",
                        principalColumn: "DepartmentID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Employees_Employees_SupervisorID",
                        column: x => x.SupervisorID,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeaveRequests",
                columns: table => new
                {
                    RequestID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeID = table.Column<int>(type: "int", nullable: true),
                    LeaveType = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)0),
                    FromDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ToDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalDays = table.Column<double>(type: "double precision", nullable: false),
                    IsHalfDayStart = table.Column<bool>(type: "boolean", nullable: false),
                    IsHalfDayEnd = table.Column<bool>(type: "boolean", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)1),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedByName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedByName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveRequests", x => x.RequestID);
                    table.CheckConstraint("CK_LeaveRequest_ApprovedBy", "\"ApprovedBy\" IS NULL OR \"Status\" <> 1");
                    table.CheckConstraint("CK_LeaveRequest_ApprovedDate", "\"ApprovedDate\" IS NULL OR \"Status\" <> 1");
                    table.CheckConstraint("CK_LeaveRequest_DateRange", "\"ToDate\" >= \"FromDate\"");
                    table.CheckConstraint("CK_LeaveRequest_LeaveType", "\"LeaveType\" IN (0, 1, 2, 3, 4, 5)");
                    table.CheckConstraint("CK_LeaveRequest_Status", "\"Status\" IN (0, 1, 2, 3)");
                    table.ForeignKey(
                        name: "FK_LeaveRequests_Employees_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PayrollDetails",
                columns: table => new
                {
                    DetailID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PeriodID = table.Column<int>(type: "int", nullable: false),
                    EmployeeID = table.Column<int>(type: "int", nullable: true),
                    BaseSalarySnapshot = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ActualWorkDays = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: false),
                    BonusAndAllowancesJson = table.Column<string>(type: "text", nullable: true),
                    DeductionsJson = table.Column<string>(type: "text", nullable: true),
                    TotalIncome = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    NetSalary = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)1),
                    PaidDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedByName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedByName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollDetails", x => x.DetailID);
                    table.CheckConstraint("CK_PayrollDetail_NetSalary", "\"NetSalary\" >= 0");
                    table.CheckConstraint("CK_PayrollDetail_PaidDate", "\"PaidDate\" IS NULL OR \"Status\" = 4");
                    table.CheckConstraint("CK_PayrollDetail_Status", "\"Status\" IN (0, 1, 2, 3, 4)");
                    table.CheckConstraint("CK_PayrollDetail_TotalIncome", "\"TotalIncome\" >= 0");
                    table.CheckConstraint("CK_PayrollDetail_WorkDays", "\"ActualWorkDays\" >= 0");
                    table.ForeignKey(
                        name: "FK_PayrollDetails_Employees_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PayrollDetails_PayrollPeriods_PeriodID",
                        column: x => x.PeriodID,
                        principalTable: "PayrollPeriods",
                        principalColumn: "PeriodID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeID = table.Column<int>(type: "integer", nullable: false),
                    Username = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    Role = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)3),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    FailedLoginAttempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LockoutEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastLogin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastPasswordChange = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedByName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedByName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                    table.CheckConstraint("CK_User_FailedLogins", "\"FailedLoginAttempts\" >= 0");
                    table.CheckConstraint("CK_User_LockoutEnd", "\"LockoutEnd\" IS NULL OR \"LockoutEnd\" > '2000-01-01'");
                    table.CheckConstraint("CK_User_Role", "\"Role\" IN (0, 1, 2, 3)");
                    table.CheckConstraint("CK_User_Username", "LENGTH(TRIM(\"Username\")) > 0");
                    table.ForeignKey(
                        name: "FK_Users_Employees_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkTasks",
                columns: table => new
                {
                    TaskID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)1),
                    Priority = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)2),
                    EmployeeID = table.Column<int>(type: "integer", nullable: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    CreatedByName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedBy = table.Column<int>(type: "integer", nullable: true),
                    ModifiedByName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkTasks", x => x.TaskID);
                    table.CheckConstraint("CK_WorkTask_Priority", "\"Priority\" IN (0, 1, 2, 3)");
                    table.CheckConstraint("CK_WorkTask_Status", "\"Status\" IN (0, 1, 2, 3)");
                    table.ForeignKey(
                        name: "FK_WorkTasks_Employees_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MessageReactions",
                columns: table => new
                {
                    MessageReactionID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChatMessageID = table.Column<int>(type: "integer", nullable: false),
                    UserID = table.Column<int>(type: "integer", nullable: false),
                    Emoji = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    CreatedByName = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<int>(type: "integer", nullable: true),
                    ModifiedByName = table.Column<string>(type: "text", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageReactions", x => x.MessageReactionID);
                    table.ForeignKey(
                        name: "FK_MessageReactions_ChatMessages_ChatMessageID",
                        column: x => x.ChatMessageID,
                        principalTable: "ChatMessages",
                        principalColumn: "ChatMessageID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MessageReactions_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserID = table.Column<int>(type: "integer", nullable: false),
                    TokenHash = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReplacedByTokenHash = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_EmployeeID_WorkDate",
                table: "AttendanceRecords",
                columns: new[] { "EmployeeID", "WorkDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_WorkDate",
                table: "AttendanceRecords",
                column: "WorkDate");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_ChangedBy",
                table: "AuditLog",
                column: "ChangedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_RecordID",
                table: "AuditLog",
                column: "RecordID");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_Table_Date",
                table: "AuditLog",
                columns: new[] { "TableName", "ChangedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMembers_ChatChannelID_UserID",
                table: "ChatMembers",
                columns: new[] { "ChatChannelID", "UserID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatMembers_UserID",
                table: "ChatMembers",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_ChatChannelID_CreatedDate",
                table: "ChatMessages",
                columns: new[] { "ChatChannelID", "CreatedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_ReplyToMessageID",
                table: "ChatMessages",
                column: "ReplyToMessageID");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_SenderUserID",
                table: "ChatMessages",
                column: "SenderUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Code",
                table: "Departments",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Departments_ManagerEmployeeID",
                table: "Departments",
                column: "ManagerEmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_OrgPath",
                table: "Departments",
                column: "OrgPath",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DepartmentID",
                table: "Employees",
                column: "DepartmentID");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Email",
                table: "Employees",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_EmployeeCode",
                table: "Employees",
                column: "EmployeeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Search_NameStatus",
                table: "Employees",
                columns: new[] { "FullName", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_SupervisorID",
                table: "Employees",
                column: "SupervisorID");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_Employee_Date",
                table: "LeaveRequests",
                columns: new[] { "EmployeeID", "FromDate" });

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_Status",
                table: "LeaveRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MessageAttachments_ChatMessageID",
                table: "MessageAttachments",
                column: "ChatMessageID");

            migrationBuilder.CreateIndex(
                name: "IX_MessageReactions_ChatMessageID_UserID_Emoji",
                table: "MessageReactions",
                columns: new[] { "ChatMessageID", "UserID", "Emoji" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MessageReactions_UserID",
                table: "MessageReactions",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollDetails_Employee_Period_Unique",
                table: "PayrollDetails",
                columns: new[] { "EmployeeID", "PeriodID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollDetails_PeriodID",
                table: "PayrollDetails",
                column: "PeriodID");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollDetails_Status",
                table: "PayrollDetails",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollPeriods_Month_Year",
                table: "PayrollPeriods",
                columns: new[] { "Month", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollPeriods_Status",
                table: "PayrollPeriods",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserID",
                table: "RefreshTokens",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_EmployeeID",
                table: "Users",
                column: "EmployeeID",
                unique: true,
                filter: "\"EmployeeID\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Lock_Optimize",
                table: "Users",
                columns: new[] { "IsLocked", "LockoutEnd", "FailedLoginAttempts" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_LockStatus",
                table: "Users",
                columns: new[] { "IsLocked", "LockoutEnd" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Status_Role",
                table: "Users",
                columns: new[] { "IsLocked", "Role" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkTasks_DueDate",
                table: "WorkTasks",
                column: "DueDate",
                filter: "\"DueDate\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_WorkTasks_Employee_Status",
                table: "WorkTasks",
                columns: new[] { "EmployeeID", "Status" });

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceRecords_Employees_EmployeeID",
                table: "AttendanceRecords",
                column: "EmployeeID",
                principalTable: "Employees",
                principalColumn: "EmployeeID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMembers_Users_UserID",
                table: "ChatMembers",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_Users_SenderUserID",
                table: "ChatMessages",
                column: "SenderUserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Employees_ManagerEmployeeID",
                table: "Departments",
                column: "ManagerEmployeeID",
                principalTable: "Employees",
                principalColumn: "EmployeeID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Employees_ManagerEmployeeID",
                table: "Departments");

            migrationBuilder.DropTable(
                name: "AttendanceRecords");

            migrationBuilder.DropTable(
                name: "AuditLog");

            migrationBuilder.DropTable(
                name: "ChatMembers");

            migrationBuilder.DropTable(
                name: "LeaveRequests");

            migrationBuilder.DropTable(
                name: "MessageAttachments");

            migrationBuilder.DropTable(
                name: "MessageReactions");

            migrationBuilder.DropTable(
                name: "PayrollDetails");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "WorkTasks");

            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "PayrollPeriods");

            migrationBuilder.DropTable(
                name: "ChatChannels");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropSequence(
                name: "EmployeeCodeSequence");
        }
    }
}
