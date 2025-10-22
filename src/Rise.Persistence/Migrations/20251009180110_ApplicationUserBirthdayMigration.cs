<<<<<<< HEAD
﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;
=======
﻿using Microsoft.EntityFrameworkCore.Migrations;
>>>>>>> codex/add-alert-message-for-supervisor-monitoring

#nullable disable

namespace Rise.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ApplicationUserBirthdayMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "BirthDay",
                table: "ApplicationUser",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BirthDay",
                table: "ApplicationUser");
        }
    }
}
