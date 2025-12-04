using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rise.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BaseMessageHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                oldDefaultValue: new DateTime(2025, 11, 24, 18, 53, 29, 58, DateTimeKind.Utc).AddTicks(4190));

            migrationBuilder.CreateTable(
                name: "MessageHistoryItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LastReadMessageId = table.Column<int>(type: "int", nullable: false),
                    ChatId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageHistoryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageHistoryItems_BaseUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "BaseUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MessageHistoryItems_Chat_ChatId",
                        column: x => x.ChatId,
                        principalTable: "Chat",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MessageHistoryItems_Message_LastReadMessageId",
                        column: x => x.LastReadMessageId,
                        principalTable: "Message",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_MessageHistoryItems_ChatId",
                table: "MessageHistoryItems",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageHistoryItems_LastReadMessageId",
                table: "MessageHistoryItems",
                column: "LastReadMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageHistoryItems_UserId",
                table: "MessageHistoryItems",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MessageHistoryItems");

            migrationBuilder.AlterColumn<DateTime>(
                name: "HandledDate",
                table: "RegistrationRequests",
                type: "datetime(6)",
                nullable: true,
                defaultValue: new DateTime(2025, 11, 24, 18, 53, 29, 58, DateTimeKind.Utc).AddTicks(4190),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true,
                oldDefaultValue: new DateTime(2025, 11, 26, 21, 14, 18, 761, DateTimeKind.Utc).AddTicks(2875));
        }
    }
}
