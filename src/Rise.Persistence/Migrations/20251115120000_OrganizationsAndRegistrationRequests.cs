using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Rise.Persistence;

#nullable disable

namespace Rise.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20251115120000_OrganizationsAndRegistrationRequests")]
    public partial class OrganizationsAndRegistrationRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "current_timestamp()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "current_timestamp()"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql(
                "INSERT INTO `Organizations` (`Id`, `Name`, `Description`, `CreatedAt`, `UpdatedAt`, `IsDeleted`) " +
                "VALUES (1, 'Onbekende organisatie', NULL, '2024-01-01 00:00:00', '2024-01-01 00:00:00', 0);");

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "Supervisors",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SupervisorId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.Sql("UPDATE `Supervisors` SET `OrganizationId` = 1 WHERE `OrganizationId` = 0;");

            migrationBuilder.Sql("UPDATE `Users` SET `OrganizationId` = 1 WHERE `OrganizationId` = 0;");

            migrationBuilder.CreateTable(
                name: "UserRegistrationRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AccountId = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FullName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProcessedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    DecisionNote = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    AssignedSupervisorId = table.Column<int>(type: "int", nullable: true),
                    ProcessedBySupervisorId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "current_timestamp()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "current_timestamp()"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRegistrationRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRegistrationRequests_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRegistrationRequests_Supervisors_AssignedSupervisorId",
                        column: x => x.AssignedSupervisorId,
                        principalTable: "Supervisors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UserRegistrationRequests_Supervisors_ProcessedBySupervisorId",
                        column: x => x.ProcessedBySupervisorId,
                        principalTable: "Supervisors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Supervisors_OrganizationId",
                table: "Supervisors",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_OrganizationId",
                table: "Users",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_SupervisorId",
                table: "Users",
                column: "SupervisorId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRegistrationRequests_AccountId",
                table: "UserRegistrationRequests",
                column: "AccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRegistrationRequests_AssignedSupervisorId",
                table: "UserRegistrationRequests",
                column: "AssignedSupervisorId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRegistrationRequests_OrganizationId",
                table: "UserRegistrationRequests",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRegistrationRequests_ProcessedBySupervisorId",
                table: "UserRegistrationRequests",
                column: "ProcessedBySupervisorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Supervisors_Organizations_OrganizationId",
                table: "Supervisors",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Organizations_OrganizationId",
                table: "Users",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Supervisors_SupervisorId",
                table: "Users",
                column: "SupervisorId",
                principalTable: "Supervisors",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Supervisors_Organizations_OrganizationId",
                table: "Supervisors");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Organizations_OrganizationId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Supervisors_SupervisorId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "UserRegistrationRequests");

            migrationBuilder.DropTable(
                name: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Supervisors_OrganizationId",
                table: "Supervisors");

            migrationBuilder.DropIndex(
                name: "IX_Users_OrganizationId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_SupervisorId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SupervisorId",
                table: "Users");
        }
    }
}
