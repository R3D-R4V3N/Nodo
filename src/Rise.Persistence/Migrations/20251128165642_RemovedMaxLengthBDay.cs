using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rise.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemovedMaxLengthBDay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "HandledDate",
                table: "RegistrationRequests",
                type: "datetime(6)",
                nullable: true,
                defaultValue: new DateTime(2025, 11, 28, 16, 56, 41, 726, DateTimeKind.Utc).AddTicks(7310),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true,
                oldDefaultValue: new DateTime(2025, 11, 24, 18, 53, 29, 58, DateTimeKind.Utc).AddTicks(4190));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "HandledDate",
                table: "RegistrationRequests",
                type: "datetime(6)",
                nullable: true,
                defaultValue: new DateTime(2025, 11, 24, 18, 53, 29, 58, DateTimeKind.Utc).AddTicks(4190),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true,
                oldDefaultValue: new DateTime(2025, 11, 28, 16, 56, 41, 726, DateTimeKind.Utc).AddTicks(7310));
        }
    }
}
