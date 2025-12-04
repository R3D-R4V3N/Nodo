using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rise.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CleanedUpRegistration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RegistrationRequests_Supervisors_ApprovedBySupervisorId",
                table: "RegistrationRequests");

            migrationBuilder.DropIndex(
                name: "IX_RegistrationRequests_NormalizedEmail",
                table: "RegistrationRequests");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "RegistrationRequests");

            migrationBuilder.DropColumn(
                name: "BirthDate",
                table: "RegistrationRequests");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "RegistrationRequests");

            migrationBuilder.DropColumn(
                name: "NormalizedEmail",
                table: "RegistrationRequests");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "RegistrationRequests");

            migrationBuilder.RenameColumn(
                name: "DeniedReason",
                table: "RegistrationRequests",
                newName: "Note");

            migrationBuilder.RenameColumn(
                name: "ApprovedBySupervisorId",
                table: "RegistrationRequests",
                newName: "HandledById");

            migrationBuilder.RenameIndex(
                name: "IX_RegistrationRequests_ApprovedBySupervisorId",
                table: "RegistrationRequests",
                newName: "IX_RegistrationRequests_HandledById");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "RegistrationRequests",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldMaxLength: 200)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "RegistrationRequests",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldMaxLength: 200)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "RegistrationRequests",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldMaxLength: 200)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "AvatarUrl",
                table: "RegistrationRequests",
                type: "varchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldMaxLength: 200)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateOnly>(
                name: "BirthDay",
                table: "RegistrationRequests",
                type: "date",
                maxLength: 255,
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<DateTime>(
                name: "HandledDate",
                table: "RegistrationRequests",
                type: "datetime(6)",
                nullable: true,
                defaultValue: new DateTime(2025, 11, 20, 16, 26, 30, 728, DateTimeKind.Utc).AddTicks(1772));

            migrationBuilder.AddColumn<int>(
                name: "StatusType",
                table: "RegistrationRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "AvatarUrl",
                table: "BaseUsers",
                type: "varchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldMaxLength: 500000)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrationRequests_Supervisors_HandledById",
                table: "RegistrationRequests",
                column: "HandledById",
                principalTable: "Supervisors",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RegistrationRequests_Supervisors_HandledById",
                table: "RegistrationRequests");

            migrationBuilder.DropColumn(
                name: "BirthDay",
                table: "RegistrationRequests");

            migrationBuilder.DropColumn(
                name: "HandledDate",
                table: "RegistrationRequests");

            migrationBuilder.DropColumn(
                name: "StatusType",
                table: "RegistrationRequests");

            migrationBuilder.RenameColumn(
                name: "Note",
                table: "RegistrationRequests",
                newName: "DeniedReason");

            migrationBuilder.RenameColumn(
                name: "HandledById",
                table: "RegistrationRequests",
                newName: "ApprovedBySupervisorId");

            migrationBuilder.RenameIndex(
                name: "IX_RegistrationRequests_HandledById",
                table: "RegistrationRequests",
                newName: "IX_RegistrationRequests_ApprovedBySupervisorId");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "RegistrationRequests",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "RegistrationRequests",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "RegistrationRequests",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 255)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "AvatarUrl",
                table: "RegistrationRequests",
                type: "longtext",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "RegistrationRequests",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "BirthDate",
                table: "RegistrationRequests",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "RegistrationRequests",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedEmail",
                table: "RegistrationRequests",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "RegistrationRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "AvatarUrl",
                table: "BaseUsers",
                type: "longtext",
                maxLength: 500000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationRequests_NormalizedEmail",
                table: "RegistrationRequests",
                column: "NormalizedEmail",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrationRequests_Supervisors_ApprovedBySupervisorId",
                table: "RegistrationRequests",
                column: "ApprovedBySupervisorId",
                principalTable: "Supervisors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
