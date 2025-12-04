using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rise.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ForgotToAddChatToDomainItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "HandledDate",
                table: "RegistrationRequests",
                type: "datetime(6)",
                nullable: true,
                defaultValue: new DateTime(2025, 11, 26, 21, 35, 7, 616, DateTimeKind.Utc).AddTicks(3407),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true,
                oldDefaultValue: new DateTime(2025, 11, 26, 21, 14, 18, 761, DateTimeKind.Utc).AddTicks(2875));

            migrationBuilder.AlterColumn<int>(
                name: "ChatId",
                table: "MessageHistoryItems",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "HandledDate",
                table: "RegistrationRequests",
                type: "datetime(6)",
                nullable: true,
                defaultValue: new DateTime(2025, 11, 26, 21, 14, 18, 761, DateTimeKind.Utc).AddTicks(2875),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true,
                oldDefaultValue: new DateTime(2025, 11, 26, 21, 35, 7, 616, DateTimeKind.Utc).AddTicks(3407));

            migrationBuilder.AlterColumn<int>(
                name: "ChatId",
                table: "MessageHistoryItems",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
