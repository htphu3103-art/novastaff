using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaStaff.DataLayers.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRefreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Token",
                table: "RefreshTokens",
                newName: "TokenHash");

            migrationBuilder.RenameColumn(
                name: "ReplacedByToken",
                table: "RefreshTokens",
                newName: "ReplacedByTokenHash");

            migrationBuilder.AddColumn<DateTime>(
                name: "RevokedAt",
                table: "RefreshTokens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE [RefreshTokens]
                SET [RevokedAt] = SYSUTCDATETIME()
                WHERE [IsRevoked] = 1 AND [RevokedAt] IS NULL
                """);

            migrationBuilder.Sql(
                """
                UPDATE [RefreshTokens]
                SET [TokenHash] = CONVERT(varchar(64), HASHBYTES('SHA2_256', CONVERT(varchar(max), [TokenHash])), 2)
                WHERE LEN([TokenHash]) <> 64
                """);

            migrationBuilder.Sql(
                """
                UPDATE [RefreshTokens]
                SET [ReplacedByTokenHash] = CONVERT(varchar(64), HASHBYTES('SHA2_256', CONVERT(varchar(max), [ReplacedByTokenHash])), 2)
                WHERE [ReplacedByTokenHash] IS NOT NULL
                    AND LEN([ReplacedByTokenHash]) <> 64
                """);

            migrationBuilder.DropColumn(
                name: "IsRevoked",
                table: "RefreshTokens");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TokenHash",
                table: "RefreshTokens",
                newName: "Token");

            migrationBuilder.RenameColumn(
                name: "ReplacedByTokenHash",
                table: "RefreshTokens",
                newName: "ReplacedByToken");

            migrationBuilder.AddColumn<bool>(
                name: "IsRevoked",
                table: "RefreshTokens",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(
                """
                UPDATE [RefreshTokens]
                SET [IsRevoked] = 1
                WHERE [RevokedAt] IS NOT NULL
                """);

            migrationBuilder.DropColumn(
                name: "RevokedAt",
                table: "RefreshTokens");
        }
    }
}
