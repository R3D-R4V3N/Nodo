using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rise.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Organization",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Location = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "current_timestamp()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "current_timestamp()"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organization", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Organization",
                columns: new[] { "Id", "Location", "Name" },
                values: new object[,]
                {
                    { 1, "Antwerpen", "Nodo Antwerpen" },
                    { 2, "Gent", "Nodo Gent" },
                    { 3, "Brussel", "Nodo Brussel" }
                });

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "BaseUsers",
                type: "int",
                nullable: true);

            migrationBuilder.Sql("UPDATE BaseUsers SET OrganizationId = 1");

            migrationBuilder.AlterColumn<int>(
                name: "OrganizationId",
                table: "BaseUsers",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BaseUsers_OrganizationId",
                table: "BaseUsers",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_BaseUsers_Organization_OrganizationId",
                table: "BaseUsers",
                column: "OrganizationId",
                principalTable: "Organization",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BaseUsers_Organization_OrganizationId",
                table: "BaseUsers");

            migrationBuilder.DropIndex(
                name: "IX_BaseUsers_OrganizationId",
                table: "BaseUsers");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "BaseUsers");

            migrationBuilder.DropTable(
                name: "Organization");
        }
    }
}
