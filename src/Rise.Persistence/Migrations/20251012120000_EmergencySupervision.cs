using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rise.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EmergencySupervision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EmergencyActivatedAtUtc",
                table: "Chat",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmergencyInitiatorId",
                table: "Chat",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEmergencyActive",
                table: "Chat",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ChatParticipant",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ChatId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "current_timestamp()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "current_timestamp()"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatParticipant", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatParticipant_ApplicationUser_UserId",
                        column: x => x.UserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChatParticipant_Chat_ChatId",
                        column: x => x.ChatId,
                        principalTable: "Chat",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserSupervisor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ChatUserId = table.Column<int>(type: "int", nullable: false),
                    SupervisorId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "current_timestamp()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "current_timestamp()"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSupervisor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSupervisor_ApplicationUser_ChatUserId",
                        column: x => x.ChatUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserSupervisor_ApplicationUser_SupervisorId",
                        column: x => x.SupervisorId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ChatParticipant_ChatId",
                table: "ChatParticipant",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatParticipant_UserId",
                table: "ChatParticipant",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatParticipant_ChatId_UserId",
                table: "ChatParticipant",
                columns: new[] { "ChatId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSupervisor_ChatUserId",
                table: "UserSupervisor",
                column: "ChatUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSupervisor_SupervisorId",
                table: "UserSupervisor",
                column: "SupervisorId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSupervisor_ChatUserId_SupervisorId",
                table: "UserSupervisor",
                columns: new[] { "ChatUserId", "SupervisorId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatParticipant");

            migrationBuilder.DropTable(
                name: "UserSupervisor");

            migrationBuilder.DropColumn(
                name: "EmergencyActivatedAtUtc",
                table: "Chat");

            migrationBuilder.DropColumn(
                name: "EmergencyInitiatorId",
                table: "Chat");

            migrationBuilder.DropColumn(
                name: "IsEmergencyActive",
                table: "Chat");
        }
    }
}
