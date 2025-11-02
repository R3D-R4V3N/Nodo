using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rise.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSupervisorRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SupervisorId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE Users u
                SET SupervisorId = (
                    SELECT s.Id
                    FROM Supervisors s
                    WHERE s.OrganizationId = u.OrganizationId
                    ORDER BY s.Id
                    LIMIT 1
                )
                WHERE SupervisorId IS NULL;
            ");

            migrationBuilder.AlterColumn<int>(
                name: "SupervisorId",
                table: "Users",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_SupervisorId",
                table: "Users",
                column: "SupervisorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Supervisors_SupervisorId",
                table: "Users",
                column: "SupervisorId",
                principalTable: "Supervisors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Supervisors_SupervisorId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_SupervisorId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SupervisorId",
                table: "Users");
        }
    }
}
