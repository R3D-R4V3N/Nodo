using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rise.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UC_CompKeyWithIsDeleted_removedUQ : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserConnections_Users_FromId",
                table: "UserConnections");

            migrationBuilder.DropIndex(
                name: "IX_UserConnections_FromId_ToId_ConnectionType",
                table: "UserConnections");

            migrationBuilder.AlterColumn<string>(
                name: "ConnectionType",
                table: "UserConnections",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_UserConnections_FromId_ToId_IsDeleted",
                table: "UserConnections",
                columns: new[] { "FromId", "ToId", "IsDeleted" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserConnections_Users_FromId",
                table: "UserConnections",
                column: "FromId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserConnections_Users_FromId",
                table: "UserConnections");

            migrationBuilder.DropIndex(
                name: "IX_UserConnections_FromId_ToId_IsDeleted",
                table: "UserConnections");

            migrationBuilder.AlterColumn<string>(
                name: "ConnectionType",
                table: "UserConnections",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_UserConnections_FromId_ToId_ConnectionType",
                table: "UserConnections",
                columns: new[] { "FromId", "ToId", "ConnectionType" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserConnections_Users_FromId",
                table: "UserConnections",
                column: "FromId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
