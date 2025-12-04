using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rise.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EventPriceToDouble : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                oldDefaultValue: new DateTime(2025, 11, 24, 17, 7, 31, 284, DateTimeKind.Utc).AddTicks(8970));

            migrationBuilder.AlterColumn<double>(
                name: "Price",
                table: "Event",
                type: "double",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(32)",
                oldMaxLength: 32)
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "HandledDate",
                table: "RegistrationRequests",
                type: "datetime(6)",
                nullable: true,
                defaultValue: new DateTime(2025, 11, 24, 17, 7, 31, 284, DateTimeKind.Utc).AddTicks(8970),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true,
                oldDefaultValue: new DateTime(2025, 11, 24, 18, 53, 29, 58, DateTimeKind.Utc).AddTicks(4190));

            migrationBuilder.AlterColumn<string>(
                name: "Price",
                table: "Event",
                type: "varchar(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double",
                oldPrecision: 18,
                oldScale: 2)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
