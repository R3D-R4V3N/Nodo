using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rise.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVoiceMessageSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
<<<<<<< HEAD
                name: "Inhoud",
=======
                name: "Text",
>>>>>>> codex/add-alert-message-for-supervisor-monitoring
                table: "Message",
                type: "varchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(2000)",
                oldMaxLength: 2000)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "AudioContentType",
                table: "Message",
                type: "varchar(128)",
                maxLength: 128,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<byte[]>(
                name: "AudioData",
                table: "Message",
                type: "longblob",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "AudioDurationSeconds",
                table: "Message",
                type: "double",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AudioContentType",
                table: "Message");

            migrationBuilder.DropColumn(
                name: "AudioData",
                table: "Message");

            migrationBuilder.DropColumn(
                name: "AudioDurationSeconds",
                table: "Message");

            migrationBuilder.AlterColumn<string>(
                name: "Inhoud",
                table: "Message",
                type: "varchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: string.Empty,
                oldClrType: typeof(string),
                oldType: "varchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
