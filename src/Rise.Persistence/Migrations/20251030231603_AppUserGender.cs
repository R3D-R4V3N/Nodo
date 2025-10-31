using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rise.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AppUserGender : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE `ApplicationUser`
                SET `Gender` = '0'
                WHERE LOWER(`Gender`) = 'x';

                UPDATE `ApplicationUser`
                SET `Gender` = '1'
                WHERE LOWER(`Gender`) IN ('man', 'm', 'male');

                UPDATE `ApplicationUser`
                SET `Gender` = '2'
                WHERE LOWER(`Gender`) IN ('woman', 'vrouw', 'female', 'f');
                """);

            migrationBuilder.AlterColumn<string>(
                name: "TextSuggestion",
                table: "UserSettingChatTextLineSuggestions",
                type: "varchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldMaxLength: 200)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "Gender",
                table: "ApplicationUser",
                type: "int",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldMaxLength: 10)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "AvatarUrl",
                table: "ApplicationUser",
                type: "varchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(250)",
                oldMaxLength: 250)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TextSuggestion",
                table: "UserSettingChatTextLineSuggestions",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(150)",
                oldMaxLength: 150)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                table: "ApplicationUser",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldMaxLength: 10)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql(
                """
                UPDATE `ApplicationUser`
                SET `Gender` = 'x'
                WHERE `Gender` = '0';

                UPDATE `ApplicationUser`
                SET `Gender` = 'man'
                WHERE `Gender` = '1';

                UPDATE `ApplicationUser`
                SET `Gender` = 'vrouw'
                WHERE `Gender` = '2';
                """);

            migrationBuilder.AlterColumn<string>(
                name: "AvatarUrl",
                table: "ApplicationUser",
                type: "varchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(150)",
                oldMaxLength: 150)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
